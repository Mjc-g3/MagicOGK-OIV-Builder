using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.VisualBasic.Logging;
using Microsoft.Web.WebView2.Core;

namespace MagicOGK_OIV_Builder
{
    public partial class main : Form
    {

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        private bool isDragging = false;
        private Point dragStart;
        private string currentProjectPath = string.Empty;
        private OIVProject currentProject = new OIVProject();
        private bool sidebarExpanded = false;
        private bool editorExpanded = false;
        private string? selectedPhotoPath = null;
        private bool webViewReady = false;
        private bool isDirty = false;
        private bool isLoadingProject = false;

        private TreeView? editorTree = null;
        private Panel? editorPropPanel = null;

        private bool isResizingEditorPanel = false;
        private int editorResizeStartX;
        private int editorResizeStartWidth;
        private int editorCurrentWidth = 380;

        private int editorTarget => editorExpanded ? editorCurrentWidth : 0;

        private Panel? replaceScreenPanel = null;
        private FlowLayoutPanel? replaceVehicleList = null;
        private string? selectedReplaceVehicle = null;
        private Label? lblSelectedReplaceVehicle = null;

        private int vehiclePage = 0;
        private const int vehiclesPerPage = 60;
        private List<(string name, string spawn, string img)> allVehicles = new();

        private string? hoveredVehicle = null;
        private TextBox? txtVehicleSearch = null;
        private Button? btnVehiclePrev = null;
        private Button? btnVehicleNext = null;

        public main()
        {
            InitializeComponent();

            SetupLeftPanelControls();
            SetupRightPanelControls();

            StyleSidebarBtn(btnSidebarOpenProject, "    📦    Open Project", 120);
            StyleSidebarBtn(btnSidebarSaveProjectAs, "    📁    Save Project As", 178);
            StyleSidebarBtn(btnSidebarOpenOIV, "    🔎    Open OIV", 236);
            StyleSidebarBtn(btnSidebarBuildOIV, "    ⚒️    Build OIV", 294);
            StyleSidebarBtn(btnSidebarFeedback, "        Feedback", 580);

            btnReplaceMods.Click += btnReplaceMods_Click;

            this.Load += Form1_Load;
            this.FormClosing += Main_FormClosing;
            this.Resize += (s, e) =>
            {
                UpdateWindowButtonsLayout();
                PositionEditorPanel();
            };
            this.Opacity = 0;
            this.ShowInTaskbar = false;

            panelMarquee.MouseDown += DragWindow;
            ApplyTextboxTheme();

            dropdownVersionTag.DrawMode = DrawMode.OwnerDrawFixed;
            dropdownVersionTag.DropDownStyle = ComboBoxStyle.DropDownList;
            dropdownVersionTag.FlatStyle = FlatStyle.Flat;

            dropdownVersionTag.DrawItem += DropdownVersionTag_DrawItem;

            SetupMatrixRain();
            SetupLogo();

            panelMatrixTitle.Height = 520;
            panelMatrixTitle.SendToBack();

            btnSidebarOpenProject.BringToFront();
            btnSidebarSaveProjectAs.BringToFront();
            btnSidebarOpenOIV.BringToFront();
            btnSidebarBuildOIV.BringToFront();
            btnSidebarFeedback.BringToFront();
        }
        


        // ─────────────────── REPLACE MENU ───────────────────

        private void btnReplaceMods_Click(object sender, EventArgs e)
        {
            ShowReplaceModsScreen();
        }

        private void ShowReplaceModsScreen()
        {
            if (replaceScreenPanel != null)
            {
                replaceScreenPanel.Visible = true;
                replaceScreenPanel.BringToFront();
                panelSidebar.BringToFront();
                panelEditorRight.BringToFront();
                return;
            }

            replaceScreenPanel = new Panel
            {
                BackColor = Color.FromArgb(16, 16, 16),
                Location = new Point(panelSidebar.Width + 20, panelMarquee.Bottom + 10),
                Size = new Size(
                    this.ClientSize.Width - panelSidebar.Width - panelEditorRight.Width - 40,
                    this.ClientSize.Height - panelMarquee.Height - 20
                ),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(replaceScreenPanel);
            replaceScreenPanel.BringToFront();

            var title = new Label
            {
                Text = "REPLACE MENU",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 22)
            };


            btnVehiclePrev = CreateSecondaryButton("← Prev");
            btnVehiclePrev.Location = new Point(300, 110);

            btnVehicleNext = CreateSecondaryButton("Next →");
            btnVehicleNext.Location = new Point(450, 110);

            Button prev = btnVehiclePrev;
            Button next = btnVehicleNext;

            next.Click += (s, e) =>
            {
                if ((vehiclePage + 1) * vehiclesPerPage < GetFilteredVehicleCount())
                {
                    vehiclePage++;
                    LoadVehiclePage();
                    UpdateVehicleNavButtons();
                }
            };

            prev.Click += (s, e) =>
            {
                if (vehiclePage > 0)
                {
                    vehiclePage--;
                    LoadVehiclePage();
                    UpdateVehicleNavButtons();
                }
            };



            var back = new Button
            {
                Text = "← Back",
                Size = new Size(100, 32),
                Location = new Point(24, 62),
                BackColor = Color.FromArgb(55, 0, 0),
                ForeColor = Color.FromArgb(230, 170, 170),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 8F, FontStyle.Bold)
            };
            back.FlatAppearance.BorderColor = Color.FromArgb(120, 30, 30);
            back.Click += (s, e) => replaceScreenPanel.Visible = false;

            lblSelectedReplaceVehicle = new Label
            {
                Text = "Selected vehicle: none",
                ForeColor = Color.FromArgb(150, 100, 100),
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(140, 68)
            };

            var replaceBtn = new Button
            {
                Text = "Replace Selected Vehicle",
                Size = new Size(220, 36),
                Location = new Point(24, 110),
                BackColor = Color.FromArgb(90, 0, 0),
                ForeColor = Color.FromArgb(240, 180, 180),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 8F, FontStyle.Bold)
            };
            replaceBtn.FlatAppearance.BorderColor = Color.FromArgb(140, 40, 40);
            replaceBtn.Click += (s, e) => ReplaceSelectedVehicle();

            void UpdateNavButtons()
            {
                prev.Enabled = vehiclePage > 0;
                next.Enabled = (vehiclePage + 1) * vehiclesPerPage < allVehicles.Count;

                prev.ForeColor = prev.Enabled ? Color.FromArgb(200, 200, 200) : Color.FromArgb(90, 90, 90);
                next.ForeColor = next.Enabled ? Color.FromArgb(200, 200, 200) : Color.FromArgb(90, 90, 90);
            }

            next.Click += (s, e) =>
            {
                if ((vehiclePage + 1) * vehiclesPerPage < allVehicles.Count)
                {
                    vehiclePage++;
                    LoadVehiclePage();
                    UpdateNavButtons();
                }
            };

            prev.Click += (s, e) =>
            {
                if (vehiclePage > 0)
                {
                    vehiclePage--;
                    LoadVehiclePage();
                    UpdateNavButtons();
                }
            };

            var searchLabel = new Label
            {
                Text = "SEARCH VEHICLE",
                ForeColor = Color.FromArgb(188, 143, 143),
                Font = new Font("Syne", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(620, 82)
            };

            txtVehicleSearch = new TextBox
            {
                Size = new Size(300, 28),
                Location = new Point(620, 105),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F)
            };

            txtVehicleSearch.TextChanged += (s, e) =>
            {
                vehiclePage = 0;
                LoadVehiclePage();
                UpdateVehicleNavButtons();
            };

            replaceScreenPanel.Controls.Add(searchLabel);
            replaceScreenPanel.Controls.Add(txtVehicleSearch);

            replaceVehicleList = new FlowLayoutPanel
            {
                Location = new Point(24, 165),
                Size = new Size(replaceScreenPanel.Width - 48, replaceScreenPanel.Height - 190),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                WrapContents = true,
                BackColor = Color.FromArgb(12, 12, 12)
            };

            replaceScreenPanel.Controls.Add(title);
            replaceScreenPanel.Controls.Add(back);
            replaceScreenPanel.Controls.Add(lblSelectedReplaceVehicle);
            replaceScreenPanel.Controls.Add(replaceBtn);
            replaceScreenPanel.Controls.Add(replaceVehicleList);

            LoadReplaceVehicleCards();

            panelSidebar.BringToFront();
            panelEditorRight.BringToFront();

