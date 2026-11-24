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
    private SpeechOperationState _operationState = SpeechOperationState.Idle;
    private bool _canSynthesize;
    private bool _shutdownStarted;

    public MainForm(
        ITextToSpeechService? textToSpeechService = null,
        string? initializationError = null)
    {
        _textToSpeechService = textToSpeechService;
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
        _workspaceTabs.SelectedIndexChanged += (_, _) => RenderState();
        FormClosing += HandleFormClosing;

        if (_textToSpeechService is not null)
        {
            _textToSpeechService.StateChanged += HandleSpeechStateChanged;
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

        LoadInstalledVoices(initializationError);
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
        _textToSpeechView.ShowCapabilityMessage(
            e.StatusMessage,
            e.CurrentState == SpeechOperationState.Error);
        RenderState();
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

    private void ShowPlaybackError(string message)
    {
        if (_shutdownStarted || IsDisposed || Disposing)
        {
            return;
        }

        _textToSpeechView.ShowCapabilityMessage(message, isError: true);
        RenderState();
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

        if (_textToSpeechService is null)
        {
            return;
        }

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

    private void RenderState()
    {
        var context = new SpeechUiContext(
            _operationState,
            _textToSpeechView.HasText,
            _speechToTextView.HasTranscript,
            CanSynthesize: _canSynthesize,
            CanRecognize: false);
        var controlState = SpeechControlStateMapper.Map(context);

        _textToSpeechView.ApplyState(controlState);
        _speechToTextView.ApplyState(controlState);
        _operationStatusControl.ApplyState(
            _operationState,
            controlState.StatusText,
            controlState.AccessibleStatus);
    }
}
