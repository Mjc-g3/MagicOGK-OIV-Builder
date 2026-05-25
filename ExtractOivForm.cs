using System;
using System.Drawing;
using System.Windows.Forms;

namespace MagicOGK_OIV_Builder
{
    public class ExtractOivForm : Form
    {
        public event Action<string, string, bool>? ExtractRequested;

        private TextBox txtOutputPath;
        private Button btnBrowseOutput;
        private CheckBox chkNestedFolders;
        private Panel dragDropPanel;
        private Label lblDragDropHint;
        private Button btnExtract;
        private TextBox txtLog;

        public string? OutputPath => txtOutputPath?.Text;
        public bool UseNestedFolders => chkNestedFolders?.Checked ?? true;

        public ExtractOivForm()
        {
            BuildUi();
            LoadConfig();
        }

        private void BuildUi()
        {
            Text = "Extract OIV Package";
            Size = new Size(700, 460);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(13, 13, 13);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Label title = new Label
            {
                Text = "EXTRACT OIV PACKAGE",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 22)
            };

            Label outputPathLabel = new Label
            {
                Text = "Extract To:",
                ForeColor = Color.FromArgb(140, 100, 100),
                AutoSize = true,
                Location = new Point(24, 62)
            };

            txtOutputPath = new TextBox
            {
                Location = new Point(24, 82),
                Size = new Size(520, 24),
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(220, 170, 170),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnBrowseOutput = new Button
            {
                Text = "BROWSE",
                Location = new Point(555, 81),
                Size = new Size(100, 26),
                BackColor = Color.FromArgb(120, 18, 24),
                ForeColor = Color.FromArgb(240, 190, 190),
                FlatStyle = FlatStyle.Flat
            };
            btnBrowseOutput.FlatAppearance.BorderSize = 0;
            btnBrowseOutput.Click += BtnBrowseOutput_Click;

            chkNestedFolders = new CheckBox
            {
                Text = "Use nested folder structure (original layout)",
                ForeColor = Color.FromArgb(180, 130, 130),
                BackColor = Color.Transparent,
                Checked = true,
                Location = new Point(24, 115),
                AutoSize = true
            };

            dragDropPanel = new Panel
            {
                Location = new Point(24, 145),
                Size = new Size(630, 80),
                BackColor = Color.FromArgb(18, 18, 18),
                BorderStyle = BorderStyle.FixedSingle,
                AllowDrop = true
            };
            dragDropPanel.DragEnter += DragDropPanel_DragEnter;
            dragDropPanel.DragDrop += DragDropPanel_DragDrop;

            lblDragDropHint = new Label
            {
                Text = "DRAG & DROP OIV FILE HERE",
                ForeColor = Color.FromArgb(100, 80, 80),
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(230, 30)
            };
            dragDropPanel.Controls.Add(lblDragDropHint);

            btnExtract = new Button
            {
                Text = "EXTRACT OIV",
                Location = new Point(555, 240),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(120, 18, 24),
                ForeColor = Color.FromArgb(240, 190, 190),
                FlatStyle = FlatStyle.Flat
            };
            btnExtract.FlatAppearance.BorderSize = 0;
            btnExtract.Click += BtnExtract_Click;

            Label logLabel = new Label
            {
                Text = "Log:",
                ForeColor = Color.FromArgb(140, 100, 100),
                AutoSize = true,
                Location = new Point(24, 285)
            };

            txtLog = new TextBox
            {
                Location = new Point(24, 305),
                Size = new Size(630, 120),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.FromArgb(180, 130, 130),
                BorderStyle = BorderStyle.FixedSingle
            };

            Controls.Add(title);
            Controls.Add(outputPathLabel);
            Controls.Add(txtOutputPath);
            Controls.Add(btnBrowseOutput);
            Controls.Add(chkNestedFolders);
            Controls.Add(dragDropPanel);
            Controls.Add(btnExtract);
            Controls.Add(logLabel);
            Controls.Add(txtLog);
        }

        private void LoadConfig()
        {
            string configPath = System.IO.Path.Combine(Application.UserAppDataPath, "extractConfig.txt");
            if (System.IO.File.Exists(configPath))
            {
                string[] lines = System.IO.File.ReadAllLines(configPath);
                if (lines.Length > 0 && System.IO.Directory.Exists(lines[0]))
                {
                    txtOutputPath.Text = lines[0];
                }
                if (lines.Length > 1 && bool.TryParse(lines[1], out bool nested))
                {
                    chkNestedFolders.Checked = nested;
                }
            }
        }

        private void SaveConfig()
        {
            string configPath = System.IO.Path.Combine(Application.UserAppDataPath, "extractConfig.txt");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(configPath)!);
            System.IO.File.WriteAllLines(configPath, new[] { txtOutputPath.Text, chkNestedFolders.Checked.ToString() });
        }

        public void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private void BtnBrowseOutput_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Select folder to extract to",
                ShowNewFolderButton = true
            };

            if (!string.IsNullOrWhiteSpace(txtOutputPath.Text) && System.IO.Directory.Exists(txtOutputPath.Text))
            {
                dlg.SelectedPath = txtOutputPath.Text;
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtOutputPath.Text = dlg.SelectedPath;
                SaveConfig();
                Log($"Output path set to: {dlg.SelectedPath}");
            }
        }

        private void DragDropPanel_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                if (files.Length == 1 && files[0].EndsWith(".oiv", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void DragDropPanel_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                if (files.Length == 1 && files[0].EndsWith(".oiv", StringComparison.OrdinalIgnoreCase))
                {
                    TryExtract(files[0]);
                }
            }
        }

        private void BtnExtract_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select OIV package to extract",
                Filter = "OpenIV Package (*.oiv)|*.oiv",
                Multiselect = false
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                TryExtract(dlg.FileName);
            }
        }

        private void TryExtract(string oivPath)
        {
            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                MessageBox.Show(this, "Please select an output folder first.", "Missing Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Log($"Selected OIV: {System.IO.Path.GetFileName(oivPath)}");
            SaveConfig();
            ExtractRequested?.Invoke(oivPath, OutputPath, UseNestedFolders);
        }
    }
}
