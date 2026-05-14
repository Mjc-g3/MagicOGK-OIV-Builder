using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class AnimatedGlowButton : Button
{
    private readonly System.Windows.Forms.Timer timer;

    private float glareX = -300;
    private float targetGlareX = -300;
    private float hoverAmount = 0f;
    private float targetHoverAmount = 0f;

    public bool TransparentIdle { get; set; } = false;

    public int CornerRadius { get; set; } = 6;

    public AnimatedGlowButton()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true
        );

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        FlatAppearance.MouseOverBackColor = BackColor;
        FlatAppearance.MouseDownBackColor = BackColor;

        BackColor = Color.FromArgb(92, 0, 0);
        ForeColor = Color.FromArgb(235, 165, 165);

        Cursor = Cursors.Hand;

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 15;
        timer.Tick += (s, e) =>
        {
            glareX += (targetGlareX - glareX) * 0.18f;
            hoverAmount += (targetHoverAmount - hoverAmount) * 0.18f;

            if (Math.Abs(glareX - targetGlareX) < 0.5f)
                glareX = targetGlareX;

            if (Math.Abs(hoverAmount - targetHoverAmount) < 0.01f)
                hoverAmount = targetHoverAmount;

            Invalidate();
        };

        MouseEnter += (s, e) =>
        {
            targetHoverAmount = 1f;
            timer.Start();
        };

        MouseLeave += (s, e) =>
        {
            targetHoverAmount = 0f;
            targetGlareX = Width + 250;
            hoverAmount = 0f;
            glareX = -300;
            Invalidate();
        };

        MouseMove += (s, e) =>
        {
            targetGlareX = e.X;
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (TransparentIdle)
            PaintParentBackground(e.Graphics);
        else
            e.Graphics.Clear(Parent?.BackColor ?? Color.FromArgb(15, 15, 15));

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        Rectangle rect = new Rectangle(1, 1, Width - 3, Height - 3);

        using GraphicsPath path = RoundedRect(rect, CornerRadius);

        Color baseColor = BackColor;
        Color hoverColor = Blend(baseColor, Color.White, 0.10f * hoverAmount);
        Color borderColor = Blend(Color.FromArgb(120, 40, 40), Color.FromArgb(255, 120, 120), hoverAmount);

        if (!TransparentIdle)
        {
            using SolidBrush bg = new SolidBrush(BackColor);
            e.Graphics.FillPath(bg, path);
        }
        else if (hoverAmount > 0.01f)
        {
            using SolidBrush bg = new SolidBrush(
                Color.FromArgb((int)(35 * hoverAmount), 255, 255, 255)
            );

            e.Graphics.FillPath(bg, path);
        }

        if (hoverAmount > 0.01f)
        {
            Rectangle glareRect = new Rectangle((int)glareX - 90, -20, 180, Height + 40);

            using LinearGradientBrush glare = new LinearGradientBrush(
                glareRect,
                Color.FromArgb((int)(0 * hoverAmount), Color.White),
                Color.FromArgb((int)(95 * hoverAmount), Color.White),
                LinearGradientMode.ForwardDiagonal
            );

            ColorBlend blend = new ColorBlend
            {
                Positions = new[] { 0f, 0.45f, 0.5f, 0.55f, 1f },
                Colors = new[]
                {
                    Color.FromArgb(0, Color.White),
                    Color.FromArgb(0, Color.White),
                    Color.FromArgb((int)(85 * hoverAmount), Color.White),
                    Color.FromArgb(0, Color.White),
                    Color.FromArgb(0, Color.White)
                }
            };

            glare.InterpolationColors = blend;
            e.Graphics.SetClip(path);
            e.Graphics.FillRectangle(glare, glareRect);
            e.Graphics.ResetClip();
        }
        /*
        if (hoverAmount > 0.02f)
        {
            using Pen border = new Pen(
                Color.FromArgb((int)(120 * hoverAmount), 255, 120, 120),
                1
            );

            e.Graphics.DrawPath(border, path);
        }
        */
        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            rect,
            ForeColor,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis
        );
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        UpdateRegion();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0)
            return;

        using GraphicsPath path = RoundedRect(
            new Rectangle(0, 0, Width, Height),
            CornerRadius
        );

        Region = new Region(path);
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

    private static Color Blend(Color a, Color b, float amount)
    {
        amount = Math.Max(0, Math.Min(1, amount));

        return Color.FromArgb(
            (int)(a.R + (b.R - a.R) * amount),
            (int)(a.G + (b.G - a.G) * amount),
            (int)(a.B + (b.B - a.B) * amount)
        );
    }
    private void PaintParentBackground(Graphics g)
    {
        if (Parent == null)
        {
            g.Clear(Color.FromArgb(15, 15, 15));
            return;
        }

        GraphicsState state = g.Save();

        g.TranslateTransform(-Left, -Top);

        using PaintEventArgs pe = new PaintEventArgs(
            g,
            new Rectangle(Left, Top, Width, Height)
        );

        InvokePaintBackground(Parent, pe);
        InvokePaint(Parent, pe);

        g.Restore(state);
    }
}