using System.Globalization;
using EasyWrite.App.Controls;
using EasyWrite.App.Presentation;
using EasyWrite.App.Theme;

namespace EasyWrite.App.Views;

internal sealed class TextToSpeechView : UserControl
{
    private readonly RichTextBox _textEditor;
    private readonly Label _placeholderLabel;
    private readonly Label _characterCountLabel;
    private readonly ComboBox _voiceComboBox;
    private readonly TrackBar _rateTrackBar;
    private readonly Label _rateValueLabel;
    private readonly TrackBar _volumeTrackBar;
    private readonly Label _volumeValueLabel;
    private readonly Button _speakButton;
    private readonly Button _pauseResumeButton;
    private readonly Button _stopButton;
    private readonly Button _clearButton;
    private readonly Label _capabilityMessageLabel;

    public TextToSpeechView()
    {
        BackColor = EasyWriteTheme.WindowBackground;
        Dock = DockStyle.Fill;
        Padding = new Padding(28, 22, 28, 24);
        AccessibleName = "Text to Speech workspace";

        _textEditor = CreateTextEditor();
        _placeholderLabel = CreatePlaceholderLabel();
        _textEditor.Controls.Add(_placeholderLabel);
        _textEditor.TextChanged += HandleTextChanged;

        _characterCountLabel = CreateMetadataLabel("0 characters");
        _voiceComboBox = CreateUnavailableComboBox(
            "Available when speech playback is connected",
            "Installed voice selection",
            1);

        _rateValueLabel = CreateValueLabel("0");
        _rateTrackBar = CreateTrackBar(-10, 10, 0, 2, "Speech rate");
        _rateTrackBar.ValueChanged += (_, _) =>
            _rateValueLabel.Text = _rateTrackBar.Value.ToString(CultureInfo.CurrentCulture);

        _volumeValueLabel = CreateValueLabel("100");
        _volumeTrackBar = CreateTrackBar(0, 100, 100, 3, "Speech volume");
        _volumeTrackBar.TickFrequency = 10;
        _volumeTrackBar.ValueChanged += (_, _) =>
            _volumeValueLabel.Text = _volumeTrackBar.Value.ToString(CultureInfo.CurrentCulture);

        _speakButton = EasyWriteTheme.CreatePrimaryButton("&Speak", 4);
        _speakButton.AccessibleName = "Speak entered text";
        _speakButton.AccessibleDescription = "Unavailable until text-to-speech is connected in Phase 02.";

        _pauseResumeButton = EasyWriteTheme.CreateSecondaryButton("&Pause", 5);
        _pauseResumeButton.AccessibleName = "Pause or resume speech";

        _stopButton = EasyWriteTheme.CreateDangerButton("S&top", 6);
        _stopButton.AccessibleName = "Stop speech";

        _clearButton = EasyWriteTheme.CreateSecondaryButton("&Clear text", 7);
        _clearButton.AccessibleName = "Clear text editor";
        _clearButton.Click += (_, _) => ClearText();

        _capabilityMessageLabel = new Label
        {
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Font = EasyWriteTheme.BodyFont(9F),
            ForeColor = EasyWriteTheme.TextSecondary,
            Margin = new Padding(0, 6, 0, 0),
            Text = "Speech playback will be connected in Phase 02. You can prepare text and settings now.",
            TextAlign = ContentAlignment.MiddleLeft
        };

        Controls.Add(CreateCard());
        UpdateTextMetadata();
    }

    public event EventHandler? ContentAvailabilityChanged;

    public bool HasText => !string.IsNullOrWhiteSpace(_textEditor.Text);

