using System.Globalization;
using EasyWrite.App.Controls;
using EasyWrite.App.Presentation;
using EasyWrite.App.Theme;
using EasyWrite.App.Views;
using EasyWrite.Core.Events;
using EasyWrite.Core.Models;
using EasyWrite.Core.Services;

namespace EasyWrite.App;

public sealed class MainForm : Form
{
    private readonly TextToSpeechView _textToSpeechView;
    private readonly SpeechToTextView _speechToTextView;
    private readonly OperationStatusControl _operationStatusControl;
    private readonly TabControl _workspaceTabs;
    private readonly ITextToSpeechService? _textToSpeechService;
    private readonly ISpeechToTextService? _speechToTextService;
    private readonly ITextExportService? _textExportService;
    private readonly TranscriptSession _transcriptSession = new();
    private CancellationTokenSource? _exportCancellationSource;
    private SpeechOperationState _operationState = SpeechOperationState.Idle;
    private bool _canSynthesize;
    private bool _canRecognize;
    private bool _canExport;
    private bool _exportInProgress;
    private bool _recognitionOwnsOperation;
    private bool _shutdownStarted;

    public MainForm(
        ITextToSpeechService? textToSpeechService = null,
        ISpeechToTextService? speechToTextService = null,
        ITextExportService? textExportService = null,
        string? synthesisInitializationError = null,
        string? recognitionInitializationError = null,
        string? exportInitializationError = null)
    {
        _textToSpeechService = textToSpeechService;
        _speechToTextService = speechToTextService;
        _textExportService = textExportService;
        _canExport = textExportService is not null;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = EasyWriteTheme.WindowBackground;
        ClientSize = new Size(1180, 760);
        DoubleBuffered = true;
        Font = EasyWriteTheme.BodyFont();
        ForeColor = EasyWriteTheme.TextPrimary;
        KeyPreview = true;
        MinimumSize = new Size(960, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EasyWrite — Speech Studio";
        AccessibleDescription =
            "A local-first desktop workspace for text-to-speech and speech-to-text.";
        AccessibleName = "EasyWrite Speech Studio";

        _textToSpeechView = new TextToSpeechView();
        _speechToTextView = new SpeechToTextView();
        _operationStatusControl = new OperationStatusControl();
        _workspaceTabs = CreateWorkspaceTabs();

        _textToSpeechView.ContentAvailabilityChanged += HandleContentAvailabilityChanged;
        _textToSpeechView.SpeakRequested += HandleSpeakRequested;
        _textToSpeechView.PauseResumeRequested += HandlePauseResumeRequested;
        _textToSpeechView.StopRequested += HandleStopRequested;
        _speechToTextView.ContentAvailabilityChanged += HandleContentAvailabilityChanged;
        _speechToTextView.StartListeningRequested += HandleStartListeningRequested;
        _speechToTextView.StopListeningRequested += HandleStopListeningRequested;
        _speechToTextView.TranscriptClearRequested += HandleTranscriptClearRequested;
        _speechToTextView.ExportRequested += HandleExportRequested;
        _workspaceTabs.SelectedIndexChanged += (_, _) => RenderState();
        FormClosing += HandleFormClosing;

        if (_textToSpeechService is not null)
        {
            _textToSpeechService.StateChanged += HandleSpeechStateChanged;
        }

        if (_speechToTextService is not null)
        {
            _speechToTextService.StateChanged += HandleRecognitionStateChanged;
            _speechToTextService.ResultRecognized += HandleResultRecognized;
        }

        var shell = new TableLayoutPanel
        {
            BackColor = EasyWriteTheme.WindowBackground,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            RowCount = 3
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        shell.Controls.Add(CreateHeader(), 0, 0);
        shell.Controls.Add(_workspaceTabs, 0, 1);
        shell.Controls.Add(_operationStatusControl, 0, 2);
        Controls.Add(shell);

        LoadInstalledVoices(synthesisInitializationError);
        LoadInstalledRecognizers(recognitionInitializationError);
        ShowExportInitializationError(exportInitializationError);
        RenderState();
    }

    private static TableLayoutPanel CreateHeader()
    {
        var header = new TableLayoutPanel
        {
            BackColor = EasyWriteTheme.Surface,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = new Padding(32, 17, 32, 13),
            RowCount = 1
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));

        var identity = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            RowCount = 2
        };
        identity.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        identity.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        identity.Controls.Add(new Label
        {
            AutoSize = true,
            Font = EasyWriteTheme.HeadingFont(21F),
            ForeColor = EasyWriteTheme.TextPrimary,
            Margin = Padding.Empty,
            Text = "EasyWrite"
        });
        identity.Controls.Add(new Label
        {
            AutoSize = true,
            Font = EasyWriteTheme.BodyFont(9.5F),
            ForeColor = EasyWriteTheme.TextSecondary,
            Margin = new Padding(2, 3, 0, 0),
            Text = "Write it. Hear it. Say it. See it."
        });

        var studioBadge = new Label
        {
            Anchor = AnchorStyles.Right,
            AutoSize = true,
            BackColor = EasyWriteTheme.PrimarySoft,
            Font = EasyWriteTheme.SubheadingFont(9.5F),
            ForeColor = EasyWriteTheme.PrimaryDark,
            Margin = Padding.Empty,
            Padding = new Padding(14, 8, 14, 8),
            Text = "Speech Studio",
            TextAlign = ContentAlignment.MiddleCenter
        };

        header.Controls.Add(identity, 0, 0);
        header.Controls.Add(studioBadge, 1, 0);
        return header;
    }