            replaceScreenPanel.Controls.Add(next);
            replaceScreenPanel.Controls.Add(prev);
            UpdateNavButtons();

        }
        private void UpdateVehicleNavButtons()
        {
            if (btnVehiclePrev == null || btnVehicleNext == null)
                return;

            int count = GetFilteredVehicleCount();
            int maxPage = Math.Max(0, (int)Math.Ceiling(count / (double)vehiclesPerPage) - 1);

            btnVehiclePrev.Enabled = vehiclePage > 0;
            btnVehicleNext.Enabled = vehiclePage < maxPage;

            btnVehiclePrev.ForeColor = btnVehiclePrev.Enabled
                ? Color.FromArgb(200, 200, 200)
                : Color.FromArgb(90, 90, 90);

            btnVehicleNext.ForeColor = btnVehicleNext.Enabled
                ? Color.FromArgb(200, 200, 200)
                : Color.FromArgb(90, 90, 90);
        }
        private int GetFilteredVehicleCount()
        {
            string search = txtVehicleSearch?.Text.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(search))
                return allVehicles.Count;

            return allVehicles.Count(v =>
                v.name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                v.spawn.Contains(search, StringComparison.OrdinalIgnoreCase)
            );
        }

        private Button CreateSecondaryButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(140, 36),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 8F, FontStyle.Bold),
                TabStop = false
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 65, 65);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(35, 35, 35);

            return btn;
        }
        private void LoadReplaceVehicleCards()
        {
            if (replaceVehicleList == null)
                return;

            string csvPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "vehicles",
                "vehicles.csv"
            );

            if (!File.Exists(csvPath))
                return;

            allVehicles.Clear();

            foreach (string line in File.ReadAllLines(csvPath).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(',');

                if (parts.Length < 3)
                    continue;

                allVehicles.Add((
                    parts[0].Trim(),
                    parts[1].Trim(),
                    parts[2].Trim()
                ));
            }

            vehiclePage = 0;
            LoadVehiclePage();
            UpdateVehicleNavButtons();
        }
        private void LoadVehiclePage()
        {
            if (replaceVehicleList == null)
                return;

            replaceVehicleList.Controls.Clear();

            string search = txtVehicleSearch?.Text.Trim() ?? "";

            var filtered = allVehicles.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(v =>
                    v.name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    v.spawn.Contains(search, StringComparison.OrdinalIgnoreCase)
                );
            }

            var filteredList = filtered.ToList();

            var pageItems = filteredList
                .Skip(vehiclePage * vehiclesPerPage)
                .Take(vehiclesPerPage);

            foreach (var v in pageItems)
            {
                replaceVehicleList.Controls.Add(
                    CreateVehicleReplaceCard(
                        v.name,
                        v.spawn,
                        GetVehicleImagePath(v.img)
                    )
                );
            }
        }

        private string GetVehicleImagePath(string imageName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string[] extensions = { ".png", ".jpg", ".jpeg", ".webp" };

            foreach (string ext in extensions)
            {
                string path = Path.Combine(baseDir, "Assets", "vehicles", imageName + ext);

                if (File.Exists(path))
                    return path;
            }

            return string.Empty;
        }

        private Panel CreateVehicleReplaceCard(string displayName, string spawnName, string imagePath)
        {
            var card = new Panel
            {
                Width = 160,
                Height = 150,
                Margin = new Padding(8),
                BackColor = Color.FromArgb(20, 20, 20),
                Cursor = Cursors.Hand,
                Tag = spawnName
            };

            card.Paint += (s, e) =>
            {
                bool isSelected = selectedReplaceVehicle == spawnName;
                bool isHovered = hoveredVehicle == spawnName;

                Color borderColor =
                    isSelected ? Color.FromArgb(220, 50, 50) :
                    isHovered ? Color.FromArgb(140, 40, 40) :
                                Color.FromArgb(50, 25, 25);

                int thickness = isSelected ? 2 : 1;

                using Pen pen = new Pen(borderColor, thickness);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);

                // subtle glow fill on hover
                if (isHovered)
                {
                    using Brush glow = new SolidBrush(Color.FromArgb(20, 120, 30, 30));
                    e.Graphics.FillRectangle(glow, 1, 1, card.Width - 2, card.Height - 2);
                }
            };

            card.MouseEnter += (s, e) =>
            {
                hoveredVehicle = spawnName;
                card.Invalidate();
            };

            card.MouseLeave += (s, e) =>
            {
                hoveredVehicle = null;
                card.Invalidate();
            };

            void HookHover(Control c)
            {
                c.MouseEnter += (s, e) =>
                {
                    hoveredVehicle = spawnName;
                    card.Invalidate();
                };

                c.MouseLeave += (s, e) =>
                {
                    hoveredVehicle = null;
                    card.Invalidate();
                };
            }

            var pic = new PictureBox
            {
                Size = new Size(140, 80),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(30, 30, 30),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                pic.Image = Image.FromFile(imagePath);
            }
            else
            {
                pic.Paint += (s, e) =>
                {
                    using var brush = new SolidBrush(Color.FromArgb(100, 60, 60));
                    using var font = new Font("Segoe UI", 8F, FontStyle.Bold);

                    string text = "No image";
                    SizeF size = e.Graphics.MeasureString(text, font);

                    e.Graphics.DrawString(
                        text,
                        font,
                        brush,
                        (pic.Width - size.Width) / 2,
                        (pic.Height - size.Height) / 2
                    );
                };
            }

            var name = new Label
            {
                Text = displayName,
                ForeColor = Color.FromArgb(220, 170, 170),
                Font = new Font("Syne", 8F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 96),
                Size = new Size(150, 22)
            };

            var spawn = new Label
            {
                Text = spawnName,
                ForeColor = Color.FromArgb(120, 85, 85),
                Font = new Font("Consolas", 8F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 120),
                Size = new Size(150, 18)
            };

            void SelectCard()
            {
                selectedReplaceVehicle = spawnName;

                if (lblSelectedReplaceVehicle != null)
                    lblSelectedReplaceVehicle.Text = $"Selected vehicle: {displayName} ({spawnName})";

                foreach (Control c in replaceVehicleList!.Controls)
                    c.Invalidate();
            }

            card.Click += (s, e) => SelectCard();
            pic.Click += (s, e) => SelectCard();
            name.Click += (s, e) => SelectCard();
            spawn.Click += (s, e) => SelectCard();

            card.Controls.Add(pic);
            card.Controls.Add(name);
            card.Controls.Add(spawn);

            HookHover(pic);
            HookHover(name);
            HookHover(spawn);

            return card;
        }
        private void ReplaceSelectedVehicle()
        {
            if (string.IsNullOrWhiteSpace(selectedReplaceVehicle))
            {
                MessageBox.Show(
                    "Select a vehicle first.",
                    "No vehicle selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            using var dlg = new OpenFileDialog
            {
                Title = $"Select replacement files for {selectedReplaceVehicle}",
                Multiselect = true,
                Filter = "Vehicle files (*.yft;*.ytd)|*.yft;*.ytd|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            int vehiclesRpfId = EnsureEditorPath(
                "mods/update/x64/dlcpacks/patchday8ng/dlc.rpf/x64/levels/gta5/vehicles.rpf"
            );

            foreach (string file in dlg.FileNames)
            {
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id = currentProject.NextId++,
                    SourcePath = file,
                    FileName = Path.GetFileName(file),
                    SubPath = string.Empty,
                    Type = "replace",
                    FolderId = vehiclesRpfId
                });
            }

            MarkDirty();

            if (editorExpanded)
            {
                BuildEditorPanel();
                SelectTreeNodeByFolderId(vehiclesRpfId);
            }

            RenderFileList();

            MessageBox.Show(
                $"Replacement files added for {selectedReplaceVehicle}.",
                "Vehicle Replace Added",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }


        // ─────────────────── UI ETC ───────────────────
        private void ApplyTextboxTheme()
        {
            StyleTextbox(txtAuthor);
            StyleTextbox(txtModName);
            StyleTextbox(txtVersion);
            StyleTextbox(txtDescription);
        }

        private void StyleTextbox(TextBox tb)
        {
            tb.BackColor = Color.FromArgb(35, 35, 35);
            tb.ForeColor = Color.FromArgb(220, 180, 180);
            tb.BorderStyle = BorderStyle.FixedSingle;
        }
        // ─────────────────── DRAGGING LOGIC ───────────────────
        private void DropdownVersionTag_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ComboBox cb = sender as ComboBox;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color bgColor = selected
                ? Color.FromArgb(60, 20, 20)   // hover/selected
                : Color.FromArgb(35, 35, 35);    // normal

            Color textColor = Color.FromArgb(220, 180, 180);

            using (SolidBrush bg = new SolidBrush(bgColor))
                e.Graphics.FillRectangle(bg, e.Bounds);

            using (SolidBrush fg = new SolidBrush(textColor))
                e.Graphics.DrawString(cb.Items[e.Index].ToString(), e.Font, fg, e.Bounds);

            e.DrawFocusRectangle();
        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        // ─────────────────── INIT ───────────────────

        private async void Form1_Load(object sender, EventArgs e)
        {

            using (var splash = new SplashForm())
            {
                splash.ShowDialog(this);
            }

            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Opacity = 1;
            this.Activate();

            // Drag
            panelDrag.MouseDown += PanelDrag_MouseDown;
            panelDrag.MouseMove += PanelDrag_MouseMove;
            panelDrag.MouseUp   += PanelDrag_MouseUp;

            // Drop zone
            panelDropZone.DragEnter += PanelDropZone_DragEnter;
            panelDropZone.DragDrop  += PanelDropZone_DragDrop;
            panelDropZone.AllowDrop  = true;

            // Button events
            btnAddFiles.Click        += btnAddFiles_Click;
            btnAddPhoto.Click        += btnAddPhoto_Click;
            btnOpenEditor.Click      += btnOpenEditor_Click;
            btnBuildOIV.Click        += btnBuildOIV_Click;
            panelColorPicker.Click   += panelColorPicker_Click;

            btnSidebarOpenProject.Click   += btnSidebarOpenProject_Click;
            btnSidebarSaveProjectAs.Click += btnSidebarSaveProjectAs_Click;
            btnSidebarOpenOIV.Click       += btnSidebarOpenOIV_Click;
            btnSidebarBuildOIV.Click      += btnBuildOIV_Click;
            btnSidebarFeedback.Click      += btnSidebarFeedback_Click;

            // Timers
            sidebarTimer.Interval = 12;
            sidebarTimer.Tick    += SidebarTimer_Tick;
            editorTimer.Interval  = 12;
            editorTimer.Tick     += EditorTimer_Tick;

            // Button hover effects
            AddButtonHover(btnOpenEditor);
            AddButtonHover(btnBuildOIV);
            AddButtonHover(btnAddFiles);
            AddButtonHover(btnAddPhoto);
            AddButtonHover(btnReplaceMods);

            // Photo preview paint
            panelPhotoPreview.Paint += PanelPhotoPreview_Paint;

            // WebView
            await webViewFileList.EnsureCoreWebView2Async();
            webViewFileList.CoreWebView2.Settings.IsZoomControlEnabled        = false;
            webViewFileList.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webViewFileList.CoreWebView2.Settings.AreDevToolsEnabled           = false;
            webViewFileList.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            webViewReady = true;
            RenderFileList();

            txtModName.TextChanged += Metadata_Changed;
            txtAuthor.TextChanged += Metadata_Changed;
            txtVersion.TextChanged += Metadata_Changed;
            txtDescription.TextChanged += Metadata_Changed;
            dropdownVersionTag.SelectedIndexChanged += Metadata_Changed;

            UpdateWindowButtonsLayout();
            SetupMarquee();
            SetupMatrixRain();
        }

        // ─────────────────── MARQUEE ANIMATION ───────────────────
        // "Build. Replace. Package." scrolls across the top drag bar.
        // The panel clips the text; gradient rectangles on each side fade it in/out.

        // Single instance of "Build. Replace. Package." travels left→right.
        // Starts fully off the left edge, fades in as it enters, fades out as it exits right.
        private float _marqueeX = 0f;
        private const float MarqueeSpeed = 1.5f;
        private const string MarqueeText = "Build.  Replace.  Package.";
        private System.Windows.Forms.Timer? _marqueeTimer;
        private float _marqueeTextWidth = 0f; // measured once on first tick

        private void SetupMarquee()
        {
            panelMarquee.Paint += MarqueePaint;

            _marqueeTimer          = new System.Windows.Forms.Timer();
            _marqueeTimer.Interval = 16;
            _marqueeTimer.Tick    += (s, e) =>
            {
                if (_marqueeTextWidth <= 0)
                {
                    using var g2   = Graphics.FromHwnd(panelMarquee.Handle);
                    using var mf   = new Font("Syne", 9.5F, FontStyle.Bold);
                    _marqueeTextWidth = g2.MeasureString(MarqueeText, mf).Width;
                    // Start fully off the left edge
                    _marqueeX = -_marqueeTextWidth;
                }

                _marqueeX += MarqueeSpeed;

                // Once the text has fully exited the right edge, restart from left
                if (_marqueeX > panelMarquee.Width)
                    _marqueeX = -_marqueeTextWidth;

                panelMarquee.Invalidate();
            };
            _marqueeTimer.Start();
        }

        private void MarqueePaint(object? sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            var rect = panelMarquee.ClientRectangle;
            g.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            g.Clear(Color.FromArgb(10, 10, 10));

            if (_marqueeTextWidth <= 0) return;

            using var font = new Font("Syne", 9.5F, FontStyle.Bold);
            float y = (rect.Height - font.Height) / 2f - 1;

            // Determine per-pixel alpha by how far the text is into the panel:
            // fade zone width on each edge
            int fadeW = 80;

            // Text left edge and right edge in panel coords
            float textLeft  = _marqueeX;
            float textRight = _marqueeX + _marqueeTextWidth;

            // Alpha based on position: 0 when fully outside, 255 in the middle
            // Left fade: text enters from the left — fade in as textLeft goes from -textWidth to fadeW
            // Right fade: text exits right — fade out as textRight goes from (panelW-fadeW) to panelW+textWidth
            float alpha = 1f;

            if (textLeft < fadeW)
            {
                // How far the leading edge has entered the fade zone
                float progress = (textLeft + _marqueeTextWidth) / (_marqueeTextWidth + fadeW);
                alpha = Math.Min(alpha, Math.Max(0f, progress));
            }
            if (textRight > rect.Width - fadeW)
            {
                float progress = (rect.Width - textLeft) / (_marqueeTextWidth + fadeW);
                alpha = Math.Min(alpha, Math.Max(0f, progress));
            }

            int a = (int)(alpha * 200);
            if (a < 3) return;

            using var brush = new SolidBrush(Color.FromArgb(a, 255, 255, 255));
            g.DrawString(MarqueeText, font, brush, _marqueeX, y);
        }

        // ─────────────────── MATRIX RAIN ANIMATION ───────────────────
        // Columns of falling characters in the sidebar title area.
        // Each column has a head (bright) and a tail that fades to black.

        private System.Windows.Forms.Timer? _matrixTimer;
        private int[]   _matrixY    = Array.Empty<int>();
        private int[]   _matrixSpeed = Array.Empty<int>();
        private char[]  _matrixHeadChar = Array.Empty<char>();
        private int     _matrixColW = 12;
        private int     _matrixCols = 0;

        private static readonly char[] MatrixChars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789<>[]{}|\\/-_=+*#@!?".ToCharArray();
        private static readonly Random MatrixRnd = new Random();

        private void SetupMatrixRain()
        {
            int w = panelMatrixTitle.Width;
            int h = panelMatrixTitle.Height;
            _matrixCols  = w / _matrixColW;
            _matrixY     = new int[_matrixCols];
            _matrixSpeed  = new int[_matrixCols];
            _matrixHeadChar = new char[_matrixCols];

            for (int i = 0; i < _matrixCols; i++)
            {
                _matrixY[i]        = MatrixRnd.Next(-h, 0);
                _matrixSpeed[i]    = MatrixRnd.Next(1, 4);
                _matrixHeadChar[i] = MatrixChars[MatrixRnd.Next(MatrixChars.Length)];
            }

            panelMatrixTitle.Paint += MatrixTitlePaint;

            _matrixTimer           = new System.Windows.Forms.Timer();
            _matrixTimer.Interval  = 75;
            _matrixTimer.Tick     += (s, e) =>
            {
                int h2 = panelMatrixTitle.Height;
                for (int i = 0; i < _matrixCols; i++)
                {
                    _matrixY[i] += _matrixSpeed[i] * _matrixColW;
                    if (_matrixY[i] > h2 + _matrixColW * 8)
                    {
                        _matrixY[i]        = MatrixRnd.Next(-h2, -_matrixColW);
                        _matrixSpeed[i]    = MatrixRnd.Next(1, 4);
                    }
                    // Randomise head char occasionally
                    if (MatrixRnd.Next(4) == 0)
                        _matrixHeadChar[i] = MatrixChars[MatrixRnd.Next(MatrixChars.Length)];
                }
                panelMatrixTitle.Invalidate();
            };
            _matrixTimer.Start();
        }

        private void MatrixTitlePaint(object? sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            int w    = panelMatrixTitle.Width;
            int h    = panelMatrixTitle.Height;
            int cw   = _matrixColW;
            int tailLen = 7; // how many chars trail behind the head

            panelMatrixTitle.BackColor = Color.FromArgb(15, 15, 15);
            //g.Clear(panelMatrixTitle.BackColor);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            using var font = new Font("Consolas", 7.5F, FontStyle.Bold);

            for (int i = 0; i < _matrixCols; i++)
            {
                int headY = _matrixY[i];

                // Draw tail — chars above the head, fading from dim red to black
                for (int t = 1; t <= tailLen; t++)
                {
                    int cy = headY - t * cw;
                    if (cy < -cw || cy > h) continue;

                    float alpha = 1f - (float)t / tailLen;
                    int   a     = (int)(alpha * alpha * 200); // quadratic falloff
                    if (a <= 5) continue;

                    // Pick a deterministic-ish char for the tail slot
                    char c = MatrixChars[(i * 7 + t * 13) % MatrixChars.Length];
                    using var tailBrush = new SolidBrush(Color.FromArgb(a, 100, 20, 20));
                    g.DrawString(c.ToString(), font, tailBrush, i * cw, cy);
                }

                // Draw head — bright reddish-white
                if (headY >= 0 && headY < h)
                {
                    using var headBrush = new SolidBrush(Color.FromArgb(240, 210, 160, 160));
                    g.DrawString(_matrixHeadChar[i].ToString(), font, headBrush, i * cw, headY);
                }
            }
        }

        // ─────────────────── WINDOW CONTROLS ───────────────────

        private void button5_Click(object sender, EventArgs e) => Application.Exit();
        private void button6_Click(object sender, EventArgs e)
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }
        private void button7_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void PanelDrag_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDragging = true; dragStart = e.Location; }
        }
        private void PanelDrag_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
                Location = new Point(Left + e.X - dragStart.X, Top + e.Y - dragStart.Y);
        }
        private void PanelDrag_MouseUp(object sender, MouseEventArgs e) => isDragging = false;

        // ─────────────────── SIDEBAR ───────────────────

        private void btnHamburger_Click(object sender, EventArgs e)
        {
            sidebarExpanded = !sidebarExpanded;
            sidebarTimer.Start();
        }

        private int sidebarTarget => sidebarExpanded ? 200 : 0;
        private void SidebarTimer_Tick(object sender, EventArgs e)
        {
            int diff = sidebarTarget - panelSidebar.Width;
            if (Math.Abs(diff) <= 3)
            {
                panelSidebar.Width = sidebarTarget;
                sidebarTimer.Stop();
            }
            else
            {
                panelSidebar.Width += diff / 3;
            }
        }

        private void btnSidebarOpenProject_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardOrSaveChanges())
                return;

            using var dlg = new OpenFileDialog
            {
                Title = "Open MagicOGK Project",
                Filter = "MagicOGK Project (*.mogk)|*.mogk|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                var proj = System.Text.Json.JsonSerializer.Deserialize<OIVProject>(File.ReadAllText(dlg.FileName));
                if (proj != null)
                {
                    currentProject     = proj;
                    currentProjectPath = dlg.FileName;
                    LoadProjectIntoUI();
                    MarkClean();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open project: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSidebarSaveProjectAs_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title    = "Save MagicOGK Project",
                Filter   = "MagicOGK Project (*.mogk)|*.mogk|All Files (*.*)|*.*",
                FileName = string.IsNullOrWhiteSpace(currentProject.ModName) ? "MyMod" : currentProject.ModName
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                SyncUIToProject();
                File.WriteAllText(dlg.FileName,
                    System.Text.Json.JsonSerializer.Serialize(currentProject,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                currentProjectPath = dlg.FileName;
                MarkClean();
                MessageBox.Show("Project saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSidebarOpenOIV_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "OIV Package (*.oiv)|*.oiv|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    { FileName = dlg.FileName, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSidebarFeedback_Click(object sender, EventArgs e)
        {
            var url = "https://forms.gle/tsbxGZUkxro11qYa6";

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        // ─────────────────── RIGHT EDITOR PANEL ───────────────────

        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            editorExpanded = !editorExpanded;
            if (editorExpanded)
                BuildEditorPanel();
            editorTimer.Start();
        }

        private void PositionEditorPanel()
        {
            panelEditorRight.Height = this.ClientSize.Height - panelMarquee.Height;
            panelEditorRight.Location = new Point(
                this.ClientSize.Width - panelEditorRight.Width,
                panelMarquee.Height
            );

            panelEditorRight.BringToFront();
        }

        private void EditorTimer_Tick(object sender, EventArgs e)
        {
            int diff = editorTarget - panelEditorRight.Width;

            if (Math.Abs(diff) <= 3)
            {
                panelEditorRight.Width = editorTarget;
                editorTimer.Stop();
            }
            else
            {
                panelEditorRight.Width += diff / 3;
            }

            PositionEditorPanel();
        }

        // ── Tree view state ───────────────────────────────────────────────────

        private void BuildEditorPanel()
        {
            panelEditorRight.Controls.Clear();
            editorTree = null;
            editorPropPanel = null;

            // ── RESIZE GRIP (ADD FIRST!) ───────────────────────────────
            Panel resizeGrip = new Panel
            {
                Dock = DockStyle.Left,
                Width = 6,
                Cursor = Cursors.SizeWE,
                BackColor = Color.FromArgb(90, 30, 30)
            };

            resizeGrip.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isResizingEditorPanel = true;
                    editorResizeStartX = Cursor.Position.X;
                    editorResizeStartWidth = panelEditorRight.Width;
                }
            };

            resizeGrip.MouseMove += (s, e) =>
            {
                if (!isResizingEditorPanel)
                    return;

                int diff = editorResizeStartX - Cursor.Position.X;
                int newWidth = editorResizeStartWidth + diff;

                int maxWidth = Math.Max(900, this.ClientSize.Width - 260);
                newWidth = Math.Max(320, Math.Min(maxWidth, newWidth));

                editorCurrentWidth = newWidth;
                panelEditorRight.Width = newWidth;
                PositionEditorPanel();
            };

            resizeGrip.MouseUp += (s, e) =>
            {
                isResizingEditorPanel = false;
            };

            panelEditorRight.Controls.Add(resizeGrip);

            // ── Header ────────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(24, 24, 24)
            };

            header.Controls.Add(new Label
            {
                Text = "FILE EDITOR",
                ForeColor = Color.FromArgb(188, 143, 143),
                Font = new Font("Syne", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(14, 13)
            });

            var btnClose = new Button
            {
                Text = "\u2715",
                ForeColor = Color.FromArgb(180, 80, 80),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Size = new Size(32, 32),
                Location = new Point(344, 6),
                TabStop = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, ev) => { editorExpanded = false; editorTimer.Start(); };
            header.Controls.Add(btnClose);

            // ── Toolbar ───────────────────────────────────────────────
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(18, 18, 18)
            };

            (string text, string tip, Action act)[] toolActions = {
        ("📁", "New Folder",       () => TreeCmd_NewFolder()),
        ("🗄", "New RPF Archive",  () => TreeCmd_NewRpf()),
        ("📂", "Import Folder",    () => TreeCmd_ImportFolder()),
        ("📄", "Add File Here",    () => TreeCmd_AddFile()),
        ("✏",  "Rename",          () => TreeCmd_Rename()),
        ("🗑", "Delete",           () => TreeCmd_Delete()),
    };

            int tbx = 6;
            foreach (var (text, tip, act) in toolActions)
            {
                var btn = new Button
                {
                    Text = text,
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(170, 120, 120),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(32, 28),
                    Location = new Point(tbx, 4),
                    Font = new Font("Segoe UI Emoji", 11F),
                    TabStop = false
                };

                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 30, 30);

                new ToolTip().SetToolTip(btn, tip);

                btn.Click += (s, ev) => act();
                toolbar.Controls.Add(btn);
                tbx += 34;
            }

            // ── Preset paths ─────────────────────────────────────────
            var presetPanel = BuildPresetPathPanel();

            // ── Properties panel ─────────────────────────────────────
            editorPropPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 0,
                BackColor = Color.FromArgb(20, 10, 10)
            };

            // ── TreeView ─────────────────────────────────────────────
            editorTree = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 15, 15),
                ForeColor = Color.FromArgb(210, 180, 180),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5F),
                ItemHeight = 24,
                ShowLines = false,
                ShowPlusMinus = false,
                ShowRootLines = false,
                FullRowSelect = true,
                HideSelection = false,
                AllowDrop = true,
                DrawMode = TreeViewDrawMode.OwnerDrawAll,
                Indent = 24
            };

            editorTree.DrawNode += EditorTree_DrawNode;
            editorTree.AfterSelect += EditorTree_AfterSelect;
            editorTree.KeyDown += EditorTree_KeyDown;
            editorTree.MouseDoubleClick += EditorTree_MouseDoubleClick;
            editorTree.NodeMouseClick += EditorTree_NodeMouseClick;
            editorTree.ItemDrag += EditorTree_ItemDrag;
            editorTree.DragEnter += EditorTree_DragEnter;
            editorTree.DragOver += EditorTree_DragOver;
            editorTree.DragDrop += EditorTree_DragDrop;

            // ── ADD ORDER (IMPORTANT) ────────────────────────────────
            panelEditorRight.Controls.Add(editorTree);
            panelEditorRight.Controls.Add(editorPropPanel);
            panelEditorRight.Controls.Add(presetPanel);
            panelEditorRight.Controls.Add(toolbar);
            panelEditorRight.Controls.Add(header);

            RebuildTree();
        }

        private Panel BuildPresetPathPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 104,
                BackColor = Color.FromArgb(16, 16, 16)
            };

            var lbl = new Label
            {
                Text = "PRESET PATHS",
                ForeColor = Color.FromArgb(120, 80, 80),
                Font = new Font("Syne", 7F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 8)
            };

            panel.Controls.Add(lbl);

            AddPresetButton(panel, "dlcpacks", "update/x64/dlcpacks", 10, 30);
            AddPresetButton(panel, "levels/gta5", "x64/levels/gta5", 104, 30);
            AddPresetButton(panel, "common/data", "common/data", 198, 30);
            AddPresetButton(panel, "scaleform", "update/update.rpf/x64/patch/data/cdimages/scaleform_generic.rpf", 292, 30);

            AddReplacePresetButton(panel, 10, 66);

            return panel;
        }



        private void AddReplacePresetButton(Panel parent, int x, int y)
        {
            var btn = CreatePresetButton("replace", x, y, 130);
            btn.Click += (s, e) => ShowReplacePresetWindow();
            parent.Controls.Add(btn);
        }
        private void ShowReplacePresetWindow()
        {
            Form win = new Form
            {
                Size = new Size(360, 240),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(18, 18, 18),
                ShowInTaskbar = false,
                Padding = new Padding(2)
            };

            var borderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(120, 20, 20),
                Padding = new Padding(1)
            };

            var innerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 36, 18, 18),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.FromArgb(18, 18, 18)
            };

            void DragPopup(object? s, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(win.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            }

            win.MouseDown += DragPopup;
            borderPanel.MouseDown += DragPopup;
            innerPanel.MouseDown += DragPopup;
            panel.MouseDown += DragPopup;

            Button close = new Button
            {
                Text = "X",
                Size = new Size(30, 24),
                Location = new Point(win.Width - 38, 6),
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.FromArgb(220, 120, 120),
                FlatStyle = FlatStyle.Flat,
                TabStop = false
            };

            close.FlatAppearance.BorderSize = 0;
            close.Click += (s, e) => win.Close();

            Button weapon = CreateDialogButton("Weapons");
            weapon.Click += (s, e) =>
            {
                win.Close();
                CreateReplacePresetPath(
                    "update/x64/dlcpacks/patchday8ng/dlc.rpf/x64/models/cdimages/weapons.rpf",
                    "Weapon Replace Preset"
                );
            };

            Button car = CreateDialogButton("Cars");
            car.Click += (s, e) =>
            {
                win.Close();
                CreateReplacePresetPath(
                    "update/x64/dlcpacks/patchday8ng/dlc.rpf/x64/levels/gta5/vehicles.rpf",
                    "Car Replace Preset"
                );
            };

            Button clothes = CreateDialogButton("Clothes");
            clothes.Click += (s, e) =>
            {
                panel.Controls.Clear();
                AddClothesReplaceButtons(panel, win);
            };

            panel.Controls.Add(weapon);
            panel.Controls.Add(car);
            panel.Controls.Add(clothes);

            innerPanel.Controls.Add(panel);
            innerPanel.Controls.Add(close);
            close.BringToFront();

            borderPanel.Controls.Add(innerPanel);
            win.Controls.Add(borderPanel);

            win.ShowDialog(this);
        }

        private Button CreateDialogButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                Width = 300,
                Height = 38,
                Margin = new Padding(0, 0, 0, 10),
                BackColor = Color.FromArgb(35, 10, 10),
                ForeColor = Color.FromArgb(220, 170, 170),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(85, 30, 30);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 15, 15);

            return btn;
        }
        private void AddClothesReplaceButtons(FlowLayoutPanel panel, Form win)
        {
            Button back = CreateDialogButton("← Back");
            back.Click += (s, e) =>
            {
                win.Close();
                ShowReplacePresetWindow();
            };

            Button franklin = CreateDialogButton("Franklin");
            franklin.Click += (s, e) =>
            {
                win.Close();
                CreateReplacePresetPath(
                    "x64v.rpf/models/cdimages/streamedpeds_players.rpf/player_one",
                    "Franklin Clothes Replace Preset"
                );
            };

            Button michael = CreateDialogButton("Michael");
            michael.Click += (s, e) =>
            {
                win.Close();
                CreateReplacePresetPath(
                    "x64v.rpf/models/cdimages/streamedpeds_players.rpf/player_zero",
                    "Michael Clothes Replace Preset"
                );
            };

            Button trevor = CreateDialogButton("Trevor");
            trevor.Click += (s, e) =>
            {
                win.Close();
                CreateReplacePresetPath(
                    "x64v.rpf/models/cdimages/streamedpeds_players.rpf/player_two",
                    "Trevor Clothes Replace Preset"
                );
            };

            panel.Controls.Add(back);
            panel.Controls.Add(franklin);
            panel.Controls.Add(michael);
            panel.Controls.Add(trevor);
        }
        private void CreateReplacePresetPath(string path, string title)
        {
            int folderId = EnsureEditorPath(path);

            MarkDirty();
            RebuildTree();
            SelectTreeNodeByFolderId(folderId);
            RenderFileList();

            DialogResult result = MessageBox.Show(
                "Replacement path created.\n\nDo you want to import replacement files now?",
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                TreeCmd_AddReplaceFilesToFolder(folderId);
            }
        }

        private void AddPresetButton(Panel parent, string text, string path, int x, int y)
        {
            var btn = new Button
            {
                Text = text,
                Tag = path,
                Size = new Size(86, 26),
                Location = new Point(x, y),
                BackColor = Color.FromArgb(35, 10, 10),
                ForeColor = Color.FromArgb(190, 135, 135),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 7F, FontStyle.Bold),
                TabStop = false
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 30, 30);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 15, 15);

            btn.Click += (s, e) =>
            {
                AddPresetPath(path);
            };

            parent.Controls.Add(btn);
        }

        private void AddWeaponReplacePresetButton(Panel parent, int x, int y)
        {
            var btn = CreatePresetButton("weapon replace", x, y, 130);
            btn.Click += (s, e) => TreeCmd_WeaponReplacePreset();
            parent.Controls.Add(btn);
        }
        private void AddCarReplacePresetButton(Panel parent, int x, int y)
        {
            var btn = CreatePresetButton("car replace", x, y, 130);
            btn.Click += (s, e) => TreeCmd_CarReplacePreset();
            parent.Controls.Add(btn);
        }

        private void AddClothesReplacePresetButton(Panel parent, int x, int y)
        {
            var btn = CreatePresetButton("clothes replace", x, y, 130);
            btn.Click += (s, e) => TreeCmd_ClothesReplacePreset();
            parent.Controls.Add(btn);
        }

        private Button CreatePresetButton(string text, int x, int y, int width)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, 26),
                Location = new Point(x, y),
                BackColor = Color.FromArgb(35, 10, 10),
                ForeColor = Color.FromArgb(190, 135, 135),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 7F, FontStyle.Bold),
                TabStop = false
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 30, 30);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 15, 15);

            return btn;
        }
        private void AddPresetPath(string path)
        {
            int? parentId = null;
            OIVFolder? lastFolder = null;

            string[] parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                var existing = currentProject.Folders.FirstOrDefault(f =>
                    f.ParentId == parentId &&
                    string.Equals(f.Name, part, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    existing = new OIVFolder
                    {
                        Id = currentProject.NextId++,
                        Name = part,
                        ParentId = parentId,
                        IsRpf = part.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase)
                    };

                    currentProject.Folders.Add(existing);
                }

                lastFolder = existing;
                parentId = existing.Id;
            }

            MarkDirty();
            RebuildTree();
            RenderFileList();

            if (lastFolder != null)
                SelectTreeNodeByFolderId(lastFolder.Id);
        }
        private int EnsureEditorPath(string path)
        {
            int? parentId = null;
            OIVFolder? lastFolder = null;

            string[] parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                var existing = currentProject.Folders.FirstOrDefault(f =>
                    f.ParentId == parentId &&
                    string.Equals(f.Name, part, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    existing = new OIVFolder
                    {
                        Id = currentProject.NextId++,
                        Name = part,
                        ParentId = parentId,
                        IsRpf = part.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase)
                    };

                    currentProject.Folders.Add(existing);
                }

                lastFolder = existing;
                parentId = existing.Id;
            }

            return lastFolder!.Id;
        }

        // ─── REBUILD TREE HELPER ───────────────────────────────────────────────────
        private string GetNodeKey(TreeNode node)
        {
            if (node.Tag is OIVFolder folder)
                return $"folder:{folder.Id}";

            if (node.Tag is OIVFileEntry file)
                return $"file:{file.Id}";

            return "root";
        }

        private HashSet<string> GetExpandedNodeKeys()
        {
            var expanded = new HashSet<string>();

            if (editorTree == null)
                return expanded;

            foreach (TreeNode node in editorTree.Nodes)
                CollectExpandedNodeKeys(node, expanded);

            return expanded;
        }

        private void CollectExpandedNodeKeys(TreeNode node, HashSet<string> expanded)
        {
            if (node.IsExpanded)
                expanded.Add(GetNodeKey(node));

            foreach (TreeNode child in node.Nodes)
                CollectExpandedNodeKeys(child, expanded);
        }

        private void RestoreExpandedNodeKeys(TreeNodeCollection nodes, HashSet<string> expanded)
        {
            foreach (TreeNode node in nodes)
            {
                if (expanded.Contains(GetNodeKey(node)))
                    node.Expand();

                RestoreExpandedNodeKeys(node.Nodes, expanded);
            }
        }

        private void SelectTreeNodeByFolderId(int folderId)
        {
            if (editorTree == null) return;

            foreach (TreeNode node in editorTree.Nodes)
            {
                var found = FindFolderNode(node, folderId);
                if (found != null)
                {
                    editorTree.SelectedNode = found;
                    found.EnsureVisible();
                    return;
                }
            }
        }

        private TreeNode? FindFolderNode(TreeNode node, int folderId)
        {
            if (node.Tag is OIVFolder folder && folder.Id == folderId)
                return node;

            foreach (TreeNode child in node.Nodes)
            {
                var found = FindFolderNode(child, folderId);
                if (found != null)
                    return found;
            }

            return null;
        }

        // ─── Tree building ────────────────────────────────────────────────────

        private void RebuildTree()
        {
            if (editorTree == null) return;

            // Save which nodes are currently expanded
            var expandedNodes = GetExpandedNodeKeys();

            editorTree.BeginUpdate();
            editorTree.Nodes.Clear();

            var root = new TreeNode("Root") { Tag = "root" };
            root.ForeColor = Color.FromArgb(190, 150, 150);
            root.NodeFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            // Recursively add folder nodes and their files
            AddFolderNodes(root, parentId: null);

            // Unassigned files go under root directly
            foreach (var file in currentProject.Files.Where(f => !f.FolderId.HasValue))
                root.Nodes.Add(MakeFileNode(file));

            editorTree.Nodes.Add(root);

            // Keep root open
            root.Expand();

            // Restore previously expanded folders
            RestoreExpandedNodeKeys(editorTree.Nodes, expandedNodes);

            editorTree.EndUpdate();
        }

        private void AddFolderNodes(TreeNode parent, int? parentId)
        {
            foreach (var folder in currentProject.Folders.Where(f => f.ParentId == parentId))
            {
                var node = MakeFolderNode(folder);
                // Recurse into children
                AddFolderNodes(node, folder.Id);
                // Files assigned to this folder
                foreach (var file in currentProject.Files.Where(f => f.FolderId == folder.Id))
                    node.Nodes.Add(MakeFileNode(file));
                parent.Nodes.Add(node);
            }
        }

        private TreeNode MakeFolderNode(OIVFolder folder)
        {
            return new TreeNode(folder.Name)
            {
                Tag = folder,
                ForeColor = folder.IsRpf
                    ? Color.FromArgb(150, 200, 255)
                    : Color.FromArgb(220, 160, 160)
            };
        }

        private TreeNode MakeFileNode(OIVFileEntry file)
        {
            return new TreeNode(file.FileName)
            {
                Tag = file,
                ForeColor = Color.FromArgb(180, 180, 180)
            };
        }

        // ─── Tree drawing ─────────────────────────────────────────────────────

        private void EditorTree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (editorTree == null || e.Node == null) return;

            Graphics g = e.Graphics;
            Rectangle row = new Rectangle(0, e.Bounds.Y, editorTree.Width, e.Bounds.Height);
            bool selected = (e.State & TreeNodeStates.Selected) != 0;
            bool hasChildren = e.Node.Nodes.Count > 0;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Row background
            using (var bg = new SolidBrush(selected
                ? Color.FromArgb(85, 35, 35)
                : Color.FromArgb(15, 15, 15)))
            {
                g.FillRectangle(bg, row);
            }

            // Optional subtle guide line for nested items
            if (e.Node.Level > 0)
            {
                int guideX = 12 + ((e.Node.Level - 1) * editorTree.Indent) + 8;
                using var guidePen = new Pen(Color.FromArgb(38, 55, 55, 55));
                g.DrawLine(guidePen, guideX, row.Top, guideX, row.Bottom);
            }

            int baseX = 12 + (e.Node.Level * editorTree.Indent);
            int centerY = row.Top + row.Height / 2;

            // Expand/collapse arrow
            if (hasChildren)
            {
                Point[] arrow;

                if (e.Node.IsExpanded)
                {
                    // Down arrow
                    arrow = new[]
                    {
                new Point(baseX,     centerY - 3),
                new Point(baseX + 8, centerY - 3),
                new Point(baseX + 4, centerY + 2)
            };
                }
                else
                {
                    // Right arrow
                    arrow = new[]
                    {
                new Point(baseX + 1, centerY - 4),
                new Point(baseX + 1, centerY + 4),
                new Point(baseX + 6, centerY)
            };
                }

                using var arrowBrush = new SolidBrush(Color.FromArgb(150, 120, 120));
                g.FillPolygon(arrowBrush, arrow);
            }

            int iconX = baseX + (hasChildren ? 14 : 4);

            // Icon
            string iconText;
            Color iconColor;

            if (e.Node.Tag is OIVFolder folder)
            {
                iconText = folder.IsRpf ? "🗄" : "📁";
                iconColor = folder.IsRpf
                    ? Color.FromArgb(150, 200, 255)
                    : Color.FromArgb(220, 170, 120);
            }
            else
            {
                iconText = "📄";
                iconColor = Color.FromArgb(170, 170, 170);
            }

            using (var iconBrush = new SolidBrush(iconColor))
            {
                g.DrawString(iconText, editorTree.Font, iconBrush, iconX, row.Top + 2);
            }

            int textX = iconX + 20;

            using (var textBrush = new SolidBrush(
                selected
                    ? Color.FromArgb(255, 220, 220)
                    : e.Node.ForeColor))
            {
                g.DrawString(e.Node.Text, e.Node.NodeFont ?? editorTree.Font, textBrush, textX, row.Top + 3);
            }

            // Thin separator for a cleaner list feel
            using var sepPen = new Pen(Color.FromArgb(18, 255, 255, 255));
            g.DrawLine(sepPen, 0, row.Bottom - 1, editorTree.Width, row.Bottom - 1);
        }

        // ─── Tree selection → properties panel ───────────────────────────────

        private void EditorTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (editorPropPanel == null || e.Node?.Tag == null) return;
            ShowPropertiesFor(e.Node.Tag);
        }

        private void ShowPropertiesFor(object tag)
        {
            if (editorPropPanel == null) return;
            editorPropPanel.Controls.Clear();

            if (tag is OIVFolder folder)
            {
                editorPropPanel.Height = 95;

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = folder.IsRpf ? "RPF ARCHIVE" : "FOLDER",
                    ForeColor = Color.FromArgb(140, 90, 90),
                    Font      = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 8)
                });

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = "NAME:",
                    ForeColor = Color.FromArgb(120, 80, 80),
                    Font      = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 26)
                });
                var txtName = new TextBox
                {
                    BackColor   = Color.Black,
                    ForeColor   = Color.FromArgb(210, 210, 210),
                    BorderStyle = BorderStyle.FixedSingle,
                    Text        = folder.Name,
                    Size        = new Size(220, 20),
                    Location    = new Point(56, 23),
                    Font        = new Font("Consolas", 8F)
                };
                txtName.TextChanged += (s, ev) =>
                {
                    folder.Name = txtName.Text;
                    MarkDirty();
                    RebuildTree();
                    RenderFileList();
                };
                editorPropPanel.Controls.Add(txtName);

                var chkDlc = new CheckBox
                {
                    Text      = "Add to dlclist.xml on install",
                    ForeColor = Color.FromArgb(170, 130, 130),
                    BackColor = Color.Transparent,
                    Font      = new Font("Syne", 7F),
                    Checked   = folder.AddToDlcList,
                    AutoSize  = true,
                    Location  = new Point(10, 50)
                };
                chkDlc.CheckedChanged += (s, ev) =>
                {
                    folder.AddToDlcList = chkDlc.Checked;
                    MarkDirty();
                    RenderFileList();
                };
                editorPropPanel.Controls.Add(chkDlc);

                var chkRpf = new CheckBox
                {
                    Text      = "This is an RPF archive",
                    ForeColor = Color.FromArgb(130, 160, 200),
                    BackColor = Color.Transparent,
                    Font      = new Font("Syne", 7F),
                    Checked   = folder.IsRpf,
                    AutoSize  = true,
                    Location  = new Point(10, 70)
                };
                chkRpf.CheckedChanged += (s, ev) =>
                {
                    folder.IsRpf = chkRpf.Checked;
                    RebuildTree();
                };
                editorPropPanel.Controls.Add(chkRpf);
            }
            else if (tag is OIVFileEntry file)
            {
                editorPropPanel.Height = 95;

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = "FILE",
                    ForeColor = Color.FromArgb(140, 90, 90),
                    Font      = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 8)
                });

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = TruncatePath(file.SourcePath, 48),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Font      = new Font("Consolas", 7F),
                    AutoSize  = true,
                    Location  = new Point(10, 26)
                });

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = "TYPE:",
                    ForeColor = Color.FromArgb(120, 80, 80),
                    Font      = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 50)
                });

                var typeBox = new ComboBox
                {
                    BackColor     = Color.Black,
                    ForeColor     = Color.FromArgb(200, 200, 200),
                    FlatStyle     = FlatStyle.Flat,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Size          = new Size(110, 20),
                    Location      = new Point(48, 48),
                    Font          = new Font("Syne", 7F)
                };
                typeBox.Items.AddRange(new object[] { "Content", "Replace", "XML Edit" });
                typeBox.SelectedIndex = file.Type switch { "replace" => 1, "xmledit" => 2, _ => 0 };
                typeBox.SelectedIndexChanged += (s, ev) =>
                    file.Type = typeBox.SelectedIndex switch { 1 => "replace", 2 => "xmledit", _ => "content" };
                editorPropPanel.Controls.Add(typeBox);

                // Full resolved path preview
                string resolved = ResolveFilePath(file);
                editorPropPanel.Controls.Add(new Label
                {
                    Text      = "→ " + TruncatePath(resolved, 50),
                    ForeColor = Color.FromArgb(100, 130, 100),
                    Font      = new Font("Consolas", 7F),
                    AutoSize  = true,
                    Location  = new Point(10, 72)
                });
            }
            else
            {
                editorPropPanel.Height = 0;
            }
        }

        // Resolve the full install path for a file by walking up the folder tree
        private string ResolveFilePath(OIVFileEntry file)
        {
            var parts = new List<string>();

            int? cur = file.FolderId;
            while (cur.HasValue)
            {
                var folder = currentProject.Folders.Find(f => f.Id == cur.Value);
                if (folder == null) break;

                parts.Insert(0, folder.Name);
                cur = folder.ParentId;
            }

            if (!string.IsNullOrWhiteSpace(file.SubPath))
            {
                foreach (var part in file.SubPath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries))
                    parts.Add(part);
            }

            parts.Add(file.FileName);

            return string.Join("\\", parts);
        }

        // ─── Toolbar commands ─────────────────────────────────────────────────

        private int? SelectedFolderId()
        {
            var tag = editorTree?.SelectedNode?.Tag;
            if (tag is OIVFolder f) return f.Id;
            if (tag is OIVFileEntry file) return file.FolderId;
            return null; // root or nothing selected
        }

        private string ResolveFolderPath(OIVFolder folder)
        {
            var parts = new List<string>();

            OIVFolder? current = folder;

            while (current != null)
            {
                parts.Insert(0, current.Name);

                if (!current.ParentId.HasValue)
                    break;

                current = currentProject.Folders.FirstOrDefault(f => f.Id == current.ParentId.Value);
            }

            return string.Join("/", parts);
        }

        private bool IsDirectChildOfDlcpacks(OIVFolder folder)
        {
            string path = ResolveFolderPath(folder).Replace("\\", "/").Trim('/');

            // Direct folders only:
            // update/x64/dlcpacks/mycar
            // NOT update/x64/dlcpacks/mycar/something
            string prefix = "update/x64/dlcpacks/";

            if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            string remaining = path.Substring(prefix.Length);

            return !remaining.Contains("/");
        }

        private bool IsDlcpacksFolder(OIVFolder folder)
        {
            string path = ResolveFolderPath(folder).Replace("\\", "/").Trim('/');
            return string.Equals(path, "update/x64/dlcpacks", StringComparison.OrdinalIgnoreCase);
        }

        private void TreeCmd_NewFolder() => TreeCmd_NewNode(isRpf: false);
        private void TreeCmd_NewRpf()    => TreeCmd_NewNode(isRpf: true);

        private void TreeCmd_NewNode(bool isRpf)
        {
            string kind = isRpf ? "RPF archive" : "folder";
            string name = PromptName($"New {kind} name:", isRpf ? "myarchive.rpf" : "MyFolder");
            if (string.IsNullOrWhiteSpace(name)) return;

            int? parentId = SelectedFolderId();

            var newFolder = new OIVFolder
            {
                Id = currentProject.NextId++,
                Name = name,
                ParentId = parentId,
                IsRpf = isRpf
            };

            currentProject.Folders.Add(newFolder);

            // Auto-toggle only if it is directly inside update/x64/dlcpacks
            newFolder.AddToDlcList = !isRpf && IsDirectChildOfDlcpacks(newFolder);

            MarkDirty();
            RebuildTree();

            // If creating inside dlcpacks, keep dlcpacks selected.
            // This prevents the next folder from being created inside the new folder.
            if (parentId.HasValue)
                SelectTreeNodeByFolderId(parentId.Value);
            else
                SelectTreeNodeByFolderId(newFolder.Id);

            RenderFileList();
        }

        private void TreeCmd_AddFile()
        {
            int? parentFolderId = SelectedFolderId();

            using var dlg = new OpenFileDialog
            {
                Title       = "Add file to selected folder",
                Multiselect = true,
                Filter      = "All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            foreach (string path in dlg.FileNames)
            {
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id         = currentProject.NextId++,
                    SourcePath = path,
                    FileName   = Path.GetFileName(path),
                    SubPath    = string.Empty,
                    Type       = "content",
                    FolderId   = parentFolderId
                });
            }
            MarkDirty();
            RebuildTree();
            RenderFileList();
        }
        private void TreeCmd_AddReplaceFilesToFolder(int folderId)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select replacement file(s)",
                Multiselect = true,
                Filter = "All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            foreach (string path in dlg.FileNames)
            {
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id = currentProject.NextId++,
                    SourcePath = path,
                    FileName = Path.GetFileName(path),
                    SubPath = string.Empty,
                    Type = "replace",
                    FolderId = folderId
                });
            }

            MarkDirty();
            RebuildTree();
            SelectTreeNodeByFolderId(folderId);
            RenderFileList();
        }
        private void TreeCmd_WeaponReplacePreset()
        {
            int weaponsRpfId = EnsureEditorPath(
                "update/x64/dlcpacks/patchday8ng/dlc.rpf/x64/models/cdimages/weapons.rpf"
            );

            MarkDirty();
            RebuildTree();
            SelectTreeNodeByFolderId(weaponsRpfId);
            RenderFileList();

            DialogResult result = MessageBox.Show(
                "Weapon replacement path created.\n\nDo you want to import replacement files now?",
                "Weapon Replace Preset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                TreeCmd_AddReplaceFilesToFolder(weaponsRpfId);
            }
        }
        private void TreeCmd_CarReplacePreset()
        {
            int vehiclesRpfId = EnsureEditorPath(
                "update/x64/dlcpacks/patchday8ng/dlc.rpf/x64/levels/gta5/vehicles.rpf"
            );

            MarkDirty();
            RebuildTree();
            SelectTreeNodeByFolderId(vehiclesRpfId);
            RenderFileList();

            if (MessageBox.Show(
                "Car replacement path created.\n\nDo you want to import replacement files now?",
                "Car Replace Preset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                TreeCmd_AddReplaceFilesToFolder(vehiclesRpfId);
            }
        }

        private void TreeCmd_ClothesReplacePreset()
        {
            int clothesRpfId = EnsureEditorPath(
                "x64v.rpf/models/cdimages/streamedpeds_players.rpf/player_one"
            );

            MarkDirty();
            RebuildTree();
            SelectTreeNodeByFolderId(clothesRpfId);
            RenderFileList();

            if (MessageBox.Show(
                "Clothes replacement path created.\n\nDo you want to import replacement files now?",
                "Clothes Replace Preset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                TreeCmd_AddReplaceFilesToFolder(clothesRpfId);
            }
        }
        private void TreeCmd_ImportFolder()
        {
            int? parentFolderId = SelectedFolderId();

            string[] folders = MultiFolderPicker.PickFolders();

            if (folders.Length == 0)
                return;

            foreach (string folderPath in folders)
            {
                if (Directory.Exists(folderPath))
                    ImportFolderRecursive(folderPath, parentFolderId);
            }

            MarkDirty();
            RebuildTree();

            if (parentFolderId.HasValue)
                SelectTreeNodeByFolderId(parentFolderId.Value);

            RenderFileList();
        }

        private void ImportFolderRecursive(string sourceFolderPath, int? parentFolderId)
        {
            var newFolder = new OIVFolder
            {
                Id = currentProject.NextId++,
                Name = Path.GetFileName(sourceFolderPath.TrimEnd('\\', '/')),
                ParentId = parentFolderId,
                IsRpf = sourceFolderPath.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase)
            };

            currentProject.Folders.Add(newFolder);

            newFolder.AddToDlcList = !newFolder.IsRpf && IsDirectChildOfDlcpacks(newFolder);

            foreach (string filePath in Directory.GetFiles(sourceFolderPath))
            {
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id = currentProject.NextId++,
                    SourcePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    SubPath = string.Empty,
                    Type = "content",
                    FolderId = newFolder.Id
                });
            }

            foreach (string childFolder in Directory.GetDirectories(sourceFolderPath))
            {
                ImportFolderRecursive(childFolder, newFolder.Id);
            }
        }

        private void TreeCmd_Rename()
        {
            var node = editorTree?.SelectedNode;
            if (node == null) return;

            if (node.Tag is OIVFolder folder)
            {
                string name = PromptName("Rename folder:", folder.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    folder.Name = name;
                    MarkDirty();
                    RebuildTree();
                    RenderFileList();
                }
            }
            else if (node.Tag is OIVFileEntry)
            {
                MessageBox.Show("File names are determined by the source file.\nRe-add the file to change it.",
                    "Cannot Rename", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TreeCmd_Delete()
        {
            var node = editorTree?.SelectedNode;
            if (node == null) return;

            if (node.Tag is OIVFolder folder)
            {
                // Collect all descendant folder IDs
                var toRemove = new HashSet<int>();
                CollectDescendants(folder.Id, toRemove);
                toRemove.Add(folder.Id);

                // Unassign / remove files in those folders
                foreach (var file in currentProject.Files.ToList())
                {
                    if (file.FolderId.HasValue && toRemove.Contains(file.FolderId.Value))
                        currentProject.Files.Remove(file);
                }
                currentProject.Folders.RemoveAll(f => toRemove.Contains(f.Id));
            }
            else if (node.Tag is OIVFileEntry file)
            {
                int fid = file.Id;
                currentProject.Files.RemoveAll(f => f.Id == fid);
            }

            MarkDirty();
            RebuildTree();
            RenderFileList();
        }

        private void CollectDescendants(int folderId, HashSet<int> set)
        {
            foreach (var child in currentProject.Folders.Where(f => f.ParentId == folderId))
            {
                set.Add(child.Id);
                CollectDescendants(child.Id, set);
            }
        }

        private void EditorTree_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) TreeCmd_Delete();
            if (e.KeyCode == Keys.F2)     TreeCmd_Rename();
        }

        private void EditorTree_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            // Double-clicking a file node opens its source in Explorer
            if (editorTree?.SelectedNode?.Tag is OIVFileEntry file && File.Exists(file.SourcePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{file.SourcePath}\"");
            }
        }
        private void EditorTree_ItemDrag(object? sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode node && node.Tag is not string)
            {
                DoDragDrop(node, DragDropEffects.Move);
            }
        }

        private void EditorTree_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data == null)
                return;

            if (e.Data.GetDataPresent(typeof(TreeNode)))
                e.Effect = DragDropEffects.Move;
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void EditorTree_DragOver(object? sender, DragEventArgs e)
        {
            if (editorTree == null || e.Data == null)
                return;

            if (e.Data.GetDataPresent(typeof(TreeNode)))
                e.Effect = DragDropEffects.Move;
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;

            Point clientPoint = editorTree.PointToClient(new Point(e.X, e.Y));
            TreeNode? node = editorTree.GetNodeAt(clientPoint);

            if (node != null)
                editorTree.SelectedNode = node;
        }

        private bool IsDescendantFolder(int possibleChildId, int parentId)
        {
            int? currentId = possibleChildId;

            while (currentId.HasValue)
            {
                if (currentId.Value == parentId)
                    return true;

                var folder = currentProject.Folders.FirstOrDefault(f => f.Id == currentId.Value);
                currentId = folder?.ParentId;
            }

            return false;
        }
        private void EditorTree_DragDrop(object? sender, DragEventArgs e)
        {
            if (editorTree == null || e.Data == null)
                return;

            Point clientPoint = editorTree.PointToClient(new Point(e.X, e.Y));
            TreeNode? targetNode = editorTree.GetNodeAt(clientPoint);

            int? targetFolderId = null;

            if (targetNode?.Tag is OIVFolder targetFolder)
                targetFolderId = targetFolder.Id;
            else if (targetNode?.Tag is OIVFileEntry targetFile)
                targetFolderId = targetFile.FolderId;

            // Moving existing folder/file inside the editor
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode))!;

                if (draggedNode == targetNode)
                    return;

                if (draggedNode.Tag is OIVFolder draggedFolder)
                {
                    // Prevent moving a folder inside itself or its own children
                    if (targetFolderId.HasValue && IsDescendantFolder(targetFolderId.Value, draggedFolder.Id))
                        return;

                    draggedFolder.ParentId = targetFolderId;
                    draggedFolder.AddToDlcList = IsDirectChildOfDlcpacks(draggedFolder);
                }
                else if (draggedNode.Tag is OIVFileEntry draggedFile)
                {
                    draggedFile.FolderId = targetFolderId;
                }

                MarkDirty();
                RebuildTree();

                if (draggedNode.Tag is OIVFolder movedFolder)
                    SelectTreeNodeByFolderId(movedFolder.Id);

                RenderFileList();
                return;
            }

            // Importing files/folders from Windows Explorer
            string[]? paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null)
                return;

            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    ImportFolderRecursive(path, targetFolderId);
                }
                else if (File.Exists(path))
                {
                    currentProject.Files.Add(new OIVFileEntry
                    {
                        Id = currentProject.NextId++,
                        SourcePath = path,
                        FileName = Path.GetFileName(path),
                        SubPath = string.Empty,
                        Type = "content",
                        FolderId = targetFolderId
                    });
                }
            }

            MarkDirty();
            RebuildTree();

            if (targetFolderId.HasValue)
                SelectTreeNodeByFolderId(targetFolderId.Value);

            RenderFileList();
        }

        // ─── Prompt helper ────────────────────────────────────────────────────

        private string PromptName(string label, string defaultVal)
        {
            using var dlg = new Form
            {
                Text            = label,
                BackColor       = Color.FromArgb(20, 20, 20),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition   = FormStartPosition.CenterParent,
                ClientSize      = new Size(320, 86),
                MaximizeBox     = false,
                MinimizeBox     = false,
                ControlBox      = false
            };
            dlg.Controls.Add(new Label
            {
                Text = label, ForeColor = Color.FromArgb(160, 110, 110),
                Font = new Font("Syne", 8F, FontStyle.Bold),
                AutoSize = true, Location = new Point(12, 10)
            });
            var txt = new TextBox
            {
                BackColor = Color.Black, ForeColor = Color.FromArgb(210, 210, 210),
                BorderStyle = BorderStyle.FixedSingle, Text = defaultVal,
                Size = new Size(294, 22), Location = new Point(12, 28),
                Font = new Font("Consolas", 9F)
            };
            dlg.Controls.Add(txt);
            var ok = new Button
            {
                Text = "OK", DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(70, 0, 0), ForeColor = Color.FromArgb(220, 160, 160),
                FlatStyle = FlatStyle.Flat, Size = new Size(70, 26), Location = new Point(12, 54)
            };
            ok.FlatAppearance.BorderColor = Color.FromArgb(110, 40, 40);
            dlg.Controls.Add(ok);
            dlg.Controls.Add(new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.FromArgb(140, 100, 100),
                FlatStyle = FlatStyle.Flat, Size = new Size(70, 26), Location = new Point(90, 54)
            });
            dlg.AcceptButton = ok;
            txt.SelectAll();

            return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : string.Empty;
        }

        // ─────────────────── FILE LIST (WebView) ───────────────────

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string? rawMsg = e.TryGetWebMessageAsString();
            if (rawMsg == null) return;

            if (rawMsg.StartsWith("remove:") && int.TryParse(rawMsg.Substring(7), out int removeId))
            {
                currentProject.Files.RemoveAll(f => f.Id == removeId);

                MarkDirty();

                if (editorExpanded) BuildEditorPanel();
                RenderFileList();
            }
            else if (rawMsg.StartsWith("path:"))
            {
                // Path editing from the file list is disabled.
                // The file editor tree is now the source of truth.
                return;
            }
        }

        private void RenderFileList()
        {
            if (!webViewReady) return;

            var sb = new System.Text.StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset='utf-8'>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{background:#0d0d0d;color:#ccc;font-family:'Segoe UI',Arial,sans-serif;font-size:12px;padding:0;height:100vh;display:flex;flex-direction:column}
.toolbar{display:flex;align-items:center;gap:10px;padding:10px 16px;background:#111;border-bottom:1px solid #1e1e1e}
.file-count{color:#666;font-size:11px}
.table-wrap{flex:1;overflow-y:auto}
table{width:100%;border-collapse:collapse}
thead th{background:#141414;color:#9e6060;font-size:10px;letter-spacing:1px;text-transform:uppercase;padding:9px 12px;text-align:left;border-bottom:1px solid #1e1e1e;position:sticky;top:0}
tbody tr{border-bottom:1px solid #161616;transition:background .1s}
tbody tr:hover{background:#141414}
td{padding:7px 12px;vertical-align:middle}
.fn{color:#e0e0e0;font-weight:600;font-size:12px;max-width:180px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.tp input{background:#0a0a0a;border:1px solid #222;color:#bbb;padding:4px 7px;width:100%;font-family:Consolas,monospace;font-size:11px;border-radius:2px}
.tp input[readonly]{background:#0f0f0f;color:#d8d8d8;cursor:default}
.tp input[readonly]:focus{outline:none;border-color:#333;color:#eee}
.rm button{background:#3a0000;border:1px solid #5a1a1a;color:#c08080;padding:3px 9px;cursor:pointer;font-size:10px;border-radius:2px}
.rm button:hover{background:#5a0000;color:#f0c0c0}
.empty{display:flex;flex-direction:column;align-items:center;justify-content:center;height:100%;color:#444;gap:10px;padding:40px}
.empty-icon{font-size:40px}
::-webkit-scrollbar{width:5px}
::-webkit-scrollbar-track{background:#0d0d0d}
::-webkit-scrollbar-thumb{background:#2a2a2a;border-radius:3px}
</style></head><body>");

            if (currentProject.Files.Count == 0)
            {
                sb.Append(@"<div class='empty'>
  <div class='empty-icon'>&#128230;</div>
  <div>No files added yet</div>
  <div style='font-size:11px;color:#333'>Add mod files using the button above or drag &amp; drop</div>
</div>");
            }
            else
            {
                sb.Append($"<div class='toolbar'><span class='file-count'>{currentProject.Files.Count} file(s) added</span></div>");
                sb.Append("<div class='table-wrap'><table><thead><tr><th>File Name</th><th>RESOLVED INSTALL PATH</th><th style='width:70px'>Remove</th></tr></thead><tbody>");

                foreach (var file in currentProject.Files)
                {
                    string name  = System.Net.WebUtility.HtmlEncode(file.FileName);
                    string src   = System.Net.WebUtility.HtmlEncode(file.SourcePath);
                    string resolvedPath = ResolveFilePath(file);
                    string path = System.Net.WebUtility.HtmlEncode(resolvedPath).Replace("'", "&#39;");
                    sb.Append($@"<tr>
<td class='fn' title='{System.Net.WebUtility.HtmlEncode(file.SourcePath)}'>{name}</td>
<td class='tp'><input type='text' value='{path}' readonly title='{path}' /></td>
<td class='rm'><button onclick='sendRemove({file.Id})'>Remove</button></td>
</tr>");
                }

                sb.Append("</tbody></table></div>");
            }

            sb.Append(@"<script>
function sendRemove(id){window.chrome.webview.postMessage('remove:'+id);}
function sendPath(id,val){window.chrome.webview.postMessage('path:'+JSON.stringify({id:String(id),val:val}));}
</script></body></html>");

            webViewFileList.CoreWebView2.NavigateToString(sb.ToString());
        }

        // ─────────────────── FILE ADDING ───────────────────

        private void btnAddFiles_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title      = "Select mod file(s)",
                Multiselect = true,
                Filter     = "All Files (*.*)|*.*|YFT (*.yft)|*.yft|YTD (*.ytd)|*.ytd|META (*.meta)|*.meta|XML (*.xml)|*.xml|ASI (*.asi)|*.asi"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            foreach (string path in dlg.FileNames)
                AddFile(path);
            RenderFileList();
            if (editorExpanded) BuildEditorPanel();
        }

        private void PanelDropZone_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            panelDropZone.BackColor = Color.FromArgb(30, 10, 10);
        }

        private void PanelDropZone_DragDrop(object sender, DragEventArgs e)
        {
            panelDropZone.BackColor = Color.FromArgb(17, 17, 17);
            if (e.Data == null) return;
            string[]? paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null) return;
            foreach (string path in paths)
                if (File.Exists(path)) AddFile(path);
            RenderFileList();
            if (editorExpanded) BuildEditorPanel();
        }

        private void AddFile(string path)
        {
            currentProject.Files.Add(new OIVFileEntry
            {
                Id         = currentProject.NextId++,
                SourcePath = path,
                FileName   = Path.GetFileName(path),
                TargetPath = "",
                Type       = "content"
            });
            MarkDirty();
        }

        // ─────────────────── PHOTO PREVIEW ───────────────────

        private void btnAddPhoto_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title  = "Select preview image",
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                selectedPhotoPath = dlg.FileName;
                btnAddPhoto.Text  = "CHANGE";
                panelPhotoPreview.Invalidate();

                SyncUIToProject();
                MarkDirty();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PanelPhotoPreview_Paint(object sender, PaintEventArgs e)
        {
            if (selectedPhotoPath != null)
            {
                try
                {
                    using var img = Image.FromFile(selectedPhotoPath);
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.DrawImage(img, 0, 0, panelPhotoPreview.Width, panelPhotoPreview.Height);
                    return;
                }
                catch { }
            }
            // placeholder
            using var font  = new Font("Syne", 8F);
            using var brush = new SolidBrush(Color.FromArgb(70, 70, 70));
            var rect = panelPhotoPreview.ClientRectangle;
            e.Graphics.DrawString("No photo", font, brush, rect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        // ─────────────────── COLOR PICKER ───────────────────

        private void panelColorPicker_Click(object sender, EventArgs e)
        {
            using var dlg = new ColorDialog { Color = panelColorPicker.BackColor };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                panelColorPicker.BackColor = dlg.Color;
                SyncUIToProject();
                MarkDirty();
            }
        }

        // ─────────────────── BUILD OIV ───────────────────

        private void btnBuildOIV_Click(object sender, EventArgs e)
        {
            SyncUIToProject();

            if (string.IsNullOrWhiteSpace(currentProject.ModName))
            {
                MessageBox.Show("Please enter a Mod Name before building.", "Missing Metadata",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (currentProject.Files.Count == 0)
            {
                MessageBox.Show("No files added. Add at least one file.", "No Files",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title    = "Save OIV Package",
                Filter   = "OIV Package (*.oiv)|*.oiv",
                FileName = currentProject.ModName.Replace(" ", "_") + ".oiv"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                OIVBuilder.Build(currentProject, dlg.FileName);
                MarkClean();
                MessageBox.Show($"OIV package built successfully!\n\n{dlg.FileName}",
                    "Build Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Build failed: " + ex.Message, "Build Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────── PROJECT SYNC ───────────────────

        private void SyncUIToProject()
        {
            currentProject.ModName     = txtModName.Text.Trim();
            currentProject.Author      = txtAuthor.Text.Trim();
            currentProject.Version     = txtVersion.Text.Trim();
            currentProject.Description = txtDescription.Text.Trim();
            if (dropdownVersionTag.SelectedItem != null)
                currentProject.VersionTag = dropdownVersionTag.SelectedItem.ToString() ?? "Stable";

            // Store banner color as hex string (RRGGBB)
            var c = panelColorPicker.BackColor;
            currentProject.BannerColor = $"{c.R:X2}{c.G:X2}{c.B:X2}";

            if (selectedPhotoPath != null)
                currentProject.PhotoPath = selectedPhotoPath;
        }

        private void LoadProjectIntoUI()
        {
            isLoadingProject = true;

            try
            {
                // Reset all UI first so old values do not leak into the newly opened project
                txtModName.Text = string.Empty;
                txtAuthor.Text = string.Empty;
                txtVersion.Text = string.Empty;
                txtDescription.Text = string.Empty;

                dropdownVersionTag.SelectedIndex = -1;

                panelColorPicker.BackColor = Color.FromArgb(35, 54, 106); // default banner color
                selectedPhotoPath = null;
                btnAddPhoto.Text = "ADD";
                panelPhotoPreview.Invalidate();

                // Optional: clear editor selection state if editor is open
                if (editorTree != null)
                    editorTree.SelectedNode = null;

                // Now load current project values
                txtModName.Text = currentProject.ModName ?? string.Empty;
                txtAuthor.Text = currentProject.Author ?? string.Empty;
                txtVersion.Text = currentProject.Version ?? string.Empty;
                txtDescription.Text = currentProject.Description ?? string.Empty;

                bool tagMatched = false;
                for (int i = 0; i < dropdownVersionTag.Items.Count; i++)
                {
                    if (dropdownVersionTag.Items[i]?.ToString() == currentProject.VersionTag)
                    {
                        dropdownVersionTag.SelectedIndex = i;
                        tagMatched = true;
                        break;
                    }
                }

                // Fallback if no tag matched
                if (!tagMatched && dropdownVersionTag.Items.Count > 0)
                    dropdownVersionTag.SelectedIndex = 0;

                // Restore banner color
                if (!string.IsNullOrWhiteSpace(currentProject.BannerColor))
                {
                    try
                    {
                        string hex = currentProject.BannerColor.TrimStart('#');
                        if (hex.Length == 6)
                        {
                            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                            int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                            panelColorPicker.BackColor = Color.FromArgb(r, g, b);
                        }
                    }
                    catch { }
                }

                // Restore photo
                if (!string.IsNullOrWhiteSpace(currentProject.PhotoPath) && File.Exists(currentProject.PhotoPath))
                {
                    selectedPhotoPath = currentProject.PhotoPath;
                    btnAddPhoto.Text = "CHANGE";
                }

                panelPhotoPreview.Invalidate();

                if (editorExpanded)
                    BuildEditorPanel();

                RenderFileList();
            }
            finally
            {
                isLoadingProject = false;
            }
        }

        // ─────────────────── HOVER HELPERS ───────────────────

        private void AddButtonHover(Button btn)
        {
            var normal = btn.BackColor;
            var bright = ControlPaint.Light(normal, 0.15f);
            btn.MouseEnter += (s, e) => btn.BackColor = bright;
            btn.MouseLeave += (s, e) => btn.BackColor = normal;
        }

        private static string TruncatePath(string path, int maxLen)
        {
            if (path.Length <= maxLen) return path;
            return "..." + path.Substring(path.Length - maxLen + 3);
        }

        // -- CLICK HANDLING ARROWS FILE EDITOR -- 
        private void EditorTree_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (editorTree == null || e.Node == null) return;

            bool hasChildren = e.Node.Nodes.Count > 0;
            if (!hasChildren) return;

            int baseX = 12 + (e.Node.Level * editorTree.Indent);
            Rectangle arrowHitBox = new Rectangle(baseX - 2, e.Node.Bounds.Top, 14, e.Node.Bounds.Height);

            if (arrowHitBox.Contains(e.Location))
            {
                if (e.Node.IsExpanded)
                    e.Node.Collapse();
                else
                    e.Node.Expand();
            }
        }

        // -- CLOSING APPLICATION WITHOUT SAVING ALERT --
        private void MarkDirty()
        {
            isDirty = true;
            UpdateWindowTitle();
        }

        private void MarkClean()
        {
            isDirty = false;
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            string name = string.IsNullOrWhiteSpace(currentProject?.ModName)
                ? "Untitled Project"
                : currentProject.ModName;

            this.Text = isDirty ? $"* {name} - OIV Builder" : $"{name} - OIV Builder";
        }
        // -- FORM CLOSING ALERT METHOD --
        private void Main_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!isDirty)
                return;

            var result = MessageBox.Show(
                "You have unsaved changes.\n\nDo you want to save before exiting?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == DialogResult.Yes)
            {
                bool saved = TrySaveProject();
                if (!saved)
                    e.Cancel = true;
            }
        }

        private bool ConfirmDiscardOrSaveChanges()
        {
            if (!isDirty)
                return true;

            var result = MessageBox.Show(
                "You have unsaved changes.\n\nDo you want to save before continuing?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Cancel)
                return false;

            if (result == DialogResult.Yes)
                return TrySaveProject();

            // No = continue without saving
            return true;
        }
        private bool TrySaveProject()
        {
            try
            {
                SyncUIToProject();

                string savePath = currentProjectPath;

                if (string.IsNullOrWhiteSpace(savePath))
                {
                    using var dlg = new SaveFileDialog
                    {
                        Title = "Save MagicOGK Project",
                        Filter = "MagicOGK Project (*.mogk)|*.mogk|All Files (*.*)|*.*",
                        FileName = string.IsNullOrWhiteSpace(currentProject.ModName) ? "MyMod" : currentProject.ModName
                    };

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return false;

                    savePath = dlg.FileName;
                }

                File.WriteAllText(
                    savePath,
                    System.Text.Json.JsonSerializer.Serialize(
                        currentProject,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

                currentProjectPath = savePath;
                MarkClean();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void Metadata_Changed(object? sender, EventArgs e)
        {
            if (isLoadingProject)
               return;

            SyncUIToProject();
            MarkDirty();
        }

        //window buttons fix scaling
        private void UpdateWindowButtonsLayout()
        {
            int topMargin = 7;
            int rightMargin = 13;
            int spacing = 3;

            button5.Location = new Point(panelDrag.ClientSize.Width - button5.Width - rightMargin, topMargin);
            button6.Location = new Point(button5.Left - button6.Width - spacing, topMargin);
            button7.Location = new Point(button6.Left - button7.Width - spacing, topMargin);
        }

        // -- sidebar logo
        private void SetupLogo()
        {
            PictureBox logo = new PictureBox();
            logo.Image = Properties.Resources.Magic_GTA5;
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.BackColor = Color.Transparent;
            logo.Parent = panelSidebar;
            logo.Size = new Size(370, 170);

            int topZoneHeight = 120; // adjust this

            logo.Location = new Point(
                (panelSidebar.Width - logo.Width) / 2,
                (topZoneHeight - logo.Height) / 2
            );

            panelSidebar.Resize += (s, e) =>
            {
                int topZoneHeight = 120;

                logo.Location = new Point(
                    (panelSidebar.Width - logo.Width) / 2,
                    (topZoneHeight - logo.Height) / 2
                );
            };

            panelSidebar.Controls.Add(logo);
            logo.BringToFront();
        }



    }
    public static class MultiFolderPicker
    {
        public static string[] PickFolders()
        {
            IFileOpenDialog dialog = (IFileOpenDialog)new FileOpenDialogRCW();

            dialog.GetOptions(out uint options);

            dialog.SetOptions(options
                | 0x00000020  // FOS_PICKFOLDERS
                | 0x00000200  // FOS_ALLOWMULTISELECT
                | 0x00001000  // FOS_FORCEFILESYSTEM
            );

            int hr = dialog.Show(IntPtr.Zero);

            if (hr != 0)
                return Array.Empty<string>();

            dialog.GetResults(out IShellItemArray results);
            results.GetCount(out uint count);

            string[] folders = new string[count];

            for (uint i = 0; i < count; i++)
            {
                results.GetItemAt(i, out IShellItem item);
                item.GetDisplayName(0x80058000, out IntPtr pathPtr);

                folders[i] = Marshal.PtrToStringUni(pathPtr)!;
                Marshal.FreeCoTaskMem(pathPtr);
            }

            return folders;
        }

        [ComImport]
        [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        private class FileOpenDialogRCW { }

        [ComImport]
        [Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig] int Show(IntPtr parent);
            void SetFileTypes();
            void SetFileTypeIndex();
            void GetFileTypeIndex();
            void Advise();
            void Unadvise();
            void SetOptions(uint fos);
            void GetOptions(out uint fos);
            void SetDefaultFolder();
            void SetFolder();
            void GetFolder();
            void GetCurrentSelection();
            void SetFileName();
            void GetFileName();
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
            void SetOkButtonLabel();
            void SetFileNameLabel();
            void GetResult();
            void AddPlace();
            void SetDefaultExtension();
            void Close(int hr);
            void SetClientGuid();
            void ClearClientData();
            void SetFilter();
            void GetResults(out IShellItemArray ppenum);
        }

        [ComImport]
        [Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemArray
        {
            void BindToHandler();
            void GetPropertyStore();
            void GetPropertyDescriptionList();
            void GetAttributes();
            void GetCount(out uint pdwNumItems);
            void GetItemAt(uint dwIndex, out IShellItem ppsi);
            void EnumItems();
        }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler();
            void GetParent();
            void GetDisplayName(uint sigdnName, out IntPtr ppszName);
            void GetAttributes();
            void Compare();
        }
    }
}
