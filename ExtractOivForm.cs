using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MagicOGK_OIV_Builder
{
    public class ExtractOivForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        public event Action<string, string, bool>? ExtractRequested;

        private static readonly Color ThemeBg = Color.FromArgb(13, 13, 13);
        private static readonly Color ThemePanel = Color.FromArgb(18, 18, 18);
        private static readonly Color ThemeRedButton = Color.FromArgb(120, 18, 24);
        private static readonly Color ThemeRedHover = Color.FromArgb(125, 32, 38);
        private static readonly Color ThemeText = Color.FromArgb(210, 150, 150);
        private static readonly Color ThemeTextSoft = Color.FromArgb(170, 120, 120);
        private static readonly Color ThemeBorder = Color.FromArgb(90, 45, 45);

        private TextBox txtOutputPath = null!;
        private Button btnBrowseOutput = null!;
        private CheckBox chkNestedFolders = null!;
        private RadioButton rbNested = null!;
        private RadioButton rbFlat = null!;
        private Panel dragDropPanel = null!;
        private Label lblDragDropHint = null!;
        private Button btnExtract = null!;
        private ToolTip toolTip = null!;
        private TextBox txtLog = null!;
        private Panel panelTitleBar = null!;
        private Label lblTitle = null!;
        private Button btnClose = null!;
        private Panel contentPanel = null!;
        private Panel centerPanel = null!;
        private bool isDragging = false;
        private Point dragStartPoint;

        public string? OutputPath => txtOutputPath?.Text;
        public bool UseNestedFolders => chkNestedFolders?.Checked ?? true;

        public ExtractOivForm()
        {
            BuildUi();
            LoadConfig();
            Shown += (s, e) =>
            {
                // 初始化居中位置
                if (contentPanel != null && centerPanel != null)
                {
                    centerPanel.Location = new Point(
                        (contentPanel.ClientSize.Width - centerPanel.Width) / 2,
                        (contentPanel.ClientSize.Height - centerPanel.Height) / 2
                    );
                }
            };
        }

        private void BuildUi()
        {
            Text = "Extract OIV Package";
            Size = new Size(600, 720);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(16, 16, 16);
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;

            // =====================================================
            // 外边框层
            // =====================================================
            Panel borderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(180, 70, 70),
                Padding = new Padding(1)
            };
            Controls.Add(borderPanel);

            // =====================================================
            // 主内容容器层
            // =====================================================
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 16, 16)
            };
            borderPanel.Controls.Add(mainPanel);

            // =====================================================
            // 标题栏
            // =====================================================
            panelTitleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(10, 10, 10)
            };
            panelTitleBar.MouseDown += PanelTitleBar_MouseDown;
            panelTitleBar.MouseMove += PanelTitleBar_MouseMove;
            panelTitleBar.MouseUp += PanelTitleBar_MouseUp;

            btnClose = new Button
            {
                Text = "×",
                Dock = DockStyle.Right,
                Width = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = ThemeText,
                Font = new Font("Segoe UI", 14F),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(200, 30, 30);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
            btnClose.Click += BtnClose_Click;

            lblTitle = new Label
            {
                Text = "EXTRACT OIV",
                ForeColor = ThemeText,
                Font = new Font("Syne", 9F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            lblTitle.MouseDown += PanelTitleBar_MouseDown;
            lblTitle.MouseMove += PanelTitleBar_MouseMove;
            lblTitle.MouseUp += PanelTitleBar_MouseUp;

            panelTitleBar.Controls.Add(lblTitle);
            panelTitleBar.Controls.Add(btnClose);
            mainPanel.Controls.Add(panelTitleBar);

            // =====================================================
            // 主内容容器
            // =====================================================
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };
            mainPanel.Controls.Add(contentPanel);

            // =====================================================
            // 居中的内容容器
            // =====================================================
            centerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(540, 640),
                Anchor = AnchorStyles.None
            };
            contentPanel.Controls.Add(centerPanel);

            // =====================================================
            // 输出路径组
            // =====================================================
            Label lblOutput = new Label
            {
                Text = "OUTPUT DIRECTORY",
                ForeColor = ThemeText,
                Font = new Font("Syne", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            txtOutputPath = new TextBox
            {
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(200, 200, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(0, 28),
                Size = new Size(420, 28),
                Font = new Font("Segoe UI", 9F)
            };
            txtOutputPath.TextChanged += (s, e) => SaveConfig();

            btnBrowseOutput = new AnimatedGlowButton
            {
                Text = "BROWSE",
                Location = new Point(425, 26),
                Size = new Size(115, 32),
                Cursor = Cursors.Hand
            };
            btnBrowseOutput.Click += BtnBrowseOutput_Click;

            // =====================================================
            // 工具提示
            // =====================================================
            toolTip = new ToolTip();
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.AutoPopDelay = 5000;

            // =====================================================
            // 模式选择
            // =====================================================
            rbNested = new RadioButton
            {
                Text = "NESTED",
                ForeColor = ThemeText,
                BackColor = Color.Transparent,
                Font = new Font("Syne", 9F, FontStyle.Bold),
                Checked = true,
                AutoSize = true,
                Location = new Point(0, 72)
            };
            rbNested.CheckedChanged += (s, e) => 
            { 
                if (rbNested.Checked)
                {
                    chkNestedFolders.Checked = true;
                    SaveConfig();
                }
            };
            toolTip.SetToolTip(rbNested, "Keeps the real folder structure");

            rbFlat = new RadioButton
            {
                Text = "FLAT",
                ForeColor = ThemeText,
                BackColor = Color.Transparent,
                Font = new Font("Syne", 9F, FontStyle.Bold),
                Checked = false,
                AutoSize = true,
                Location = new Point(80, 72)
            };
            rbFlat.CheckedChanged += (s, e) => 
            { 
                if (rbFlat.Checked)
                {
                    chkNestedFolders.Checked = false;
                    SaveConfig();
                }
            };
            toolTip.SetToolTip(rbFlat, "Creates one folder per install path");

            // 隐藏的辅助控件
            chkNestedFolders = new CheckBox { Visible = false, Checked = true };

            // =====================================================
            // 拖放区域
            // =====================================================
            dragDropPanel = new Panel
            {
                BackColor = ThemePanel,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(0, 105),
                Size = new Size(540, 150),
                AllowDrop = true,
                Cursor = Cursors.Hand
            };

            lblDragDropHint = new Label
            {
                Text = "Drag & Drop OIV Package Here",
                ForeColor = ThemeTextSoft,
                Font = new Font("Syne", 10F, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(dragDropPanel.Width, dragDropPanel.Height),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };

            dragDropPanel.Controls.Add(lblDragDropHint);
            dragDropPanel.DragEnter += DragDropPanel_DragEnter;
            dragDropPanel.DragOver += DragDropPanel_DragEnter;
            dragDropPanel.DragDrop += DragDropPanel_DragDrop;

            // =====================================================
            // 提取按钮
            // =====================================================
            btnExtract = new AnimatedGlowButton
            {
                Text = "EXTRACT OIV",
                Location = new Point(0, 270),
                Size = new Size(540, 40),
                Cursor = Cursors.Hand
            };
            btnExtract.Click += BtnExtract_Click;

            // =====================================================
            // 日志区域
            // =====================================================
            Label lblLog = new Label
            {
                Text = "LOG",
                ForeColor = ThemeText,
                Font = new Font("Syne", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 325)
            };

            txtLog = new TextBox
            {
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(180, 180, 180),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(0, 350),
                Size = new Size(540, 295),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 8.5F)
            };

            // =====================================================
            // 组装
            // =====================================================
            centerPanel.Controls.Add(lblOutput);
            centerPanel.Controls.Add(txtOutputPath);
            centerPanel.Controls.Add(btnBrowseOutput);
            centerPanel.Controls.Add(rbNested);
            centerPanel.Controls.Add(rbFlat);
            centerPanel.Controls.Add(dragDropPanel);
            centerPanel.Controls.Add(btnExtract);
            centerPanel.Controls.Add(lblLog);
            centerPanel.Controls.Add(txtLog);

            // 居中布局
            contentPanel.Resize += (s, e) =>
            {
                if (contentPanel != null && centerPanel != null)
                {
                    centerPanel.Location = new Point(
                        (contentPanel.ClientSize.Width - centerPanel.Width) / 2,
                        (contentPanel.ClientSize.Height - centerPanel.Height) / 2
                    );
                }
            };
        }

        private void PanelTitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        }

        private void PanelTitleBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point delta = new Point(e.X - dragStartPoint.X, e.Y - dragStartPoint.Y);
                Location = new Point(Location.X + delta.X, Location.Y + delta.Y);
            }
        }

        private void PanelTitleBar_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void BtnBrowseOutput_Click(object? sender, EventArgs e)
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
                MessageBox.Show(this, "Please select an output directory first.", "Missing Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Log($"Selected OIV: {System.IO.Path.GetFileName(oivPath)}");
            SaveConfig();
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

        private void SaveConfig()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extract_config.ini");
                using var writer = new StreamWriter(configPath);
                writer.WriteLine($"OutputPath={txtOutputPath.Text}");
                writer.WriteLine($"UseNestedFolders={chkNestedFolders.Checked}");
            }
            catch { }
        }

        private void LoadConfig()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extract_config.ini");
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("OutputPath="))
                            txtOutputPath.Text = line.Substring("OutputPath=".Length);
                        else if (line.StartsWith("UseNestedFolders="))
                        {
                            bool nested = bool.Parse(line.Substring("UseNestedFolders=".Length));
                            chkNestedFolders.Checked = nested;
                            rbNested.Checked = nested;
                            rbFlat.Checked = !nested;
                        }
                    }
                }
            }
            catch { }
        }
    }
}
