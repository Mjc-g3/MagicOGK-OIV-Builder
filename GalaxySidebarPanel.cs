using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class GalaxySidebarPanel : Panel
{
    private readonly System.Windows.Forms.Timer timer =
    new System.Windows.Forms.Timer();
    private readonly Random rnd = new Random();
    private Star[] stars = Array.Empty<Star>();
    private Point mouse;

    private struct Star
    {
        public float X, Y, Z, Speed, Size, Twinkle;
    }

    public GalaxySidebarPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(15, 15, 15);

        timer.Interval = 16;
        timer.Tick += (s, e) =>
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].Y += stars[i].Speed;

                float dx = stars[i].X - mouse.X;
                float dy = stars[i].Y - mouse.Y;
                float dist = Math.Max(1, (float)Math.Sqrt(dx * dx + dy * dy));

                if (dist < 70)
                {
                    stars[i].X += dx / dist * 0.8f;
                    stars[i].Y += dy / dist * 0.8f;
                }

                if (stars[i].Y > Height + 10)
                    ResetStar(ref stars[i], true);

                stars[i].Twinkle += 0.05f;
            }

            Invalidate();
        };

        timer.Start();

        MouseMove += (s, e) => mouse = e.Location;
        Resize += (s, e) => InitStars();
    }

    private void InitStars()
    {
        int count = Math.Max(60, Width * Height / 900);
        stars = new Star[count];

        for (int i = 0; i < stars.Length; i++)
            ResetStar(ref stars[i], false);
    }

    private void ResetStar(ref Star s, bool fromTop)
    {
        s.X = rnd.Next(0, Math.Max(1, Width));
        s.Y = fromTop ? rnd.Next(-80, 0) : rnd.Next(0, Math.Max(1, Height));
        s.Z = (float)rnd.NextDouble();
        s.Speed = 0.25f + s.Z * 1.2f;
        s.Size = 0.8f + s.Z * 2.2f;
        s.Twinkle = (float)rnd.NextDouble() * 10f;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        foreach (var s in stars)
        {
            int alpha = 80 + (int)(Math.Sin(s.Twinkle) * 45) + (int)(s.Z * 100);
            alpha = Math.Max(30, Math.Min(210, alpha));

            Color glow = Color.FromArgb(alpha, 180, 70, 75);

            using SolidBrush b = new SolidBrush(glow);
            e.Graphics.FillEllipse(b, s.X, s.Y, s.Size, s.Size);

            if (s.Z > 0.65f)
            {
                using Pen p = new Pen(Color.FromArgb(alpha / 3, 210, 90, 95), 1);
                e.Graphics.DrawLine(p, s.X, s.Y - 5, s.X, s.Y + 5);
                e.Graphics.DrawLine(p, s.X - 5, s.Y, s.X + 5, s.Y);
            }
        }

        using LinearGradientBrush shade = new LinearGradientBrush(
            ClientRectangle,
            Color.FromArgb(80, 0, 0, 0),
            Color.FromArgb(10, 120, 20, 20),
            90f);

        e.Graphics.FillRectangle(shade, ClientRectangle);
    }
}