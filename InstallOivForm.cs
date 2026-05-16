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
        private TextBox txtGtaPath;

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

            txtGtaPath = new TextBox
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

            Button btnAutoDetectGtaPath = new Button
            {
                Text = "AUTO-DETECT",
                Size = new Size(120, 28),
                BackColor = Color.FromArgb(120, 18, 24),
                ForeColor = Color.FromArgb(240, 190, 190),
                FlatStyle = FlatStyle.Flat
            };

            btnAutoDetectGtaPath.FlatAppearance.BorderSize = 0;
            btnAutoDetectGtaPath.Click += BtnAutoDetectGtaPath_Click;

            Button btnUninstallOiv = new Button
            {
                Text = "UNINSTALL OIV",
                Size = new Size(140, 28),
                BackColor = Color.FromArgb(55, 0, 0),
                ForeColor = Color.FromArgb(240, 190, 190),
                FlatStyle = FlatStyle.Flat
            };

            btnUninstallOiv.FlatAppearance.BorderSize = 0;
            btnUninstallOiv.Click += BtnUninstallOiv_Click;

            btnAutoDetectGtaPath.Location = new Point(24, 112);
            btnUninstallOiv.Location = new Point(435, 182);

            Controls.Add(btnAutoDetectGtaPath);
            Controls.Add(btnUninstallOiv);

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

            DialogResult confirm = MessageBox.Show(
    "This will install the selected OIV package into your GTA V mods folder.\n\n" +
    "RPF files will be copied to /mods first if needed, and backups will be created before modification.\n\n" +
    "Continue?",
    "Confirm OIV Install",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Warning
);

            if (confirm != DialogResult.Yes)
            {
                Log("Install cancelled by user.");
                return;
            }

            progressBar.Style = ProgressBarStyle.Marquee;

            MagicOivInstaller installer = new MagicOivInstaller();

            installer.Log = Log;

            bool result = installer.Install(selectedOivPath, txtGtaPath.Text.Trim());

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
        private void BtnAutoDetectGtaPath_Click(object sender, EventArgs e)
        {
            string? detectedPath = MagicOivInstaller.DetectGtaVPath();

            if (string.IsNullOrWhiteSpace(detectedPath))
            {
                MessageBox.Show(
                    "Could not auto-detect GTA V path.",
                    "Auto-detect failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            txtGtaPath.Text = detectedPath;
            Log("Auto-detected GTA V path: " + detectedPath);
        }
        private void BtnUninstallOiv_Click(object sender, EventArgs e)
        {
            string gamePath = txtGtaPath.Text.Trim();

            if (string.IsNullOrWhiteSpace(gamePath) ||
                !File.Exists(Path.Combine(gamePath, "GTA5.exe")))
            {
                MessageBox.Show(
                    "Please select or auto-detect a valid GTA V folder first.",
                    "Invalid GTA V Path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string manifestDir = Path.Combine(gamePath, "MagicOGK_UninstallLogs");

            using var dlg = new OpenFileDialog
            {
                Title = "Select uninstall manifest",
                Filter = "MagicOGK Manifest (*.json)|*.json",
                InitialDirectory = Directory.Exists(manifestDir) ? manifestDir : gamePath
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            DialogResult confirm = MessageBox.Show(
                "This will uninstall files listed in the selected manifest.\n\nContinue?",
                "Confirm Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            var installer = new MagicOivInstaller();
            installer.Log = Log;

            var progress = new Progress<int>(percent =>
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = Math.Max(0, Math.Min(100, percent));
            });

            bool ok = installer.Uninstall(gamePath, dlg.FileName, progress);

            MessageBox.Show(
                ok ? "Uninstall complete." : "Uninstall failed. Check the log.",
                "MagicOGK",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error
            );
        }
        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }
    }
}