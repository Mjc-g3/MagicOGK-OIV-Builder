using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MagicOGK_OIV_Builder
{
    public partial class ExtractOivForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        private const string CONFIG_FILE = "settings.ini";

        public event Action<string, string, bool>? ExtractRequested;

        private bool isDragging = false;
        private Point dragStartPoint;

        public string? OutputPath => txtOutputPath?.Text;
        public bool UseNestedFolders => rbNested.Checked;

        public ExtractOivForm()
        {
            InitializeComponent();
            LoadConfig();
            Shown += ExtractOivForm_Shown;
            FormClosing += ExtractOivForm_FormClosing;
        }

        private void ExtractOivForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            SaveConfig();
        }

        private void ExtractOivForm_Shown(object? sender, EventArgs e)
        {
            if (contentPanel != null && centerPanel != null)
            {
                centerPanel.Location = new Point(
                    (contentPanel.ClientSize.Width - centerPanel.Width) / 2,
                    (contentPanel.ClientSize.Height - centerPanel.Height) / 2
                );
            }
        }

        private void panelTitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        }

        private void panelTitleBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point delta = new Point(e.X - dragStartPoint.X, e.Y - dragStartPoint.Y);
                Location = new Point(Location.X + delta.X, Location.Y + delta.Y);
            }
        }

        private void panelTitleBar_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void lblTitle_MouseDown(object? sender, MouseEventArgs e)
        {
            panelTitleBar_MouseDown(sender, e);
        }

        private void lblTitle_MouseMove(object? sender, MouseEventArgs e)
        {
            panelTitleBar_MouseMove(sender, e);
        }

        private void lblTitle_MouseUp(object? sender, MouseEventArgs e)
        {
            panelTitleBar_MouseUp(sender, e);
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void btnClose_MouseEnter(object? sender, EventArgs e)
        {
            if (btnClose != null)
                btnClose.BackColor = Color.FromArgb(200, 30, 30);
        }

        private void btnClose_MouseLeave(object? sender, EventArgs e)
        {
            if (btnClose != null)
                btnClose.BackColor = Color.Transparent;
        }

        private void btnBrowseOutput_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Select Output Directory",
                ShowNewFolderButton = true
            };

            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
            {
                txtOutputPath.Text = dlg.SelectedPath;
                Log($"Output path set to: {dlg.SelectedPath}");
            }
        }

        private void txtOutputPath_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void txtOutputPath_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                if (files.Length == 1)
                {
                    string path = files[0];
                    if (File.Exists(path))
                    {
                        path = Path.GetDirectoryName(path)!;
                    }
                    if (Directory.Exists(path))
                    {
                        txtOutputPath.Text = path;
                        Log($"Output path set to: {path}");
                    }
                }
            }
        }

        private void rbNested_CheckedChanged(object? sender, EventArgs e)
        {
        }

        private void rbFlat_CheckedChanged(object? sender, EventArgs e)
        {
        }

        private void dragDropPanel_DragEnter(object? sender, DragEventArgs e)
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

        private void dragDropPanel_DragDrop(object? sender, DragEventArgs e)
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

        private void btnExtract_Click(object? sender, EventArgs e)
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
                MessageBox.Show(this, "Please select an output directory first.", "Missing Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Log($"Selected OIV: {Path.GetFileName(oivPath)}");
            ExtractRequested?.Invoke(oivPath, OutputPath, UseNestedFolders);
        }

        public void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(Log), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
            txtLog.ScrollToCaret();
        }

        private string GetConfigPath() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIG_FILE);

        private void SaveConfig()
        {
            try
            {
                string configPath = GetConfigPath();
                using var writer = new StreamWriter(configPath);
                writer.WriteLine("[setting]");
                writer.WriteLine($"OutputPath={txtOutputPath.Text}");
                writer.WriteLine($"Mode={(rbNested.Checked ? "nested" : "flat")}");
            }
            catch { }
        }

        private void LoadConfig()
        {
            try
            {
                string configPath = GetConfigPath();
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("OutputPath="))
                            txtOutputPath.Text = line.Substring("OutputPath=".Length);
                        else if (line.StartsWith("Mode="))
                        {
                            string mode = line.Substring("Mode=".Length).Trim();
                            rbNested.Checked = (mode == "nested");
                            rbFlat.Checked = (mode == "flat");
                        }
                    }
                }
            }
            catch { }
        }
    }
}