    private TabControl CreateWorkspaceTabs()
    {
        var tabs = new TabControl
        {
            AccessibleDescription = "Choose between the text-to-speech and speech-to-text workspaces.",
            AccessibleName = "Speech workspace navigation",
            Alignment = TabAlignment.Top,
            Appearance = TabAppearance.Normal,
            Dock = DockStyle.Fill,
            DrawMode = TabDrawMode.OwnerDrawFixed,
            Font = EasyWriteTheme.SubheadingFont(10F),
            ItemSize = new Size(210, 42),
            Margin = Padding.Empty,
            Padding = new Point(22, 6),
            SizeMode = TabSizeMode.Fixed,
            TabIndex = 0
        };

        var textToSpeechPage = new TabPage
        {
            AccessibleName = "Text to Speech tab",
            BackColor = EasyWriteTheme.WindowBackground,
            Padding = Padding.Empty,
            Text = "Text to Speech",
            UseVisualStyleBackColor = false
        };
        textToSpeechPage.Controls.Add(_textToSpeechView);

        var speechToTextPage = new TabPage
        {
            AccessibleName = "Speech to Text tab",
            BackColor = EasyWriteTheme.WindowBackground,
            Padding = Padding.Empty,
            Text = "Speech to Text",
            UseVisualStyleBackColor = false
        };
        speechToTextPage.Controls.Add(_speechToTextView);

        tabs.TabPages.Add(textToSpeechPage);
        tabs.TabPages.Add(speechToTextPage);
        tabs.DrawItem += DrawWorkspaceTab;
        return tabs;
    }

    private void DrawWorkspaceTab(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _workspaceTabs.TabPages.Count)
        {
            return;
        }

        var isSelected = e.Index == _workspaceTabs.SelectedIndex;
        var background = isSelected ? EasyWriteTheme.PrimarySoft : EasyWriteTheme.Surface;
        var foreground = isSelected ? EasyWriteTheme.PrimaryDark : EasyWriteTheme.TextSecondary;
        using var backgroundBrush = new SolidBrush(background);
        using var foregroundBrush = new SolidBrush(foreground);
        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

        var textBounds = new Rectangle(
            e.Bounds.X + 8,
            e.Bounds.Y + 2,
            e.Bounds.Width - 16,
            e.Bounds.Height - 4);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        e.Graphics.DrawString(
            _workspaceTabs.TabPages[e.Index].Text,
            _workspaceTabs.Font,
            foregroundBrush,
            textBounds,
            format);

