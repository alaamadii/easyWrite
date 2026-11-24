namespace EasyWrite.App;

public sealed class MainForm : Form
{
    public MainForm()
    {
        Text = "EasyWrite";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(960, 640);
        ClientSize = new Size(1180, 760);

        Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            Location = new Point(32, 28),
            Text = "EasyWrite project foundation"
        });

        Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = SystemColors.GrayText,
            Location = new Point(35, 75),
            Text = "The complete speech workspace will be implemented in the UI phase."
        });
    }
}
