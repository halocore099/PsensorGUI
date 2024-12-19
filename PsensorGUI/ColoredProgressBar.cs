using System;
using System.Drawing;
using System.Windows.Forms;

public class ColoredProgressBar : ProgressBar
{
    private string _label = "";

    public string Label
    {
        get => _label;
        set
        {
            _label = value;
            Invalidate();
        }
    }

    public ColoredProgressBar()
    {
        this.SetStyle(ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle rect = this.ClientRectangle;
        Graphics g = e.Graphics;

        // Enable anti-aliasing for smoother text
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Draw the base progress bar
        ProgressBarRenderer.DrawHorizontalBar(g, rect);
        rect.Inflate(-3, -3);

        if (this.Value > 0)
        {
            // Calculate the percentage
            float percentage = (float)this.Value / this.Maximum;
            int width = (int)(rect.Width * percentage);

            // Determine color based on usage level
            Color color = percentage switch
            {
                < 0.6f => Color.FromArgb(0, 164, 0),    // Green
                < 0.85f => Color.FromArgb(255, 164, 0),  // Orange
                _ => Color.FromArgb(232, 17, 35)         // Red
            };

            // Draw the progress bar fill
            using (Brush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, rect.X, rect.Y, width, rect.Height);
            }
        }

        // Draw the text
        using (var font = new Font("Segoe UI", 9f))
        {
            string text = $"{_label}: {Value}%";
            SizeF textSize = g.MeasureString(text, font);
            float x = (rect.Width - textSize.Width) / 2;
            float y = (rect.Height - textSize.Height) / 2 + 2;

            // Draw text shadow/outline for better visibility
            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(50, Color.Black)))
            {
                g.DrawString(text, font, shadowBrush, x + 1, y + 1);
            }

            // Draw the main text
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                g.DrawString(text, font, textBrush, x, y);
            }
        }
    }
}