    public void ApplyState(SpeechControlState state)
    {
        _textEditor.Enabled = state.TextEditorEnabled;
        _rateTrackBar.Enabled = state.SpeechSettingsEnabled;
        _volumeTrackBar.Enabled = state.SpeechSettingsEnabled;
        _voiceComboBox.Enabled = state.VoiceSelectionEnabled;
        _speakButton.Enabled = state.SpeakEnabled;
        _pauseResumeButton.Enabled = state.PauseResumeEnabled;
        _pauseResumeButton.Text = state.PauseResumeText;
        _stopButton.Enabled = state.StopSpeechEnabled;
        _clearButton.Enabled = state.ClearTextEnabled;
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
            RowCount = 8
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 2F));

        layout.Controls.Add(CreateTitleLabel("Text to Speech"), 0, 0);
        layout.Controls.Add(
            CreateDescriptionLabel("Turn written text into audio using an installed Windows voice."),
            0,
            1);
        layout.Controls.Add(CreateEditorHeader(), 0, 2);
        layout.Controls.Add(_textEditor, 0, 3);
        layout.Controls.Add(CreateSettingsLayout(), 0, 4);
        layout.Controls.Add(CreateActionLayout(), 0, 5);
        layout.Controls.Add(_capabilityMessageLabel, 0, 6);
        card.Controls.Add(layout);
        return card;
    }

    private TableLayoutPanel CreateEditorHeader()
    {
        var header = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 18, 0, 8),
            RowCount = 1
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

        var label = new Label
        {
            AutoSize = true,
            Font = EasyWriteTheme.SubheadingFont(9.5F),
            ForeColor = EasyWriteTheme.TextPrimary,
            Text = "Text to speak"
        };
        _characterCountLabel.Anchor = AnchorStyles.Right;
        header.Controls.Add(label, 0, 0);
        header.Controls.Add(_characterCountLabel, 1, 0);
        return header;
    }

    private TableLayoutPanel CreateSettingsLayout()
    {
        var settings = new TableLayoutPanel
        {
            ColumnCount = 9,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 14, 0, 6),
            RowCount = 2
        };

        settings.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 22F));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38F));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54F));

        settings.Controls.Add(CreateSettingLabel("Voice"), 0, 0);
        settings.Controls.Add(CreateSettingLabel("Rate"), 3, 0);
        settings.Controls.Add(CreateSettingLabel("Volume"), 6, 0);
        settings.Controls.Add(_voiceComboBox, 1, 1);
        settings.Controls.Add(_rateTrackBar, 4, 1);
        settings.Controls.Add(_rateValueLabel, 5, 1);
        settings.Controls.Add(_volumeTrackBar, 7, 1);
        settings.Controls.Add(_volumeValueLabel, 8, 1);
        return settings;
    }

    private TableLayoutPanel CreateActionLayout()
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

        var primaryActions = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = Padding.Empty,
            WrapContents = false
        };
        primaryActions.Controls.Add(_speakButton);
        primaryActions.Controls.Add(_pauseResumeButton);
        primaryActions.Controls.Add(_stopButton);

        _clearButton.Anchor = AnchorStyles.Right;
        actions.Controls.Add(primaryActions, 0, 0);
        actions.Controls.Add(_clearButton, 1, 0);
        return actions;
    }

    private static RichTextBox CreateTextEditor() => new()
    {
        AcceptsTab = true,
        AccessibleDescription = "Enter or paste the text that will later be spoken.",
        AccessibleName = "Text to speak",
        BackColor = EasyWriteTheme.SurfaceMuted,
        BorderStyle = BorderStyle.FixedSingle,
        DetectUrls = false,
        Dock = DockStyle.Fill,
        Font = EasyWriteTheme.BodyFont(11F),
        ForeColor = EasyWriteTheme.TextPrimary,
        Margin = Padding.Empty,
        Padding = new Padding(8),
        ScrollBars = RichTextBoxScrollBars.Vertical,
        TabIndex = 0
    };

    private Label CreatePlaceholderLabel()
    {
        var label = new Label
        {
            AutoSize = true,
            BackColor = EasyWriteTheme.SurfaceMuted,
            Cursor = Cursors.IBeam,
            Font = EasyWriteTheme.BodyFont(11F),
            ForeColor = EasyWriteTheme.Disabled,
            Location = new Point(10, 10),
            Text = "Type or paste text here…"
        };
        label.Click += (_, _) => _textEditor.Focus();
        return label;
    }

    private static ComboBox CreateUnavailableComboBox(
        string message,
        string accessibleName,
        int tabIndex) => new()
        {
            AccessibleDescription = "This capability is not connected in the current phase.",
            AccessibleName = accessibleName,
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDown,
            Font = EasyWriteTheme.BodyFont(),
            IntegralHeight = false,
            Margin = new Padding(0, 4, 0, 0),
            TabIndex = tabIndex,
            Text = message
        };

    private static TrackBar CreateTrackBar(
        int minimum,
        int maximum,
        int value,
        int tabIndex,
        string accessibleName) => new()
        {
            AccessibleName = accessibleName,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 38,
            LargeChange = 2,
            Margin = new Padding(0, 1, 0, 0),
            Maximum = maximum,
            Minimum = minimum,
            SmallChange = 1,
            TabIndex = tabIndex,
            TickFrequency = 2,
            Value = value
        };

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

    private static Label CreateSettingLabel(string text) => new()
    {
        AutoSize = true,
        Font = EasyWriteTheme.SubheadingFont(8.75F),
        ForeColor = EasyWriteTheme.TextSecondary,
        Margin = new Padding(0, 0, 10, 2),
        Text = text
    };

    private static Label CreateValueLabel(string text) => new()
    {
        Anchor = AnchorStyles.Left,
        AutoSize = true,
        Font = EasyWriteTheme.SubheadingFont(9F),
        ForeColor = EasyWriteTheme.TextPrimary,
        Margin = new Padding(8, 0, 0, 0),
        Text = text
    };

    private void HandleTextChanged(object? sender, EventArgs e)
    {
        UpdateTextMetadata();
        ContentAvailabilityChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateTextMetadata()
    {
        _characterCountLabel.Text = _textEditor.TextLength == 1
            ? "1 character"
            : $"{_textEditor.TextLength:N0} characters";
        _placeholderLabel.Visible = _textEditor.TextLength == 0;
    }

    private void ClearText()
    {
        _textEditor.Clear();
        _textEditor.Focus();
    }
}
