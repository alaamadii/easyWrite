using EasyWrite.App.Controls;
using EasyWrite.App.Presentation;
using EasyWrite.App.Theme;

namespace EasyWrite.App.Views;

internal sealed class SpeechToTextView : UserControl
{
    private readonly ComboBox _languageComboBox;
    private readonly Label _microphoneStateLabel;
    private readonly Panel _microphoneIndicator;
    private readonly Button _startListeningButton;
    private readonly Button _stopListeningButton;
    private readonly RichTextBox _transcriptBox;
    private readonly Label _transcriptPlaceholderLabel;
    private readonly Label _resultCountLabel;
    private readonly Button _clearTranscriptButton;
    private readonly Button _exportButton;
    private readonly Label _capabilityMessageLabel;

    public SpeechToTextView()
    {
        BackColor = EasyWriteTheme.WindowBackground;
        Dock = DockStyle.Fill;
        Padding = new Padding(28, 22, 28, 24);
        AccessibleName = "Speech to Text workspace";

        _languageComboBox = new ComboBox
        {
            AccessibleDescription = "Recognizer discovery will be connected in Phase 03.",
            AccessibleName = "Recognition language",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDown,
            Font = EasyWriteTheme.BodyFont(),
            IntegralHeight = false,
            TabIndex = 0,
            Text = "Available when microphone recognition is connected"
        };

        _microphoneIndicator = new Panel
        {
            AccessibleDescription = "The microphone is off.",
            AccessibleName = "Microphone status indicator",
            BackColor = EasyWriteTheme.Disabled,
            Margin = new Padding(0, 5, 10, 0),
            Size = new Size(12, 12),
            TabStop = false
        };

        _microphoneStateLabel = new Label
        {
            AutoSize = true,
            Font = EasyWriteTheme.SubheadingFont(9F),
            ForeColor = EasyWriteTheme.TextSecondary,
            Margin = new Padding(0, 2, 0, 0),
            Text = "Microphone is off"
        };

        _startListeningButton = EasyWriteTheme.CreatePrimaryButton("Start &listening", 1);
        _startListeningButton.MinimumSize = new Size(168, 40);
        _startListeningButton.Width = 168;
        _startListeningButton.AccessibleName = "Start listening";
        _startListeningButton.AccessibleDescription =
            "Unavailable until microphone recognition is connected in Phase 03.";

        _stopListeningButton = EasyWriteTheme.CreateDangerButton("S&top listening", 2);
        _stopListeningButton.MinimumSize = new Size(168, 40);
        _stopListeningButton.Width = 168;
        _stopListeningButton.AccessibleName = "Stop listening";

        _transcriptBox = new RichTextBox
        {
            AccessibleDescription = "Finalized recognized speech will appear here in Phase 03.",
            AccessibleName = "Recognition transcript",
            BackColor = EasyWriteTheme.SurfaceMuted,
            BorderStyle = BorderStyle.FixedSingle,
            DetectUrls = false,
            Dock = DockStyle.Fill,
            Font = EasyWriteTheme.BodyFont(10.5F),
            ForeColor = EasyWriteTheme.TextPrimary,
            Margin = Padding.Empty,
            ReadOnly = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            TabIndex = 3
        };
        _transcriptPlaceholderLabel = CreateTranscriptPlaceholder();
        _transcriptBox.Controls.Add(_transcriptPlaceholderLabel);
        _transcriptBox.TextChanged += HandleTranscriptChanged;

        _resultCountLabel = CreateMetadataLabel("0 results");
        _clearTranscriptButton = EasyWriteTheme.CreateSecondaryButton("&Clear transcript", 4);
        _clearTranscriptButton.MinimumSize = new Size(148, 40);
        _clearTranscriptButton.AccessibleName = "Clear transcript";
        _clearTranscriptButton.Click += (_, _) => ClearTranscript();

        _exportButton = EasyWriteTheme.CreateSecondaryButton("&Export TXT…", 5);
        _exportButton.MinimumSize = new Size(136, 40);
        _exportButton.AccessibleName = "Export transcript as a text file";
        _exportButton.AccessibleDescription = "Export will be connected in Phase 04.";

        _capabilityMessageLabel = new Label
        {
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Font = EasyWriteTheme.BodyFont(9F),
            ForeColor = EasyWriteTheme.TextSecondary,
            Margin = new Padding(0, 6, 0, 0),
            Text = "Microphone recognition will be connected in Phase 03. No audio is being captured.",
            TextAlign = ContentAlignment.MiddleLeft
        };

        Controls.Add(CreateCard());
        UpdateTranscriptMetadata();
    }

    public event EventHandler? ContentAvailabilityChanged;

    public bool HasTranscript => !string.IsNullOrWhiteSpace(_transcriptBox.Text);

    public void ApplyState(SpeechControlState state)
    {
        _languageComboBox.Enabled = state.RecognizerSelectionEnabled;
        _startListeningButton.Enabled = state.StartListeningEnabled;
        _stopListeningButton.Enabled = state.StopListeningEnabled;
        _clearTranscriptButton.Enabled = state.ClearTranscriptEnabled;
        _exportButton.Enabled = state.ExportEnabled;

        var isListening = state.StopListeningEnabled;
        _microphoneIndicator.BackColor = isListening
            ? EasyWriteTheme.Danger
            : EasyWriteTheme.Disabled;
        _microphoneStateLabel.Text = isListening ? "Microphone is listening" : "Microphone is off";
        _microphoneIndicator.AccessibleDescription = isListening
            ? "The microphone is active."
            : "The microphone is off.";
    }

