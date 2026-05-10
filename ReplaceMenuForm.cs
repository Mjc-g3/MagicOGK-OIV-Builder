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
            Size = new Size(560, 330);
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
            close.Click += (s, e) => Close();
            main.Controls.Add(close);

            Panel vehicles = CreateCard("🚗", "Vehicles", "Replace car model files", 24, 95);
            Panel clothes = CreateCard("👕", "Clothes", "Story player clothing", 202, 95);
            Panel weapons = CreateCard("🔫", "Weapons", "Replace weapon files", 380, 95);
            Panel peds = CreateCard("👤", "Peds", "Replace NPC/player peds", 113, 210);

            main.Controls.Add(vehicles);
            main.Controls.Add(clothes);
            main.Controls.Add(weapons);
            main.Controls.Add(peds);

            vehicles.Click += (s, e) => { OnVehicles?.Invoke(); Close(); };
            clothes.Click += (s, e) => { OnClothes?.Invoke(); Close(); };
            weapons.Click += (s, e) => { OnWeapons?.Invoke(); Close(); };
            peds.Click += (s, e) => { OnPeds?.Invoke(); Close(); };

            HookChildClicks(vehicles, () => { OnVehicles?.Invoke(); Close(); });
            HookChildClicks(clothes, () => { OnClothes?.Invoke(); Close(); });
            HookChildClicks(weapons, () => { OnWeapons?.Invoke(); Close(); });
            HookChildClicks(peds, () => { OnPeds?.Invoke(); Close(); });

            this.Deactivate += (s, e) => this.Close();
        }

        private Panel CreateCard(string icon, string title, string desc, int x, int y)
        {
            Panel card = new Panel
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

            card.MouseEnter += (s, e) => card.Invalidate();
            card.MouseLeave += (s, e) => card.Invalidate();

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
    }
}
