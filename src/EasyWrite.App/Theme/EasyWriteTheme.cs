namespace EasyWrite.App.Theme;

internal static class EasyWriteTheme
{
    public static readonly Color WindowBackground = Color.FromArgb(244, 247, 252);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceMuted = Color.FromArgb(247, 249, 253);
    public static readonly Color Border = Color.FromArgb(215, 223, 235);
    public static readonly Color Primary = Color.FromArgb(37, 99, 235);
    public static readonly Color PrimaryDark = Color.FromArgb(29, 78, 216);
    public static readonly Color PrimarySoft = Color.FromArgb(229, 238, 255);
    public static readonly Color TextPrimary = Color.FromArgb(24, 35, 54);
    public static readonly Color TextSecondary = Color.FromArgb(86, 101, 124);
    public static readonly Color Danger = Color.FromArgb(190, 50, 50);
    public static readonly Color DangerSoft = Color.FromArgb(255, 238, 238);
    public static readonly Color Warning = Color.FromArgb(180, 105, 20);
    public static readonly Color Success = Color.FromArgb(30, 130, 90);
    public static readonly Color Disabled = Color.FromArgb(151, 163, 181);

    public static Font HeadingFont(float size = 20F) =>
        new("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Point);

    public static Font SubheadingFont(float size = 11F) =>
        new("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Point);

    public static Font BodyFont(float size = 9.5F) =>
        new("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Point);

    public static Button CreatePrimaryButton(string text, int tabIndex) =>
        CreateButton(text, tabIndex, Primary, Color.White, FlatStyle.Flat);

    public static Button CreateSecondaryButton(string text, int tabIndex) =>
        CreateButton(text, tabIndex, Surface, TextPrimary, FlatStyle.Flat);

    public static Button CreateDangerButton(string text, int tabIndex) =>
        CreateButton(text, tabIndex, DangerSoft, Danger, FlatStyle.Flat);

    private static Button CreateButton(
        string text,
        int tabIndex,
        Color backColor,
        Color foreColor,
        FlatStyle flatStyle)
    {
        var button = new Button
        {
            AutoSize = false,
            BackColor = backColor,
            Cursor = Cursors.Hand,
            FlatStyle = flatStyle,
            Font = SubheadingFont(9.25F),
            ForeColor = foreColor,
            Height = 40,
            MinimumSize = new Size(112, 40),
            Padding = new Padding(12, 0, 12, 0),
            TabIndex = tabIndex,
            Text = text,
            UseVisualStyleBackColor = false
        };

        button.FlatAppearance.BorderColor = backColor == Surface ? Border : backColor;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseDownBackColor = backColor == Primary ? PrimaryDark : SurfaceMuted;
        button.FlatAppearance.MouseOverBackColor = backColor == Primary ? PrimaryDark : SurfaceMuted;
        button.EnabledChanged += (_, _) => ApplyButtonEnabledAppearance(
            button,
            backColor,
            foreColor);
        return button;
    }

    private static void ApplyButtonEnabledAppearance(
        Button button,
        Color enabledBackColor,
        Color enabledForeColor)
    {
        button.BackColor = button.Enabled ? enabledBackColor : SurfaceMuted;
        button.ForeColor = button.Enabled ? enabledForeColor : Disabled;
        button.FlatAppearance.BorderColor = button.Enabled
            ? enabledBackColor == Surface ? Border : enabledBackColor
            : Border;
    }
}