    private CardPanel CreateCard()
    {
        var card = new CardPanel { Dock = DockStyle.Fill };
        var layout = new TableLayoutPanel
        {
            BackColor = EasyWriteTheme.Surface,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            RowCount = 9
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 2F));

        layout.Controls.Add(CreateTitleLabel("Speech to Text"), 0, 0);
        layout.Controls.Add(
            CreateDescriptionLabel("Dictate through your microphone using an installed Windows recognizer."),
            0,
            1);
        layout.Controls.Add(CreateRecognizerLayout(), 0, 2);
        layout.Controls.Add(CreateListeningActions(), 0, 3);
        layout.Controls.Add(CreateTranscriptHeader(), 0, 4);
        layout.Controls.Add(_transcriptBox, 0, 5);
        layout.Controls.Add(CreateTranscriptActions(), 0, 6);
        layout.Controls.Add(_capabilityMessageLabel, 0, 7);
        card.Controls.Add(layout);
        return card;
    }

    private TableLayoutPanel CreateRecognizerLayout()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 14, 0, 2),
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));

        var languageLabel = new Label
        {
            AutoSize = true,
            Font = EasyWriteTheme.SubheadingFont(8.75F),
            ForeColor = EasyWriteTheme.TextSecondary,
            Margin = Padding.Empty,
            Text = "Recognition language"
        };

        var microphoneGroup = new FlowLayoutPanel
        {
            Anchor = AnchorStyles.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 7, 0, 0),
            WrapContents = false
        };
        microphoneGroup.Controls.Add(_microphoneIndicator);
        microphoneGroup.Controls.Add(_microphoneStateLabel);

        layout.Controls.Add(languageLabel, 0, 0);
        layout.SetColumnSpan(microphoneGroup, 2);
        layout.Controls.Add(_languageComboBox, 0, 1);
        layout.Controls.Add(microphoneGroup, 1, 1);
        return layout;
    }

    private FlowLayoutPanel CreateListeningActions()
    {
        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 4, 0, 4),
            WrapContents = false
        };
        actions.Controls.Add(_startListeningButton);
        actions.Controls.Add(_stopListeningButton);
        return actions;
    }

    private TableLayoutPanel CreateTranscriptHeader()
    {
        var header = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 8),
            RowCount = 1
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

        var label = new Label
        {
            AutoSize = true,
            Font = EasyWriteTheme.SubheadingFont(9.5F),
            ForeColor = EasyWriteTheme.TextPrimary,
            Text = "Transcript"
        };
        _resultCountLabel.Anchor = AnchorStyles.Right;
        header.Controls.Add(label, 0, 0);
        header.Controls.Add(_resultCountLabel, 1, 0);
        return header;
    }

    private TableLayoutPanel CreateTranscriptActions()
    {
        var actions = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 5, 0, 5),
            RowCount = 1
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _clearTranscriptButton.Anchor = AnchorStyles.Left;
        _exportButton.Anchor = AnchorStyles.Right;
        actions.Controls.Add(_clearTranscriptButton, 0, 0);
        actions.Controls.Add(_exportButton, 1, 0);
        return actions;
    }

    private Label CreateTranscriptPlaceholder()
    {
        var label = new Label
        {
            AutoSize = true,
            BackColor = EasyWriteTheme.SurfaceMuted,
            Cursor = Cursors.IBeam,
            Font = EasyWriteTheme.BodyFont(10.5F),
            ForeColor = EasyWriteTheme.Disabled,
            Location = new Point(10, 10),
            Text = "Recognized speech will appear here after microphone support is connected."
        };
        label.Click += (_, _) => _transcriptBox.Focus();
        return label;
    }

    private static Label CreateTitleLabel(string text) => new()
    {
        AutoSize = true,
        Font = EasyWriteTheme.HeadingFont(19F),
        ForeColor = EasyWriteTheme.TextPrimary,
        Margin = Padding.Empty,
        Text = text
    };

    private static Label CreateDescriptionLabel(string text) => new()
    {
        AutoSize = true,
        Font = EasyWriteTheme.BodyFont(10F),
        ForeColor = EasyWriteTheme.TextSecondary,
        Margin = new Padding(0, 6, 0, 0),
        Text = text
    };

    private static Label CreateMetadataLabel(string text) => new()
    {
        AutoSize = true,
        Font = EasyWriteTheme.BodyFont(9F),
        ForeColor = EasyWriteTheme.TextSecondary,
        Text = text,
        TextAlign = ContentAlignment.MiddleRight
    };

    private void HandleTranscriptChanged(object? sender, EventArgs e)
    {
        UpdateTranscriptMetadata();
        ContentAvailabilityChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateTranscriptMetadata()
    {
        var resultCount = _transcriptBox.Lines.Count(line => !string.IsNullOrWhiteSpace(line));
        _resultCountLabel.Text = resultCount == 1 ? "1 result" : $"{resultCount:N0} results";
        _transcriptPlaceholderLabel.Visible = _transcriptBox.TextLength == 0;
    }

    private void ClearTranscript()
    {
        _transcriptBox.Clear();
        _transcriptBox.Focus();
    }
}
