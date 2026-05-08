using System;
using System.Drawing;
using System.Windows.Forms;

namespace MagicOGK_OIV_Builder
{
    public partial class ReplaceMenuForm : Form
    {
        public Action OnVehicles;
        public Action OnClothes;
        public Action OnWeapons;

        public ReplaceMenuForm()
        {
            // FORM
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(260, 190);
            BackColor = Color.FromArgb(16, 16, 16);
            ShowInTaskbar = false;
            TopMost = true;

            // BORDER
            Panel border = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(180, 70, 70)
            };
            Controls.Add(border);

            Panel main = new Panel
            {
                Location = new Point(1, 1),
                Size = new Size(258, 188),
                BackColor = Color.FromArgb(16, 16, 16)
            };
            border.Controls.Add(main);

            // TITLE
            Label title = new Label
            {
                Text = "REPLACE MENU",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 11F, FontStyle.Bold),
                Location = new Point(12, 10),
                AutoSize = true
            };
            main.Controls.Add(title);

            // BUTTONS
            Button btnVehicles = CreateButton("Replace Vehicles", 50);
            Button btnClothes = CreateButton("Replace Clothes", 95);
            Button btnWeapons = CreateButton("Replace Weapons", 140);

            main.Controls.Add(btnVehicles);
            main.Controls.Add(btnClothes);
            main.Controls.Add(btnWeapons);

            btnVehicles.Click += (s, e) => { OnVehicles?.Invoke(); Close(); };
            btnClothes.Click += (s, e) => { OnClothes?.Invoke(); Close(); };
            btnWeapons.Click += (s, e) => { OnWeapons?.Invoke(); Close(); };

            this.Deactivate += (s, e) => this.Close();
        }
        private Button CreateButton(string text, int y)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(15, y),
                Size = new Size(228, 35),
                BackColor = Color.FromArgb(95, 0, 0),
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(180, 70, 70);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(125, 0, 0);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 0, 0);

            return btn;
        }
    }
}