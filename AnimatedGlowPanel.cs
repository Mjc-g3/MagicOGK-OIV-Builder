using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class AnimatedGlowPanel : Panel
{
    private readonly System.Windows.Forms.Timer timer;
    private float glareX = -300;
    private float targetGlareX = -300;
    private float hoverAmount = 0f;
    private float targetHoverAmount = 0f;

    public int CornerRadius { get; set; } = 4;

    public AnimatedGlowPanel()
    {
        DoubleBuffered = true;
        Cursor = Cursors.Hand;

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 15;
        timer.Tick += (s, e) =>
        {
            glareX += (targetGlareX - glareX) * 0.18f;
            hoverAmount += (targetHoverAmount - hoverAmount) * 0.18f;
            Invalidate();
        };

        MouseEnter += (s, e) =>
        {
            targetHoverAmount = 1f;
            timer.Start();
        };

        MouseLeave += (s, e) =>
        {
            Point p = PointToClient(Cursor.Position);

            if (!ClientRectangle.Contains(p))
            {
                targetHoverAmount = 0f;
            }
        };

        MouseMove += (s, e) =>
        {
            targetGlareX = e.X;
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Color.FromArgb(15, 15, 15));
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = new Rectangle(1, 1, Width - 3, Height - 3);

        using GraphicsPath path = RoundedRect(rect, CornerRadius);

        using (SolidBrush bg = new SolidBrush(BackColor))
            e.Graphics.FillPath(bg, path);

        if (hoverAmount > 0.01f)
        {
            Rectangle glareRect = new Rectangle((int)glareX - 160, -30, 320, Height + 60);

            using LinearGradientBrush glare = new LinearGradientBrush(
                glareRect,
                Color.FromArgb(0, Color.White),
                Color.FromArgb((int)(55 * hoverAmount), Color.White),
                LinearGradientMode.ForwardDiagonal
            );

            ColorBlend blend = new ColorBlend
            {
                Positions = new[] { 0f, 0.35f, 0.5f, 0.65f, 1f },
                Colors = new[]
                {
                    Color.FromArgb(0, Color.White),
                    Color.FromArgb(0, Color.White),
                    Color.FromArgb((int)(65 * hoverAmount), Color.White),
                    Color.FromArgb(0, Color.White),
                    Color.FromArgb(0, Color.White)
                }
            };

            glare.InterpolationColors = blend;
            e.Graphics.SetClip(path);
            e.Graphics.FillRectangle(glare, glareRect);
            e.Graphics.ResetClip();
        }

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            rect,
            ForeColor,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter
        );
    }
    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);

        e.Control.MouseEnter += (s, ev) =>
        {
            targetHoverAmount = 1f;
            timer.Start();
        };

        e.Control.MouseMove += (s, ev) =>
        {
            Point p = PointToClient(Cursor.Position);
            targetGlareX = p.X;
        };

        e.Control.MouseLeave += (s, ev) =>
        {
            Point p = PointToClient(Cursor.Position);

            if (!ClientRectangle.Contains(p))
            {
                targetHoverAmount = 0f;
            }
        };
    }
    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        GraphicsPath path = new GraphicsPath();

        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);

        path.CloseFigure();
        return path;
    }
}