        if (isSelected)
        {
            using var accentPen = new Pen(EasyWriteTheme.Primary, 3F);
            e.Graphics.DrawLine(
                accentPen,
                e.Bounds.Left + 12,
                e.Bounds.Bottom - 2,
                e.Bounds.Right - 12,
                e.Bounds.Bottom - 2);
        }
    }

    private void HandleContentAvailabilityChanged(object? sender, EventArgs e) => RenderState();

    private async void HandleSpeakRequested(object? sender, EventArgs e)
    {
        if (_shutdownStarted || _textToSpeechService is null)
        {
            return;
        }

        if (!_textToSpeechView.HasText)
        {
            _textToSpeechView.ShowCapabilityMessage(
                "Enter some text before starting speech.",
                isError: true);
            return;
        }

        if (string.IsNullOrWhiteSpace(_textToSpeechView.SelectedVoiceName))
        {
            _textToSpeechView.ShowCapabilityMessage(
                "Choose an installed Windows voice before starting speech.",
                isError: true);
            return;
        }

        var settings = new SpeechSettings
        {
            VoiceName = _textToSpeechView.SelectedVoiceName,
            Rate = _textToSpeechView.Rate,
            Volume = _textToSpeechView.Volume
        };

        try
        {
            await _textToSpeechService.SpeakAsync(_textToSpeechView.EnteredText, settings);
        }
        catch (OperationCanceledException)
        {
            // Stop and shutdown are expected cancellation paths.
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ShowPlaybackError(exception.Message);
        }
        catch (Exception)
        {
            ShowPlaybackError(
                "Speech playback failed unexpectedly. Check Windows speech and audio settings, then try again.");
        }
    }

    private void HandlePauseResumeRequested(object? sender, EventArgs e)
    {
        if (_shutdownStarted || _textToSpeechService is null)
        {
            return;
        }

        try
        {
            if (_operationState == SpeechOperationState.Paused)
            {
                _textToSpeechService.ResumeSpeech();
            }
            else if (_operationState == SpeechOperationState.Speaking)
            {
                _textToSpeechService.PauseSpeech();
            }
        }
        catch (ObjectDisposedException)
        {
            // Shutdown won the race with this UI command.
        }
        catch (InvalidOperationException exception)
        {
            ShowPlaybackError(exception.Message);
        }
    }

    private void HandleStopRequested(object? sender, EventArgs e)
    {
        if (_shutdownStarted || _textToSpeechService is null)
        {
            return;
        }

        try
        {
            _textToSpeechService.StopSpeech();
        }
        catch (ObjectDisposedException)
        {
            // Shutdown won the race with this UI command.
        }
        catch (InvalidOperationException exception)
        {
            ShowPlaybackError(exception.Message);
        }
    }

    private async void HandleStartListeningRequested(object? sender, EventArgs e)
    {
        if (_shutdownStarted || _speechToTextService is null)
        {
            return;
        }

        var culture = _speechToTextView.SelectedRecognizerCulture;
        if (culture is null)
        {
            _speechToTextView.ShowCapabilityMessage(
                "Choose an installed recognition language before starting the microphone.",
                isError: true);
            return;
        }

        try
        {
            await _speechToTextService.StartListeningAsync(culture);
        }
        catch (OperationCanceledException)
        {
            // Shutdown and explicit cancellation are expected control flow.
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ShowRecognitionError(exception.Message);
        }
        catch (Exception)
        {
            ShowRecognitionError(
                "Microphone recognition failed unexpectedly. Check Windows microphone and speech-language settings.");
        }
    }

    private async void HandleStopListeningRequested(object? sender, EventArgs e)
    {
        if (_shutdownStarted || _speechToTextService is null)
        {
            return;
        }

        try
        {
            await _speechToTextService.StopListeningAsync();
        }
        catch (OperationCanceledException)
        {
            // Shutdown and repeated stop paths may cancel the pending stop task.
        }
        catch (ObjectDisposedException)
        {
            // Shutdown won the race with this UI command.
        }
        catch (InvalidOperationException exception)
        {
            ShowRecognitionError(exception.Message);
        }
    }

    private void HandleSpeechStateChanged(object? sender, SpeechStateChangedEventArgs e)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(() => ApplySpeechState(e));
            }
            catch (InvalidOperationException)
            {
                // The window handle was destroyed while the callback was queued.
            }

            return;
        }

        ApplySpeechState(e);
    }

    private void ApplySpeechState(SpeechStateChangedEventArgs e)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        _operationState = e.CurrentState;
        _recognitionOwnsOperation = false;
        _textToSpeechView.ShowCapabilityMessage(
            e.StatusMessage,
            e.CurrentState == SpeechOperationState.Error);
        RenderState();
    }

    private void HandleRecognitionStateChanged(object? sender, SpeechStateChangedEventArgs e)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(() => ApplyRecognitionState(e));
            }
            catch (InvalidOperationException)
            {
                // The window handle was destroyed while the callback was queued.
            }

            return;
        }

        ApplyRecognitionState(e);
    }

    private void ApplyRecognitionState(SpeechStateChangedEventArgs e)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        _operationState = e.CurrentState;
        _recognitionOwnsOperation = e.CurrentState is
            SpeechOperationState.Listening or SpeechOperationState.Stopping;
        _speechToTextView.ShowCapabilityMessage(
            e.StatusMessage,
            e.CurrentState == SpeechOperationState.Error);
        RenderState();
    }

    private void HandleResultRecognized(object? sender, SpeechResultEventArgs e)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(() => AppendRecognitionResult(e.Result));
            }
            catch (InvalidOperationException)
            {
                // The window handle was destroyed while the callback was queued.
            }

            return;
        }

        AppendRecognitionResult(e.Result);
    }

    private void AppendRecognitionResult(SpeechResult result)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(result.Text))
        {
            return;
        }

        _transcriptSession.Add(result);
        _speechToTextView.AppendResult(result);
        RenderState();
    }

    private void HandleTranscriptClearRequested(object? sender, EventArgs e)
    {
        if (_shutdownStarted || _exportInProgress)
        {
            return;
        }

        _transcriptSession.Clear();
        _speechToTextView.ClearTranscript();
        RenderState();
    }

    private async void HandleExportRequested(object? sender, EventArgs e)
    {
        if (_shutdownStarted ||
            _exportInProgress ||
            _textExportService is null ||
            !_transcriptSession.HasResults)
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "txt",
            FileName = $"EasyWrite-Transcript-{DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.txt",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            OverwritePrompt = true,
            RestoreDirectory = true,
            Title = "Export EasyWrite transcript"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var results = _transcriptSession.CreateSnapshot();
        using var cancellationSource = new CancellationTokenSource();
        _exportCancellationSource = cancellationSource;
        _exportInProgress = true;
        _speechToTextView.SetExportInProgress(true);
        RenderState();

        try
        {
            await _textExportService.ExportAsync(
                dialog.FileName,
                results,
                cancellationSource.Token);

            if (!_shutdownStarted && !IsDisposed && !Disposing)
            {
                _speechToTextView.ShowCapabilityMessage("Transcript exported successfully.");
            }
        }
        catch (OperationCanceledException)
        {
            // Dialog cancellation and form shutdown are not export errors.
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ShowExportError(exception.Message);
        }
        catch (Exception)
        {
            ShowExportError(
                "The transcript could not be saved. Choose another location and try again.");
        }
        finally
        {
            if (ReferenceEquals(_exportCancellationSource, cancellationSource))
            {
                _exportCancellationSource = null;
            }

            _exportInProgress = false;
            if (!_shutdownStarted && !IsDisposed && !Disposing)
            {
                _speechToTextView.SetExportInProgress(false);
                RenderState();
            }
        }
    }

    private void LoadInstalledVoices(string? initializationError)
    {
        if (_textToSpeechService is null)
        {
            _textToSpeechView.SetVoices([]);
            _textToSpeechView.ShowCapabilityMessage(
                initializationError ??
                "Speech playback is unavailable. Restart EasyWrite after checking Windows speech settings.",
                isError: true);
            return;
        }

        try
        {
            var voices = _textToSpeechService.GetInstalledVoices();
            _textToSpeechView.SetVoices(voices);
            _canSynthesize = voices.Any(voice => voice.IsEnabled);
            _textToSpeechView.ShowCapabilityMessage(
                _canSynthesize
                    ? "Ready. Enter text, choose a voice, and select Speak."
                    : "No enabled Windows voices were found. Install a speech voice in Windows Settings and restart EasyWrite.",
                isError: !_canSynthesize);
        }
        catch (Exception exception) when (exception is InvalidOperationException or ObjectDisposedException)
        {
            _canSynthesize = false;
            _textToSpeechView.SetVoices([]);
            _textToSpeechView.ShowCapabilityMessage(exception.Message, isError: true);
        }
    }

    private void LoadInstalledRecognizers(string? initializationError)
    {
        if (_speechToTextService is null)
        {
            _speechToTextView.SetRecognizerCultures([]);
            _speechToTextView.ShowCapabilityMessage(
                initializationError ??
                "Microphone recognition is unavailable. Install a Windows speech language and restart EasyWrite.",
                isError: true);
            return;
        }

        try
        {
            var cultures = _speechToTextService.GetInstalledRecognizerCultures();
            _speechToTextView.SetRecognizerCultures(cultures);
            _canRecognize = cultures.Count > 0;
            _speechToTextView.ShowCapabilityMessage(
                _canRecognize
                    ? "Ready. Choose a recognition language and select Start listening."
                    : "No compatible Windows speech recognizer was found. Install a speech language in Windows Settings and restart EasyWrite.",
                isError: !_canRecognize);
        }
        catch (Exception exception) when (exception is InvalidOperationException or ObjectDisposedException)
        {
            _canRecognize = false;
            _speechToTextView.SetRecognizerCultures([]);
            _speechToTextView.ShowCapabilityMessage(exception.Message, isError: true);
        }
    }

    private void ShowExportInitializationError(string? initializationError)
    {
        if (_canExport || string.IsNullOrWhiteSpace(initializationError))
        {
            return;
        }

        _speechToTextView.ShowCapabilityMessage(initializationError, isError: true);
    }

    private void ShowPlaybackError(string message)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        _textToSpeechView.ShowCapabilityMessage(message, isError: true);
        RenderState();
    }

    private void ShowRecognitionError(string message)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        _speechToTextView.ShowCapabilityMessage(message, isError: true);
        RenderState();
    }

    private void ShowExportError(string message)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        _speechToTextView.ShowCapabilityMessage(message, isError: true);
    }

    private void HandleFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_shutdownStarted)
        {
            return;
        }

        _shutdownStarted = true;
        _textToSpeechView.SpeakRequested -= HandleSpeakRequested;
        _textToSpeechView.PauseResumeRequested -= HandlePauseResumeRequested;
        _textToSpeechView.StopRequested -= HandleStopRequested;
        _speechToTextView.StartListeningRequested -= HandleStartListeningRequested;
        _speechToTextView.StopListeningRequested -= HandleStopListeningRequested;
        _speechToTextView.TranscriptClearRequested -= HandleTranscriptClearRequested;
        _speechToTextView.ExportRequested -= HandleExportRequested;
        _exportCancellationSource?.Cancel();

        if (_textToSpeechService is not null)
        {
            _textToSpeechService.StateChanged -= HandleSpeechStateChanged;
            try
            {
                _textToSpeechService.StopSpeech();
            }
            catch (Exception exception) when (exception is InvalidOperationException or ObjectDisposedException)
            {
                // Program disposes the service immediately after the message loop exits.
            }
        }

        if (_speechToTextService is not null)
        {
            _speechToTextService.StateChanged -= HandleRecognitionStateChanged;
            _speechToTextService.ResultRecognized -= HandleResultRecognized;
            try
            {
                _ = ObserveShutdownStopAsync(_speechToTextService.StopListeningAsync());
            }
            catch (Exception exception) when (exception is InvalidOperationException or ObjectDisposedException)
            {
                // Program disposes the service immediately after the message loop exits.
            }
        }
    }

    private void RenderState()
    {
        var context = new SpeechUiContext(
            _operationState,
            _textToSpeechView.HasText,
            _transcriptSession.HasResults,
            CanSynthesize: _canSynthesize,
            CanRecognize: _canRecognize,
            CanExport: _canExport,
            ExportInProgress: _exportInProgress);
        var controlState = SpeechControlStateMapper.Map(context);

        _textToSpeechView.ApplyState(controlState);
        _speechToTextView.ApplyState(
            controlState,
            _recognitionOwnsOperation ? _operationState : SpeechOperationState.Idle);
        _operationStatusControl.ApplyState(
            _operationState,
            controlState.StatusText,
            controlState.AccessibleStatus);
    }

    private static async Task ObserveShutdownStopAsync(Task stopTask)
    {
        try
        {
            await stopTask;
        }
        catch (Exception exception) when (exception is OperationCanceledException or InvalidOperationException)
        {
            // Program immediately disposes the service and no UI remains for feedback.
        }
    }
}
