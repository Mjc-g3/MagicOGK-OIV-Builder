using System;
using System.Drawing;
using System.Windows.Forms;

namespace MagicOGK_OIV_Builder
{
    public partial class LoadingForm : Form
    {
        public LoadingForm(string message = "Opening OIV package...")
        {
            // FORM
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(340, 130);
            BackColor = Color.FromArgb(16, 16, 16);
            ShowInTaskbar = false;
            ControlBox = false;
            TopMost = true;

            // OUTER BORDER PANEL
            Panel borderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(180, 70, 70)
            };

            Controls.Add(borderPanel);

            // INNER PANEL
            Panel mainPanel = new Panel
            {
                Location = new Point(1, 1),
                Size = new Size(338, 128),
                BackColor = Color.FromArgb(16, 16, 16)
            };

            borderPanel.Controls.Add(mainPanel);

            // TEXT
            Label lbl = new Label
            {
                Text = message,
                ForeColor = Color.FromArgb(220, 150, 150),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 32),
                Size = new Size(338, 35)
            };

            mainPanel.Controls.Add(lbl);

            // ------ LOADING BAR ------

            

            // PROGRESS BAR BACKGROUND
            Panel progressBack = new Panel
            {
                Location = new Point(35, 78),
                Size = new Size(268, 10),
                BackColor = Color.FromArgb(32, 32, 32)
            };

            mainPanel.Controls.Add(progressBack);

            // MOVING PROGRESS CHUNK
            int chunkWidth = 80;

            Panel progressFill = new Panel
            {
                Location = new Point(-chunkWidth, 0),
                Size = new Size(chunkWidth, 10),
                BackColor = Color.FromArgb(130, 200, 245)
            };

            progressBack.Controls.Add(progressFill);

            Panel glow = new Panel
            {
                Size = new Size(chunkWidth / 2, 10),
                BackColor = Color.FromArgb(80, 220, 255),
                Location = new Point(0, 0)
            };

            progressFill.Controls.Add(glow);

            // TRUE LEFT-TO-RIGHT MARQUEE ANIMATION
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;

            int x = -chunkWidth;

            timer.Tick += (s, e) =>
            {
                x += 3;

                if (x > progressBack.Width)
                    x = -chunkWidth;

                progressFill.Location = new Point(x, 0);
            };

            timer.Start();

            this.FormClosed += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
            };
        }
    }
}