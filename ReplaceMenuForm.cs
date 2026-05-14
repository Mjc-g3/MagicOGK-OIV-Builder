using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MagicOGK_OIV_Builder
{
    public partial class ReplaceMenuForm : Form
    {
        public Action OnVehicles;
        public Action OnClothes;
        public Action OnWeapons;
        public Action OnPeds;

        public ReplaceMenuForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(560, 365);
            BackColor = Color.FromArgb(16, 16, 16);
            ShowInTaskbar = false;
            TopMost = true;

            Panel border = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(180, 70, 70),
                Padding = new Padding(1)
            };
            Controls.Add(border);

            Panel main = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 16, 16)
            };
            border.Controls.Add(main);

            main.Paint += (s, e) =>
            {
                using LinearGradientBrush bg = new LinearGradientBrush(main.ClientRectangle, Color.FromArgb(25, 25, 25), Color.FromArgb(12, 12, 12), 90f);
                e.Graphics.FillRectangle(bg, main.ClientRectangle);
            };

            Label title = new Label
            {
                Text = "REPLACE MENU",
                ForeColor = Color.FromArgb(255, 115, 125),
                Font = new Font("Syne", 15F, FontStyle.Bold),
                Location = new Point(22, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            main.Controls.Add(title);

            Label subtitle = new Label
            {
                Text = "Choose what type of replacement you want to build.",
                ForeColor = Color.FromArgb(190, 160, 160),
                Font = new Font("Segoe UI", 9F),
                Location = new Point(24, 54),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            main.Controls.Add(subtitle);

            Button close = new Button
            {
                Text = "✕",
                Location = new Point(518, 14),
                Size = new Size(28, 28),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(220, 120, 120),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                TabStop = false,
                Cursor = Cursors.Hand
            };
            close.FlatAppearance.BorderSize = 0;
            close.Click += (s, e) => AnimateDialogOut();
            main.Controls.Add(close);

            Panel vehicles = CreateCard("🚗", "Vehicles", "Replace car model files", 120, 105);
            Panel clothes = CreateCard("👕", "Clothes", "Story player clothing", 300, 105);

            Panel weapons = CreateCard("🔫", "Weapons", "Replace weapon files", 120, 220);
            Panel peds = CreateCard("👤", "Peds", "Replace NPC/player peds", 300, 220);

            main.Controls.Add(vehicles);
            main.Controls.Add(clothes);
            main.Controls.Add(weapons);
            main.Controls.Add(peds);

            vehicles.Click += (s, e) => { OnVehicles?.Invoke(); AnimateDialogOut(); };
            clothes.Click += (s, e) => { OnClothes?.Invoke(); AnimateDialogOut(); };
            weapons.Click += (s, e) => { OnWeapons?.Invoke(); AnimateDialogOut(); };
            peds.Click += (s, e) => { OnPeds?.Invoke(); AnimateDialogOut(); };

            HookChildClicks(vehicles, () => { OnVehicles?.Invoke(); AnimateDialogOut(); });
            HookChildClicks(clothes, () => { OnClothes?.Invoke(); AnimateDialogOut(); });
            HookChildClicks(weapons, () => { OnWeapons?.Invoke(); AnimateDialogOut(); });
            HookChildClicks(peds, () => { OnPeds?.Invoke(); AnimateDialogOut(); });

            this.Deactivate += (s, e) =>
            {
                if (!isClosingAnimated)
                    AnimateDialogOut();
            };

            Shown += (s, e) => AnimateDialogIn();
            TopMost = true;
        }

        private Panel CreateCard(string icon, string title, string desc, int x, int y)
        {
            AnimatedGlowPanel card = new AnimatedGlowPanel
            {
                Location = new Point(x, y),
                Size = new Size(155, 92),
                BackColor = Color.FromArgb(24, 24, 24),
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
            {
                bool hover = card.ClientRectangle.Contains(card.PointToClient(Cursor.Position));
                using LinearGradientBrush bg = new LinearGradientBrush(card.ClientRectangle, Color.FromArgb(34, 34, 34), Color.FromArgb(18, 18, 18), 90f);
                e.Graphics.FillRectangle(bg, card.ClientRectangle);
                using Pen p = new Pen(hover ? Color.FromArgb(255, 115, 125) : Color.FromArgb(70, 50, 50), hover ? 2 : 1);
                e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            };

            Label iconLbl = new Label
            {
                Text = icon,
                ForeColor = Color.WhiteSmoke,
                Font = new Font("Segoe UI Emoji", 22F),
                Location = new Point(0, 8),
                Size = new Size(155, 34),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label titleLbl = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(255, 115, 125),
                Font = new Font("Syne", 9F, FontStyle.Bold),
                Location = new Point(5, 46),
                Size = new Size(145, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label descLbl = new Label
            {
                Text = desc,
                ForeColor = Color.FromArgb(185, 165, 165),
                Font = new Font("Segoe UI", 8F),
                Location = new Point(8, 66),
                Size = new Size(139, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            card.Controls.Add(iconLbl);
            card.Controls.Add(titleLbl);
            card.Controls.Add(descLbl);

            return card;
        }

        private void HookChildClicks(Control parent, Action action)
        {
            foreach (Control c in parent.Controls)
            {
                c.Cursor = Cursors.Hand;
                c.Click += (s, e) => action();
            }
        }
        //Animated transitions
        private void AnimateDialogIn()
        {
            Opacity = 0;

            int finalY = Location.Y;
            Location = new Point(Location.X, finalY - 25);

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 20;

            int step = 0;
            int maxSteps = 14;

            timer.Tick += (s, e) =>
            {
                step++;

                double t = step / (double)maxSteps;
                double eased = 1 - Math.Pow(1 - t, 3); // ease-out

                Opacity = eased;
                Location = new Point(Location.X, finalY - 25 + (int)(25 * eased));

                if (step >= maxSteps)
                {
                    Opacity = 1;
                    Location = new Point(Location.X, finalY);
                    timer.Stop();
                    timer.Dispose();
                }
            };

            timer.Start();
        }
        private bool isClosingAnimated = false;

        private void AnimateDialogOut(Action? onFinished = null)
        {
            if (isClosingAnimated)
                return;

            isClosingAnimated = true;

            int startY = Location.Y;

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 15;

            int step = 0;
            int maxSteps = 12;

            timer.Tick += (s, e) =>
            {
                step++;

                double t = step / (double)maxSteps;
                double eased = 1 - Math.Pow(1 - t, 3);

                Opacity = 1.0 - eased;

                Location = new Point(
                    Location.X,
                    startY - (int)(20 * eased)
                );

                if (step >= maxSteps)
                {
                    timer.Stop();
                    timer.Dispose();

                    onFinished?.Invoke();

                    base.Close();
                }
            };

            timer.Start();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!isClosingAnimated)
            {
                e.Cancel = true;
                AnimateDialogOut();
                return;
            }

            base.OnFormClosing(e);
        }
    }
}
