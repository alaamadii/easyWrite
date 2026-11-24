using System.ComponentModel;
using System.Drawing.Drawing2D;
using EasyWrite.App.Theme;
using EasyWrite.Core.Models;

namespace EasyWrite.App.Controls;

internal sealed class OperationStatusControl : UserControl
{
    private readonly StatusIndicator _indicator;
    private readonly Label _statusLabel;

    public OperationStatusControl()
    {
        BackColor = EasyWriteTheme.Surface;
        Dock = DockStyle.Fill;
        MinimumSize = new Size(0, 48);
        Padding = new Padding(32, 8, 32, 8);
        AccessibleName = "Application operation status";

        _indicator = new StatusIndicator
        {
            AccessibleName = "Status indicator",
            Margin = new Padding(0, 4, 10, 0),
            Size = new Size(16, 16),
            TabStop = false
        };

        _statusLabel = new Label
        {
            AutoSize = true,
            Font = EasyWriteTheme.SubheadingFont(9.5F),
            ForeColor = EasyWriteTheme.TextPrimary,
            Margin = new Padding(0, 2, 0, 0),
            Text = "Ready"
        };

        var leftGroup = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = Padding.Empty,
            WrapContents = false
        };
        leftGroup.Controls.Add(_indicator);
        leftGroup.Controls.Add(_statusLabel);

        var privacyLabel = new Label
        {
            Anchor = AnchorStyles.Right,
            AutoSize = true,
            Font = EasyWriteTheme.BodyFont(9F),
            ForeColor = EasyWriteTheme.TextSecondary,
            Margin = Padding.Empty,
            Text = "Local Windows speech  •  Privacy first",
            TextAlign = ContentAlignment.MiddleRight
        };

        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            RowCount = 1
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.Controls.Add(leftGroup, 0, 0);
        layout.Controls.Add(privacyLabel, 1, 0);
        Controls.Add(layout);
    }

    public void ApplyState(
        SpeechOperationState operationState,
        string statusText,
        string accessibleStatus)
    {
        _statusLabel.Text = statusText;
        _indicator.IndicatorColor = GetIndicatorColor(operationState);
        AccessibleDescription = accessibleStatus;
        _indicator.AccessibleDescription = accessibleStatus;
    }

    private static Color GetIndicatorColor(SpeechOperationState operationState) =>
        operationState switch
        {
            SpeechOperationState.Idle => EasyWriteTheme.Disabled,
            SpeechOperationState.Speaking => EasyWriteTheme.Primary,
            SpeechOperationState.Paused => EasyWriteTheme.Warning,
            SpeechOperationState.Listening => EasyWriteTheme.Danger,
            SpeechOperationState.Stopping => EasyWriteTheme.TextSecondary,
            SpeechOperationState.Error => EasyWriteTheme.Danger,
            _ => EasyWriteTheme.Disabled
        };

    private sealed class StatusIndicator : Control
    {
        private Color _indicatorColor = EasyWriteTheme.Disabled;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color IndicatorColor
        {
            get => _indicatorColor;
            set
            {
                _indicatorColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(_indicatorColor);
            e.Graphics.FillEllipse(brush, 2, 2, Width - 4, Height - 4);
        }
    }
}
