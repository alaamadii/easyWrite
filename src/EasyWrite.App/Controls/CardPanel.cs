using EasyWrite.App.Theme;

namespace EasyWrite.App.Controls;

internal sealed class CardPanel : Panel
{
    public CardPanel()
    {
        BackColor = EasyWriteTheme.Surface;
        DoubleBuffered = true;
        Padding = new Padding(28, 24, 28, 24);
        ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var borderPen = new Pen(EasyWriteTheme.Border);
        e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
    }
}
