using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MagicOGK_OIV_Builder.Services;

namespace MagicOGK_OIV_Builder
{
    public partial class InstallOivForm : Form
    {
        private readonly string gta5Path;
        private string selectedOivPath = "";

        private TextBox txtOivPath;
        private Button btnBrowseOiv;
        private Button btnInstall;
        private ProgressBar progressBar;
        private TextBox txtLog;

        public InstallOivForm(string gta5Path)
        {
            InitializeComponent();
            this.gta5Path = gta5Path;

            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Install OIV Package";
            Size = new Size(720, 430);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(13, 13, 13);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label title = new Label
            {
                Text = "INSTALL OIV PACKAGE",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 22)
            };

            Label gtaLabel = new Label
            {
                Text = "GTA V Directory:",
                ForeColor = Color.FromArgb(140, 100, 100),
                AutoSize = true,
                Location = new Point(24, 62)
            };

            TextBox txtGtaPath = new TextBox
            {
                Text = gta5Path,
                ReadOnly = true,
                Location = new Point(24, 82),
                Size = new Size(650, 24),
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(220, 170, 170),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label oivLabel = new Label
            {
                Text = "OIV Package:",
                ForeColor = Color.FromArgb(140, 100, 100),
                AutoSize = true,
                Location = new Point(24, 122)
            };

            txtOivPath = new TextBox
            {
                ReadOnly = true,
                Location = new Point(24, 142),
                Size = new Size(540, 24),
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(220, 170, 170),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            txtOivPath.Click += BtnBrowseOiv_Click;

            btnBrowseOiv = new Button
            {
                Text = "BROWSE",
                Location = new Point(575, 141),
                Size = new Size(100, 26),
                BackColor = Color.FromArgb(120, 18, 24),
                ForeColor = Color.FromArgb(240, 190, 190),
                FlatStyle = FlatStyle.Flat
            };
            btnBrowseOiv.FlatAppearance.BorderSize = 0;
            btnBrowseOiv.Click += BtnBrowseOiv_Click;

            btnInstall = new Button
            {
                Text = "INSTALL",
                Location = new Point(575, 182),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(120, 18, 24),
                ForeColor = Color.FromArgb(240, 190, 190),
                FlatStyle = FlatStyle.Flat
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;

            progressBar = new ProgressBar
            {
                Location = new Point(24, 185),
                Size = new Size(540, 24)
            };

            txtLog = new TextBox
            {
                Location = new Point(24, 230),
                Size = new Size(650, 130),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.FromArgb(180, 130, 130),
                BorderStyle = BorderStyle.FixedSingle
            };

            Controls.Add(title);
            Controls.Add(gtaLabel);
            Controls.Add(txtGtaPath);
            Controls.Add(oivLabel);
            Controls.Add(txtOivPath);
            Controls.Add(btnBrowseOiv);
            Controls.Add(progressBar);
            Controls.Add(btnInstall);
            Controls.Add(txtLog);
        }

        private void BtnBrowseOiv_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select OIV package",
                Filter = "OpenIV Package (*.oiv)|*.oiv",
                Multiselect = false
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            selectedOivPath = dlg.FileName;
            txtOivPath.Text = selectedOivPath;

            Log("Selected OIV: " + Path.GetFileName(selectedOivPath));
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedOivPath))
            {
                MessageBox.Show("Please select an OIV package first.");
                return;
            }

            progressBar.Style = ProgressBarStyle.Marquee;

            MagicOivInstaller installer = new MagicOivInstaller();

            installer.Log = Log;

            bool result = installer.Install(selectedOivPath, gta5Path);

            progressBar.Style = ProgressBarStyle.Blocks;

            if (result)
            {
                Log("Ready for install phase.");
            }
            else
            {
                Log("Installer validation failed.");
            }
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }
    }
}