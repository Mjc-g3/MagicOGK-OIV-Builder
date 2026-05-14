using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Linq;
using Microsoft.VisualBasic.Logging;
using Microsoft.Web.WebView2.Core;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;

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

        private const string AppCastUrl =
     "https://raw.githubusercontent.com/Mjc-g3/MagicOGK-OIV-Builder/master/appcast.xml";

        private Label? lblWebsite = null;
        private TextBox? txtWebsite = null;

        private TreeView? editorTree = null;
        private Panel? editorPropPanel = null;

        private bool isResizingEditorPanel = false;
        private int editorResizeStartX;
        private int editorResizeStartWidth;
        private int editorCurrentWidth = 380;

        private int editorTarget => editorExpanded ? editorCurrentWidth : 0;

        private Panel? replaceScreenPanel = null;
        private FlowLayoutPanel? replaceVehicleList = null;
        private Panel? replaceVehicleViewport = null;
        private Panel? replaceVehicleScrollTrack = null;
        private Panel? replaceVehicleScrollThumb = null;

        private bool draggingReplaceScrollbar = false;
        private int replaceScrollbarDragOffset = 0;
        private string? selectedReplaceVehicle = null;
        private Label? lblSelectedReplaceVehicle = null;


        private int vehiclePage = 0;
        private const int vehiclesPerPage = 60;
        private List<(string name, string spawn, string img)> allVehicles = new();
        private VScrollBar replaceVehicleScrollBar;

        private string? hoveredVehicle = null;
        private TextBox? txtVehicleSearch = null;
        private Button? btnVehiclePrev = null;
        private Button? btnVehicleNext = null;

        private ContextMenuStrip replaceMenu;
        private ToolStripMenuItem replacePedsItem;
        private ToolStripMenuItem replaceVehiclesItem;
        private ToolStripMenuItem replaceClothesItem;
        private ToolStripMenuItem replaceWeaponsItem;
        private readonly Dictionary<string, Image> weaponImageCache = new();

        private Panel? replaceWeaponViewport = null;
        private FlowLayoutPanel? replaceWeaponList = null;
        private Panel? replaceWeaponScrollTrack = null;
        private Panel? replaceWeaponScrollThumb = null;

        private bool draggingWeaponScrollbar = false;
        private int weaponScrollbarDragOffset = 0;

        private bool allowCloseWithoutPrompt = false;

        private SparkleUpdater? _sparkle;

        private CancellationTokenSource sidebarStaggerCts;

        //colors
        private static readonly Color ThemeBg = Color.FromArgb(13, 13, 13);
        private static readonly Color ThemePanel = Color.FromArgb(18, 18, 18);

        private static readonly Color ThemeRedButton = Color.FromArgb(92, 18, 22);
        private static readonly Color ThemeRedHover = Color.FromArgb(125, 32, 38);

        private static readonly Color ThemeText = Color.FromArgb(210, 150, 150);
        private static readonly Color ThemeTextSoft = Color.FromArgb(170, 120, 120);
        private static readonly Color ThemeTextDim = Color.FromArgb(95, 75, 75);
        private static readonly Color ThemeBorder = Color.FromArgb(90, 45, 45);

        /*
        private int sidebarOpenX = 0;
        private int sidebarClosedX => -panelSidebar.Width;
        private int sidebarTargetX => sidebarExpanded ? sidebarOpenX : sidebarClosedX;
        */
        public main()
        {
            InitializeComponent();

            SetupLeftPanelControls();
            SetupRightPanelControls();
            ApplyCleanLeftPanelLayout();

            StyleSidebarBtn(btnSidebarOpenProject, "    📦    Open Project", 120);
            StyleSidebarBtn(btnSidebarSaveProjectAs, "    📁    Save Project As", 178);
            StyleSidebarBtn(btnSidebarOpenOIV, "    🔎    Open OIV", 236);
            StyleSidebarBtn(btnSidebarExtractOIV, "    📤    Extract OIV", 294);
            StyleSidebarBtn(btnSidebarBuildOIV, "    ⚒️    Build OIV", 352);
            StyleSidebarBtn(btnCheckUpdates, "    ⚒️    Check for updates", 410);
            StyleSidebarBtn(btnSidebarFeedback, "    📨    Feedback", 580);

            btnReplaceMods.Click += btnReplaceMods_Click;

            this.Load += Form1_Load;
            this.FormClosing += Main_FormClosing;
            this.Resize += (s, e) =>
            {
                UpdateWindowButtonsLayout();
                PositionReplaceScreen();

            };
            this.Opacity = 0;
            this.ShowInTaskbar = false;

            this.Size = new Size(1030, 720);
            this.MinimumSize = new Size(1030, 720);

            panelMarquee.MouseDown += DragWindow;
            ApplyTextboxTheme();
            
            dropdownVersionTag.BackColor = Color.FromArgb(28, 28, 28);
            dropdownVersionTag.ForeColor = Color.FromArgb(235, 165, 165);
            
            SetupLogo();

            panelMatrixTitle.Height = 520;
            panelMatrixTitle.SendToBack();

            btnSidebarOpenProject.BringToFront();
            btnSidebarSaveProjectAs.BringToFront();
            btnSidebarOpenOIV.BringToFront();
            btnSidebarExtractOIV.BringToFront();
            btnSidebarBuildOIV.BringToFront();
            btnSidebarFeedback.BringToFront();

            panelSidebar.Dock = DockStyle.None;
            panelSidebar.Width = SidebarWidth;
            panelSidebar.Left = -SidebarWidth;
            panelSidebar.Top = panelMarquee.Height;
            panelSidebar.Height = ClientSize.Height - panelMarquee.Height;

            SetupReplaceMenu();

            this.AutoScaleMode = AutoScaleMode.Dpi;

            float scale = this.DeviceDpi / 96f;

            int baseWidth = 1030;
            int baseHeight = 720;

            this.Size = new Size(
                (int)(baseWidth * scale),
                (int)(baseHeight * scale)
            );

            this.MinimumSize = new Size(
                (int)(baseWidth * scale),
                (int)(baseHeight * scale)
            );
        }

        // ─────────────────── UI UPDATE ETC ───────────────────
        private void ApplyCleanLeftPanelLayout()
        {
            Control host = txtAuthor.Parent;

            int x = 31;
            int w = 300;

            int smallBoxW = 140;
            int smallBoxH = 76;
            int gap = 15;

            Font sectionFont = new Font("Segoe UI", 12, FontStyle.Bold);
            Color sectionColor = Color.FromArgb(220, 150, 150);

            // Remove old custom section labels
            foreach (Control c in host.Controls.OfType<Label>()
                         .Where(l => l.Text == "PROJECT SETUP" || l.Text == "ACTIONS")
                         .ToList())
            {
                host.Controls.Remove(c);
                c.Dispose();
            }

            // PROJECT SETUP title - lined up with PACKAGE FILES
            Label lblProjectSetup = new Label
            {
                Text = "PROJECT SETUP",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = sectionColor,
                Font = new Font("Syne", 11F, FontStyle.Bold),
                Location = new Point(x, 20)
            };

            host.Controls.Add(lblProjectSetup);
            lblProjectSetup.BringToFront();

            // ---------- PROJECT SETUP CONTROLS ----------

            MoveLabelByText("AUTHOR", new Point(34, 42));
            txtAuthor.Location = new Point(x, 62);
            txtAuthor.Size = new Size(w, 22);

            // LINK textbox, made in code
            if (lblWebsite == null)
            {
                lblWebsite = new Label
                {
                    Text = "LINK",
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(220, 150, 150),
                    Font = new Font("Syne", 8F, FontStyle.Bold)
                };

                host.Controls.Add(lblWebsite);
            }

            if (txtWebsite == null)
            {
                txtWebsite = new TextBox
                {
                    BackColor = Color.FromArgb(30, 30, 30),
                    ForeColor = Color.FromArgb(230, 170, 170),
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = txtAuthor.Font,
                    PlaceholderText = "Website / Discord / GTA5-Mods link..."
                };

                txtWebsite.TextChanged += Metadata_Changed;

                host.Controls.Add(txtWebsite);
            }

            lblWebsite.Location = new Point(34, 96);
            txtWebsite.Location = new Point(x, 116);
            txtWebsite.Size = new Size(w, 22);

            MoveLabelByText("MOD NAME", new Point(34, 150));
            txtModName.Location = new Point(x, 170);
            txtModName.Size = new Size(w, 22);

            MoveLabelByText("VERSION TAG", new Point(34, 204));
            MoveLabelByText("VERSION", new Point(188, 204));

            dropdownVersionTag.Location = new Point(x, 224);
            dropdownVersionTag.Size = new Size(140, 24);

            txtVersion.Location = new Point(186, 224);
            txtVersion.Size = new Size(145, 22);

            MoveLabelByText("DESCRIPTION", new Point(34, 256));
            txtDescription.Location = new Point(x, 276);
            txtDescription.Size = new Size(w, 60);

            // ---------- PHOTO / COLOR SECTION MOVED UP ----------

            int baseY = txtDescription.Bottom + 12;

            MoveLabelByText("PHOTO PREVIEW", new Point(36, baseY));
            MoveLabelByText("BANNER COLOR", new Point(188, baseY));

            panelPhotoPreview.Location = new Point(x, baseY + 18);
            panelPhotoPreview.Size = new Size(smallBoxW, smallBoxH);

            panelColorPicker.Location = new Point(x + smallBoxW + gap, baseY + 18);
            panelColorPicker.Size = new Size(145, smallBoxH);

            // ---------- COLOR PICKER HINT LABEL ----------
            foreach (Control c in panelColorPicker.Controls.OfType<Label>().ToList())
            {
                panelColorPicker.Controls.Remove(c);
                c.Dispose();
            }

            Label lblColorHint = new Label
            {
                Text = "Click to change color",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(160, 120, 120),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Cursor = Cursors.Hand
            };

            panelColorPicker.Controls.Add(lblColorHint);
            lblColorHint.BringToFront();

            panelColorPicker.Cursor = Cursors.Hand;


            lblColorHint.Click += panelColorPicker_Click;

            btnAddPhoto.Location = new Point(x, baseY + 98);
            btnAddPhoto.Size = new Size(w, 25);
            btnAddPhoto.Text = "ADD PHOTO";

            // ---------- ACTIONS TITLE ----------

            int actionsTitleY = btnAddPhoto.Bottom + 18;

            Label lblActions = new Label
            {
                Text = "ACTIONS",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = sectionColor,
                Font = new Font("Syne", 11F, FontStyle.Bold),
                Location = new Point(x, actionsTitleY)
            };

            host.Controls.Add(lblActions);
            lblActions.BringToFront();

            // ---------- ACTION BUTTONS ----------

            int buttonH = 39;
            int buttonGap = 8;
            int actionY = lblActions.Bottom + 12;

            btnOpenEditor.Location = new Point(x, actionY);
            btnOpenEditor.Size = new Size(w, buttonH);
            btnOpenEditor.Text = "Open File Editor";

            btnReplaceMods.Location = new Point(x, actionY + buttonH + buttonGap);
            btnReplaceMods.Size = new Size(w, buttonH);
            btnReplaceMods.Text = "Replace Install Menu";

            btnBuildOIV.Location = new Point(x, actionY + (buttonH + buttonGap) * 2);
            btnBuildOIV.Size = new Size(w, buttonH);
            btnBuildOIV.Text = "Build OIV Package";

            // Z-ORDER
            lblProjectSetup.BringToFront();
            lblActions.BringToFront();

            txtAuthor.BringToFront();

            lblWebsite?.BringToFront();
            txtWebsite?.BringToFront();

            txtModName.BringToFront();
            dropdownVersionTag.BringToFront();
            txtVersion.BringToFront();
            txtDescription.BringToFront();

            panelPhotoPreview.BringToFront();
            panelColorPicker.BringToFront();
            lblColorHint.BringToFront();

            btnAddPhoto.BringToFront();

            btnOpenEditor.BringToFront();
            btnReplaceMods.BringToFront();
            btnBuildOIV.BringToFront();
        }
        //colors
        private void ApplyThemeColors()
        {
            BackColor = ThemeBg;

            panelLeft.BackColor = ThemePanel;
            panelRight.BackColor = ThemeBg;
            panelSidebar.BackColor = ThemeBg;

            Label[] labels =
            {
        lblAuthor,
        lblModName,
        lblVersionTag,
        lblVersion,
        lblDescription,
        lblPhotoLabel,
        lblColorLabel,
        lblPackageFiles
    };

            foreach (Label lbl in labels)
                lbl.ForeColor = ThemeText;

            TextBox[] textBoxes =
            {
        txtAuthor,
        txtModName,
        txtVersion
    };

            foreach (TextBox tb in textBoxes)
            {
                tb.BackColor = Color.FromArgb(28, 28, 28);
                tb.ForeColor = ThemeText;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }

            dropdownVersionTag.BackColor = Color.FromArgb(28, 28, 28);
            dropdownVersionTag.ForeColor = ThemeText;

            txtDescription.BackColor = Color.FromArgb(28, 28, 28);
            txtDescription.ForeColor = ThemeText;

            panelDropZone.BackColor = Color.FromArgb(14, 14, 14);
            lblNoFiles.ForeColor = ThemeTextDim;
            lblAddFilesHint.ForeColor = ThemeTextDim;

            ApplyRedButton(btnAddPhoto);
            ApplyRedButton(btnOpenEditor);
            ApplyRedButton(btnReplaceMods);
            ApplyRedButton(btnBuildOIV);
            ApplyRedButton(btnAddFiles);
        }
        private void ApplyRedButton(Button btn)
        {
            btn.BackColor = ThemeRedButton;
            btn.ForeColor = ThemeText;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ThemeRedHover;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 10, 14);
            btn.UseVisualStyleBackColor = false;
        }
        private void panelColorPicker_Click(object sender, EventArgs e)
        {
            using ColorDialog dlg = new ColorDialog();

            dlg.Color = panelColorPicker.BackColor;
            dlg.FullOpen = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                panelColorPicker.BackColor = dlg.Color;

                foreach (Control c in panelColorPicker.Controls)
                {
                    if (c is Label lbl)
                        lbl.Visible = false;
                }

                //currentProject.BannerColor = dlg.Color;

                MarkDirty();
            }
        }

        private void MoveLabelByText(string text, Point location)
        {
            Label? label = FindLabelByText(this, text);

            if (label != null)
            {
                label.Location = location;
                label.BringToFront();
            }
        }

        private Label? FindLabelByText(Control parent, string text)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Label lbl &&
                    string.Equals(lbl.Text, text, StringComparison.OrdinalIgnoreCase))
                {
                    return lbl;
                }

                Label? childResult = FindLabelByText(control, text);
                if (childResult != null)
                    return childResult;
            }

            return null;
        }

        // ─────────────────── CUSTOM DROPDOWN ───────────────────
        public class CustomDropdown : Control
        {
            public List<object> Items { get; } = new();
            public int SelectedIndex { get; set; } = -1;
            public object? SelectedItem => SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;

            public event EventHandler? SelectedIndexChanged;

            public CustomDropdown()
            {
                DoubleBuffered = true;
                Cursor = Cursors.Hand;
                Height = 24;
                BackColor = Color.FromArgb(35, 35, 35);
                ForeColor = Color.FromArgb(220, 180, 180);
            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);

                ContextMenuStrip menu = new ContextMenuStrip();
                menu.BackColor = Color.FromArgb(25, 25, 25);
                menu.ForeColor = ForeColor;
                menu.RenderMode = ToolStripRenderMode.System;

                for (int i = 0; i < Items.Count; i++)
                {
                    int index = i;
                    var item = new ToolStripMenuItem(Items[i].ToString());
                    item.BackColor = Color.FromArgb(25, 25, 25);
                    item.ForeColor = ForeColor;
                    item.Click += (s, ev) =>
                    {
                        SelectedIndex = index;
                        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                        Invalidate();
                    };
                    menu.Items.Add(item);
                }

                menu.Show(this, new Point(0, Height));
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

                using var bg = new SolidBrush(BackColor);
                e.Graphics.FillRectangle(bg, rect);

                using var border = new Pen(Color.FromArgb(95, 95, 95));
                e.Graphics.DrawRectangle(border, rect);

                string text = SelectedItem?.ToString() ?? "";

                TextRenderer.DrawText(
                    e.Graphics,
                    text,
                    Font,
                    new Rectangle(6, 0, Width - 28, Height),
                    ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left
                );

                Point[] arrow =
                {
            new Point(Width - 17, Height / 2 - 2),
            new Point(Width - 9, Height / 2 - 2),
            new Point(Width - 13, Height / 2 + 3)
        };

                using var arrowBrush = new SolidBrush(Color.FromArgb(230, 170, 170));
                e.Graphics.FillPolygon(arrowBrush, arrow);
            }
        }

        // ────────────────── CUSTOM SCROLLBAR+TEXTBOX ───────────────────
        public class CustomScrollTextBox : UserControl
        {
            private readonly TextBox innerTextBox;
            private readonly Panel scrollTrack;
            private readonly Panel scrollThumb;

            public override string Text
            {
                get => innerTextBox.Text;
                set => innerTextBox.Text = value;
            }

            public string PlaceholderText
            {
                get => innerTextBox.PlaceholderText;
                set => innerTextBox.PlaceholderText = value;
            }

            public CustomScrollTextBox()
            {
                BackColor = Color.FromArgb(35, 35, 35);

                innerTextBox = new TextBox
                {
                    Multiline = true,
                    BorderStyle = BorderStyle.None,
                    ScrollBars = ScrollBars.None,
                    BackColor = Color.FromArgb(35, 35, 35),
                    ForeColor = Color.FromArgb(220, 180, 180),
                    Location = new Point(6, 6),
                    Width = Width - 20,
                    Height = Height - 12
                };

                scrollTrack = new Panel
                {
                    Width = 8,
                    Dock = DockStyle.Right,
                    BackColor = Color.FromArgb(45, 45, 45)
                };

                scrollThumb = new Panel
                {
                    Width = 6,
                    Height = 28,
                    Left = 1,
                    Top = 4,
                    BackColor = Color.FromArgb(180, 120, 120)
                };

                scrollTrack.Controls.Add(scrollThumb);
                Controls.Add(innerTextBox);
                Controls.Add(scrollTrack);

                innerTextBox.TextChanged += (s, e) =>
                {
                    UpdateThumb();
                    OnTextChanged(e);
                };

                innerTextBox.MouseWheel += ScrollText;
                MouseWheel += ScrollText;

                Resize += (s, e) =>
                {
                    innerTextBox.Width = Width - 20;
                    innerTextBox.Height = Height - 12;
                    UpdateThumb();
                };
            }

            private void ScrollText(object? sender, MouseEventArgs e)
            {
                int lines = e.Delta > 0 ? -3 : 3;

                int index = innerTextBox.GetFirstCharIndexFromLine(
                    Math.Max(0, innerTextBox.GetLineFromCharIndex(innerTextBox.GetFirstCharIndexOfCurrentLine()) + lines)
                );

                if (index >= 0)
                {
                    innerTextBox.SelectionStart = index;
                    innerTextBox.ScrollToCaret();
                }

                UpdateThumb();
            }

            private void UpdateThumb()
            {
                int lineCount = Math.Max(1, innerTextBox.Lines.Length);
                int visibleLines = Math.Max(1, innerTextBox.Height / innerTextBox.Font.Height);

                if (lineCount <= visibleLines)
                {
                    scrollThumb.Visible = false;
                    return;
                }

                scrollThumb.Visible = true;

                float ratio = visibleLines / (float)lineCount;
                scrollThumb.Height = Math.Max(24, (int)(scrollTrack.Height * ratio));

                int currentLine = innerTextBox.GetLineFromCharIndex(innerTextBox.SelectionStart);
                float scrollRatio = currentLine / (float)Math.Max(1, lineCount - visibleLines);

                scrollThumb.Top = 4 + (int)((scrollTrack.Height - scrollThumb.Height - 8) * scrollRatio);
            }
        }

        // ─────────────────── REPLACE MENU ───────────────────
        private void SetupReplaceMenu()
        {
            replaceMenu = new ContextMenuStrip();

            replaceVehiclesItem = new ToolStripMenuItem("Replace Vehicles");
            replaceClothesItem = new ToolStripMenuItem("Replace Clothes");
            replaceWeaponsItem = new ToolStripMenuItem("Replace Weapons");
            replacePedsItem = new ToolStripMenuItem("Replace Peds");

            replaceVehiclesItem.Click += (s, e) => OpenReplaceVehiclesMenu();
            replaceClothesItem.Click += (s, e) => OpenReplaceClothesMenu();
            replaceWeaponsItem.Click += (s, e) => OpenReplaceWeaponsMenu();
            replacePedsItem.Click += (s, e) => OpenReplacePedsMenu();

            replaceMenu.Items.Add(replaceVehiclesItem);
            replaceMenu.Items.Add(replaceClothesItem);
            replaceMenu.Items.Add(replaceWeaponsItem);
            replaceMenu.Items.Add(replacePedsItem);
        }

        private void PositionReplaceScreen()
        {
            if (replaceScreenPanel == null)
                return;

            int topBarHeight = panelMarquee.Height;

            replaceScreenPanel.Location = new Point(0, topBarHeight);
            replaceScreenPanel.Size = new Size(
                ClientSize.Width,
                ClientSize.Height - topBarHeight
            );

            replaceScreenPanel.BringToFront();
            btnHamburger.BringToFront();
        }
        private void btnReplaceMods_Click(object sender, EventArgs e)
        {
            ReplaceMenuForm menu = new ReplaceMenuForm();

            // CENTER OVER MAIN FORM
            menu.Location = new Point(
                this.Left + (this.Width - menu.Width) / 2,
                this.Top + (this.Height - menu.Height) / 2
            );

            menu.OnVehicles = () => OpenReplaceVehiclesMenu();
            menu.OnClothes = () => OpenReplaceClothesMenu();
            menu.OnWeapons = () => OpenReplaceWeaponsMenu();
            menu.OnPeds = () => OpenReplacePedsMenu();

            menu.Show(this);
        }

        private void OpenReplaceVehiclesMenu()
        {
            if (replaceScreenPanel == null)
            {
                replaceScreenPanel = new Panel
                {
                    BackColor = Color.FromArgb(16, 16, 16),
                    Location = new Point(0, panelMarquee.Height),
                    Size = new Size(
                        this.ClientSize.Width,
                        this.ClientSize.Height - panelMarquee.Height
                    ),
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };

                Controls.Add(replaceScreenPanel);
            }

            replaceScreenPanel.Controls.Clear();
            replaceScreenPanel.Visible = true;
            replaceScreenPanel.BringToFront();
            PositionReplaceScreen();

            var title = new Label
            {
                Text = "REPLACE MENU",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 11F, FontStyle.Bold),
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
            back.Click += (s, e) =>
            {
                replaceScreenPanel.Visible = false;
                panelMarquee.BringToFront();
                btnHamburger.BringToFront();
            };

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

            replaceVehicleViewport = new Panel
            {
                Location = new Point(24, 165),
                Size = new Size(replaceScreenPanel.Width - 58, replaceScreenPanel.Height - 190),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(16, 16, 16)
            };

            replaceVehicleList = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Size = new Size(replaceVehicleViewport.Width, replaceVehicleViewport.Height),
                AutoSize = false,
                AutoScroll = false,
                WrapContents = true,
                Padding = new Padding(34, 0, 0, 0),
                BackColor = Color.FromArgb(16, 16, 16)
            };

            replaceVehicleViewport.Controls.Add(replaceVehicleList);

            // CUSTOM SCROLLBAR TRACK
            replaceVehicleScrollTrack = new Panel
            {
                Location = new Point(replaceVehicleViewport.Right + 8, replaceVehicleViewport.Top),
                Size = new Size(10, replaceVehicleViewport.Height),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(24, 24, 24),
                Cursor = Cursors.Hand
            };

            // CUSTOM SCROLLBAR THUMB
            replaceVehicleScrollThumb = new Panel
            {
                Location = new Point(1, 0),
                Size = new Size(8, 60),
                BackColor = Color.FromArgb(180, 70, 70),
                Cursor = Cursors.Hand
            };

            replaceVehicleScrollTrack.Controls.Add(replaceVehicleScrollThumb);

            replaceVehicleScrollThumb.MouseDown += ReplaceVehicleScrollThumb_MouseDown;
            replaceVehicleScrollThumb.MouseMove += ReplaceVehicleScrollThumb_MouseMove;
            replaceVehicleScrollThumb.MouseUp += ReplaceVehicleScrollThumb_MouseUp;

            replaceVehicleScrollTrack.MouseDown += ReplaceVehicleScrollTrack_MouseDown;

            replaceVehicleViewport.MouseWheel += ReplaceVehicleViewport_MouseWheel;
            replaceVehicleList.MouseWheel += ReplaceVehicleViewport_MouseWheel;

            replaceScreenPanel.Controls.Add(title);
            replaceScreenPanel.Controls.Add(back);
            replaceScreenPanel.Controls.Add(lblSelectedReplaceVehicle);
            replaceScreenPanel.Controls.Add(replaceBtn);
            replaceScreenPanel.Controls.Add(replaceVehicleViewport);
            replaceScreenPanel.Controls.Add(replaceVehicleScrollTrack);

            LoadReplaceVehicleCards();

            panelSidebar.BringToFront();
            panelEditorRight.BringToFront();

            replaceScreenPanel.Controls.Add(next);
            replaceScreenPanel.Controls.Add(prev);
            UpdateNavButtons();

        }
        private void UpdateCustomReplaceScrollbar()
        {
            if (replaceVehicleViewport == null ||
                replaceVehicleList == null ||
                replaceVehicleScrollTrack == null ||
                replaceVehicleScrollThumb == null)
                return;

            replaceVehicleList.Width = replaceVehicleViewport.ClientSize.Width;

            int contentHeight = replaceVehicleList.Controls
                .Cast<Control>()
                .Select(c => c.Bottom + c.Margin.Bottom)
                .DefaultIfEmpty(0)
                .Max();

            int visibleHeight = replaceVehicleViewport.ClientSize.Height;

            replaceVehicleList.Height = Math.Max(contentHeight, visibleHeight);

            if (contentHeight <= visibleHeight)
            {
                replaceVehicleScrollTrack.Visible = false;
                replaceVehicleList.Top = 0;
                return;
            }

            replaceVehicleScrollTrack.Visible = true;

            int thumbHeight = Math.Max(
                35,
                (int)((float)visibleHeight / contentHeight * replaceVehicleScrollTrack.Height)
            );

            replaceVehicleScrollThumb.Height = thumbHeight;

            SetReplaceVehicleScroll(-replaceVehicleList.Top);
        }

        private void SetReplaceVehicleScroll(int scrollY)
        {
            if (replaceVehicleViewport == null ||
                replaceVehicleList == null ||
                replaceVehicleScrollTrack == null ||
                replaceVehicleScrollThumb == null)
                return;

            int maxScroll = Math.Max(0, replaceVehicleList.Height - replaceVehicleViewport.ClientSize.Height);

            scrollY = Math.Max(0, Math.Min(scrollY, maxScroll));

            replaceVehicleList.Top = -scrollY;

            if (maxScroll <= 0)
            {
                replaceVehicleScrollThumb.Top = 0;
                return;
            }

            int maxThumbTop = replaceVehicleScrollTrack.Height - replaceVehicleScrollThumb.Height;

            replaceVehicleScrollThumb.Top = (int)((float)scrollY / maxScroll * maxThumbTop);
        }

        private void ReplaceVehicleViewport_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (replaceVehicleList == null)
                return;

            int currentScroll = -replaceVehicleList.Top;
            int scrollAmount = e.Delta > 0 ? -45 : 45;

            SetReplaceVehicleScroll(currentScroll + scrollAmount);
        }

        private void ReplaceVehicleScrollThumb_MouseDown(object? sender, MouseEventArgs e)
        {
            draggingReplaceScrollbar = true;
            replaceScrollbarDragOffset = e.Y;
        }

        private void ReplaceVehicleScrollThumb_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!draggingReplaceScrollbar ||
                replaceVehicleViewport == null ||
                replaceVehicleList == null ||
                replaceVehicleScrollTrack == null ||
                replaceVehicleScrollThumb == null)
                return;

            int newThumbTop = replaceVehicleScrollThumb.Top + e.Y - replaceScrollbarDragOffset;

            int maxThumbTop = replaceVehicleScrollTrack.Height - replaceVehicleScrollThumb.Height;
            newThumbTop = Math.Max(0, Math.Min(newThumbTop, maxThumbTop));

            replaceVehicleScrollThumb.Top = newThumbTop;

            int maxScroll = Math.Max(0, replaceVehicleList.Height - replaceVehicleViewport.ClientSize.Height);

            int newScrollY = (int)((float)newThumbTop / maxThumbTop * maxScroll);

            SetReplaceVehicleScroll(newScrollY);
        }

        private void ReplaceVehicleScrollThumb_MouseUp(object? sender, MouseEventArgs e)
        {
            draggingReplaceScrollbar = false;
        }

        private void ReplaceVehicleScrollTrack_MouseDown(object? sender, MouseEventArgs e)
        {
            if (replaceVehicleScrollThumb == null || replaceVehicleList == null)
                return;

            int currentScroll = -replaceVehicleList.Top;

            if (e.Y < replaceVehicleScrollThumb.Top)
                SetReplaceVehicleScroll(currentScroll - 120);
            else if (e.Y > replaceVehicleScrollThumb.Bottom)
                SetReplaceVehicleScroll(currentScroll + 120);
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
            UpdateCustomReplaceScrollbar();
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
            int cardW = 160;
            int cardH = 150;

            int imgW = 140;
            int imgH = 80;

            var card = new Panel
            {
                Width = cardW,
                Height = cardH,
                Margin = new Padding(8),
                BackColor = Color.FromArgb(16, 16, 16),
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
                Size = new Size(imgW, imgH),

                // perfectly centered
                Location = new Point((cardW - imgW) / 2, 10),

                BackColor = Color.FromArgb(16, 16, 16),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None
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
                Size = new Size(cardW - 10, 22),
                BackColor = Color.Transparent
            };

            var spawn = new Label
            {
                Text = spawnName,
                ForeColor = Color.FromArgb(120, 85, 85),
                Font = new Font("Consolas", 8F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 120),
                Size = new Size(cardW - 10, 18),
                BackColor = Color.Transparent
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
                "update/x64/dlcpacks/patchday8ng/dlc.rpf/x64/levels/gta5/vehicles.rpf"
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



        private string? selectedClothesCharacter = null;
        private Label? lblSelectedClothesCharacter = null;

        private string? selectedReplaceWeapon = null;
        private Label? lblSelectedReplaceWeapon = null;

        private string? selectedPedReplaceType = null;
        private Label? lblSelectedPedReplaceType = null;
        private ComboBox? cmbPedBasePath = null;
        private TextBox? txtPedModelName = null;

        // REPLACE MENU - CLOTHES

        private void OpenReplaceClothesMenu()
        {
            if (replaceScreenPanel == null)
            {
                replaceScreenPanel = new Panel
                {
                    BackColor = Color.FromArgb(16, 16, 16),
                    Location = new Point(0, panelMarquee.Height),
                    Size = new Size(
                        this.ClientSize.Width,
                        this.ClientSize.Height - panelMarquee.Height
                    ),
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };

                Controls.Add(replaceScreenPanel);
            }

            replaceScreenPanel.Controls.Clear();
            replaceScreenPanel.Visible = true;
            replaceScreenPanel.BringToFront();
            PositionReplaceScreen();

            selectedClothesCharacter = null;

            Label title = new Label
            {
                Text = "REPLACE CLOTHES",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 11F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 22)
            };

            Button back = CreateSecondaryButton("← Back");
            back.Size = new Size(100, 32);
            back.Location = new Point(24, 62);
            back.Click += (s, e) =>
            {
                replaceScreenPanel.Visible = false;
                panelMarquee.BringToFront();
                btnHamburger.BringToFront();
            };

            lblSelectedClothesCharacter = new Label
            {
                Text = "Selected character: none",
                ForeColor = Color.FromArgb(150, 100, 100),
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(140, 68)
            };

            Button replaceBtn = new Button
            {
                Text = "Replace Selected Clothes",
                Size = new Size(220, 36),
                Location = new Point(24, 110),
                BackColor = Color.FromArgb(90, 0, 0),
                ForeColor = Color.FromArgb(240, 180, 180),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 8F, FontStyle.Bold)
            };
            replaceBtn.FlatAppearance.BorderColor = Color.FromArgb(140, 40, 40);
            replaceBtn.Click += (s, e) => ReplaceSelectedClothes();

            Panel characterList = new Panel
            {
                Location = new Point(24, 175),
                Size = new Size(replaceScreenPanel.Width - 48, 230),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(16, 16, 16)
            };

            Panel franklinCard = CreateClothesCharacterCard("Franklin", "franklin", "player_one", "franklin.png");
            Panel michaelCard = CreateClothesCharacterCard("Michael", "michael", "player_zero", "michael.png");
            Panel trevorCard = CreateClothesCharacterCard("Trevor", "trevor", "player_two", "trevor.png");
            Panel addonCard = CreateAddonClothingCard();

            Panel[] cards = { franklinCard, michaelCard, trevorCard, addonCard };

            void CenterClothesCards()
            {
                int cardW = 190;
                int gap = 24;
                int totalW = (cardW * cards.Length) + (gap * (cards.Length - 1));
                int startX = Math.Max(0, (characterList.Width - totalW) / 2);
                int y = 10;

                for (int i = 0; i < cards.Length; i++)
                {
                    cards[i].Location = new Point(startX + i * (cardW + gap), y);
                }
            }

            foreach (Panel card in cards)
                characterList.Controls.Add(card);

            CenterClothesCards();
            characterList.Resize += (s, e) => CenterClothesCards();

            Label info = new Label
            {
                Text = "Choose which story character you want to replace clothing files for.",
                ForeColor = Color.FromArgb(130, 90, 90),
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(58, 425)
            };

            replaceScreenPanel.Controls.Add(title);
            replaceScreenPanel.Controls.Add(back);
            replaceScreenPanel.Controls.Add(lblSelectedClothesCharacter);
            replaceScreenPanel.Controls.Add(replaceBtn);
            replaceScreenPanel.Controls.Add(characterList);
            replaceScreenPanel.Controls.Add(info);

            replaceScreenPanel.BringToFront();
            panelMarquee.BringToFront();
            btnHamburger.BringToFront();
        }

        private Panel CreateAddonClothingCard()
        {
            int cardW = 190;
            int cardH = 210;

            Panel card = new Panel
            {
                Width = cardW,
                Height = cardH,
                Margin = new Padding(12),
                BackColor = Color.FromArgb(16, 16, 16),
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
            {
                bool hover = card.ClientRectangle.Contains(card.PointToClient(Cursor.Position));

                Color borderColor = hover
                    ? Color.FromArgb(220, 50, 50)   // hover = bright red
                    : Color.FromArgb(50, 25, 25);   // normal = subtle like others

                using Pen pen = new Pen(borderColor, hover ? 2 : 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            Label plus = new Label
            {
                Text = "+",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 45),
                Size = new Size(cardW, 55),
                BackColor = Color.Transparent
            };

            Label title = new Label
            {
                Text = "Add-On Clothing",
                ForeColor = Color.FromArgb(220, 170, 170),
                Font = new Font("Syne", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 115),
                Size = new Size(cardW - 10, 30),
                BackColor = Color.Transparent
            };

            Label subtitle = new Label
            {
                Text = "mpclothes / addon",
                ForeColor = Color.FromArgb(120, 85, 85),
                Font = new Font("Consolas", 8F),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 145),
                Size = new Size(cardW - 10, 20),
                BackColor = Color.Transparent
            };

            void ClickCard()
            {
                AddAddonClothing();
            }

            card.Click += (s, e) => ClickCard();
            plus.Click += (s, e) => ClickCard();
            title.Click += (s, e) => ClickCard();
            subtitle.Click += (s, e) => ClickCard();

            card.MouseEnter += (s, e) => card.Invalidate();
            card.MouseLeave += (s, e) => card.Invalidate();

            card.Controls.Add(plus);
            card.Controls.Add(title);
            card.Controls.Add(subtitle);

            return card;
        }

        private Panel CreateClothesCharacterCard(string displayName, string characterId, string fileId, string imageFile)
        {
            int cardW = 190;
            int cardH = 210;

            Panel card = new Panel
            {
                Width = cardW,
                Height = cardH,
                Margin = new Padding(12),
                BackColor = Color.FromArgb(16, 16, 16),
                Cursor = Cursors.Hand,
                Tag = characterId
            };

            PictureBox pic = new PictureBox
            {
                Size = new Size(150, 135),
                Location = new Point((cardW - 150) / 2, 14),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(16, 16, 16)
            };

            string imagePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "characters",
                imageFile
            );

            if (File.Exists(imagePath))
            {
                try
                {
                    pic.Image = Image.FromFile(imagePath);
                }
                catch
                {
                    pic.Image = null;
                }
            }
            else
            {
                pic.Paint += (s, e) =>
                {
                    using Brush brush = new SolidBrush(Color.FromArgb(120, 70, 70));
                    using Font font = new Font("Segoe UI", 9F, FontStyle.Bold);

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

            Label name = new Label
            {
                Text = displayName,
                ForeColor = Color.FromArgb(220, 170, 170),
                Font = new Font("Syne", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 158),
                Size = new Size(cardW - 10, 28),
                BackColor = Color.Transparent
            };

            Label file = new Label
            {
                Text = fileId,
                ForeColor = Color.FromArgb(120, 85, 85),
                Font = new Font("Consolas", 8F),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 184),
                Size = new Size(cardW - 10, 18),
                BackColor = Color.Transparent
            };

            card.Paint += (s, e) =>
            {
                bool selected = selectedClothesCharacter == characterId;
                bool hover = card.ClientRectangle.Contains(card.PointToClient(Cursor.Position));

                Color borderColor =
                    selected ? Color.FromArgb(220, 50, 50) :
                    hover ? Color.FromArgb(180, 70, 70) :
                              Color.FromArgb(50, 25, 25);

                int thickness = selected ? 2 : hover ? 2 : 1;

                using Pen pen = new Pen(borderColor, thickness);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            void SelectCharacter()
            {
                selectedClothesCharacter = characterId;

                if (lblSelectedClothesCharacter != null)
                    lblSelectedClothesCharacter.Text = $"Selected character: {displayName}";

                if (card.Parent != null)
                {
                    foreach (Control c in card.Parent.Controls)
                        c.Invalidate();
                }
            }

            card.Click += (s, e) => SelectCharacter();
            pic.Click += (s, e) => SelectCharacter();
            name.Click += (s, e) => SelectCharacter();
            file.Click += (s, e) => SelectCharacter();

            card.MouseEnter += (s, e) => card.Invalidate();
            card.MouseLeave += (s, e) => card.Invalidate();

            card.Controls.Add(pic);
            card.Controls.Add(name);
            card.Controls.Add(file);

            return card;
        }

        private void ReplaceSelectedClothes()
        {
            if (string.IsNullOrWhiteSpace(selectedClothesCharacter))
            {
                MessageBox.Show(
                    "Select Franklin, Michael, or Trevor first.",
                    "No character selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            using var dlg = new OpenFileDialog
            {
                Title = $"Select clothing replacement files for {selectedClothesCharacter}",
                Multiselect = true,
                Filter = "Clothing files (*.ydd;*.ytd;*.ymt;*.yft)|*.ydd;*.ytd;*.ymt;*.yft|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string targetPath = selectedClothesCharacter switch
            {
                "franklin" => "x64v.rpf/models/cdimages/streamedpeds_players.rpf/player_one",
                "michael" => "x64v.rpf/models/cdimages/streamedpeds_players.rpf/player_zero",
                "trevor" => "x64v.rpf/models/cdimages/streamedpeds_players.rpf/player_two",
                _ => "x64v.rpf/models/cdimages/streamedpeds_players.rpf"
            };

            int folderId = EnsureEditorPath(targetPath);

            foreach (string file in dlg.FileNames)
            {
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id = currentProject.NextId++,
                    SourcePath = file,
                    FileName = Path.GetFileName(file),
                    SubPath = string.Empty,
                    Type = "replace",
                    FolderId = folderId
                });
            }

            MarkDirty();

            if (editorExpanded)
            {
                BuildEditorPanel();
                SelectTreeNodeByFolderId(folderId);
            }

            RenderFileList();

            MessageBox.Show(
                $"Clothing replacement files added for {selectedClothesCharacter}.",
                "Clothes Replace Added",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        private void AddAddonClothing()
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select add-on clothing files/folders",
                Multiselect = true,
                Filter = "Clothing files (*.ydd;*.ytd;*.ymt;*.yft;*.meta)|*.ydd;*.ytd;*.ymt;*.yft;*.meta|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            int folderId = EnsureEditorPath(
                "update/x64/dlcpacks/mpclothes/dlc.rpf/x64/models/cdimages/mpclothes_male.rpf/mp_m_freemode_01_mp_m_clothes_01"
            );

            foreach (string file in dlg.FileNames)
            {
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id = currentProject.NextId++,
                    SourcePath = file,
                    FileName = Path.GetFileName(file),
                    SubPath = string.Empty,
                    Type = "add",
                    FolderId = folderId
                });
            }

            MarkDirty();

            if (editorExpanded)
            {
                BuildEditorPanel();
                SelectTreeNodeByFolderId(folderId);
            }

            RenderFileList();

            MessageBox.Show(
                "Add-on clothing files added to mpclothes path.",
                "Add-On Clothing Added",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }


        // REPLACE MENU - WEAPONS

        private readonly List<(string display, string spawn)> allWeapons = new()
{
    // Melee
    ("Antique Cavalry Dagger", "weapon_dagger"),
    ("Baseball Bat", "weapon_bat"),
    ("Broken Bottle", "weapon_bottle"),
    ("Crowbar", "weapon_crowbar"),
    ("Fist", "weapon_unarmed"),
    ("Flashlight", "weapon_flashlight"),
    ("Golf Club", "weapon_golfclub"),
    ("Hammer", "weapon_hammer"),
    ("Hatchet", "weapon_hatchet"),
    ("Brass Knuckles", "weapon_knuckle"),
    ("Knife", "weapon_knife"),
    ("Machete", "weapon_machete"),
    ("Switchblade", "weapon_switchblade"),
    ("Nightstick", "weapon_nightstick"),
    ("Pipe Wrench", "weapon_wrench"),
    ("Battle Axe", "weapon_battleaxe"),
    ("Pool Cue", "weapon_poolcue"),
    ("Stone Hatchet", "weapon_stone_hatchet"),
    ("Candy Cane", "weapon_candycane"),
    ("The Shocker", "weapon_stunrod"),

    // Handguns
    ("Pistol", "weapon_pistol"),
    ("Pistol Mk II", "weapon_pistol_mk2"),
    ("Combat Pistol", "weapon_combatpistol"),
    ("AP Pistol", "weapon_appistol"),
    ("Stun Gun", "weapon_stungun"),
    ("Pistol .50", "weapon_pistol50"),
    ("SNS Pistol", "weapon_snspistol"),
    ("SNS Pistol Mk II", "weapon_snspistol_mk2"),
    ("Heavy Pistol", "weapon_heavypistol"),
    ("Vintage Pistol", "weapon_vintagepistol"),
    ("Flare Gun", "weapon_flaregun"),
    ("Marksman Pistol", "weapon_marksmanpistol"),
    ("Heavy Revolver", "weapon_revolver"),
    ("Heavy Revolver Mk II", "weapon_revolver_mk2"),
    ("Double Action Revolver", "weapon_doubleaction"),
    ("Up-n-Atomizer", "weapon_raypistol"),
    ("Ceramic Pistol", "weapon_ceramicpistol"),
    ("Navy Revolver", "weapon_navyrevolver"),
    ("Perico Pistol", "weapon_gadgetpistol"),
    ("Stun Gun (MP)", "weapon_stungun_mp"),
    ("WM 29 Pistol", "weapon_pistolxm3"),

    // SMGs
    ("Micro SMG", "weapon_microsmg"),
    ("SMG", "weapon_smg"),
    ("SMG Mk II", "weapon_smg_mk2"),
    ("Assault SMG", "weapon_assaultsmg"),
    ("Combat PDW", "weapon_combatpdw"),
    ("Machine Pistol", "weapon_machinepistol"),
    ("Mini SMG", "weapon_minismg"),
    ("Unholy Hellbringer", "weapon_raycarbine"),
    ("Tactical SMG", "weapon_tecpistol"),

    // Shotguns
    ("Pump Shotgun", "weapon_pumpshotgun"),
    ("Pump Shotgun Mk II", "weapon_pumpshotgun_mk2"),
    ("Sawed-Off Shotgun", "weapon_sawnoffshotgun"),
    ("Assault Shotgun", "weapon_assaultshotgun"),
    ("Bullpup Shotgun", "weapon_bullpupshotgun"),
    ("Heavy Shotgun", "weapon_heavyshotgun"),
    ("Double Barrel Shotgun", "weapon_dbshotgun"),
    ("Sweeper Shotgun", "weapon_autoshotgun"),
    ("Combat Shotgun", "weapon_combatshotgun"),

    // Assault Rifles
    ("Assault Rifle", "weapon_assaultrifle"),
    ("Assault Rifle Mk II", "weapon_assaultrifle_mk2"),
    ("Carbine Rifle", "weapon_carbinerifle"),
    ("Carbine Rifle Mk II", "weapon_carbinerifle_mk2"),
    ("Advanced Rifle", "weapon_advancedrifle"),
    ("Special Carbine", "weapon_specialcarbine"),
    ("Special Carbine Mk II", "weapon_specialcarbine_mk2"),
    ("Bullpup Rifle", "weapon_bullpuprifle"),
    ("Bullpup Rifle Mk II", "weapon_bullpuprifle_mk2"),
    ("Compact Rifle", "weapon_compactrifle"),
    ("Military Rifle", "weapon_militaryrifle"),
    ("Heavy Rifle", "weapon_heavyrifle"),
    ("Tactical Rifle", "weapon_tacticalrifle"),

    // LMGs
    ("MG", "weapon_mg"),
    ("Combat MG", "weapon_combatmg"),
    ("Combat MG Mk II", "weapon_combatmg_mk2"),
    ("Gusenberg Sweeper", "weapon_gusenberg"),

    // Snipers
    ("Sniper Rifle", "weapon_sniperrifle"),
    ("Heavy Sniper", "weapon_heavysniper"),
    ("Heavy Sniper Mk II", "weapon_heavysniper_mk2"),
    ("Marksman Rifle", "weapon_marksmanrifle"),
    ("Marksman Rifle Mk II", "weapon_marksmanrifle_mk2"),
    ("Precision Rifle", "weapon_precisionrifle"),
    ("Musket", "weapon_musket"),

    // Heavy Weapons
    ("RPG", "weapon_rpg"),
    ("Grenade Launcher", "weapon_grenadelauncher"),
    ("Grenade Launcher Smoke", "weapon_grenadelauncher_smoke"),
    ("Minigun", "weapon_minigun"),
    ("Firework Launcher", "weapon_firework"),
    ("Railgun", "weapon_railgun"),
    ("Homing Launcher", "weapon_hominglauncher"),
    ("Compact Grenade Launcher", "weapon_compactlauncher"),
    ("Widowmaker", "weapon_rayminigun"),
    ("EMP Launcher", "weapon_emplauncher"),
    ("Railgun XM3", "weapon_railgunxm3"),

    // Throwables
    ("Grenade", "weapon_grenade"),
    ("BZ Gas", "weapon_bzgas"),
    ("Molotov", "weapon_molotov"),
    ("Sticky Bomb", "weapon_stickybomb"),
    ("Proximity Mine", "weapon_proxmine"),
    ("Snowball", "weapon_snowball"),
    ("Pipe Bomb", "weapon_pipebomb"),
    ("Baseball", "weapon_ball"),
    ("Tear Gas", "weapon_smokegrenade"),
    ("Flare", "weapon_flare"),
    ("Acid Package", "weapon_acidpackage"),

    // Misc
    ("Jerry Can", "weapon_petrolcan"),
    ("Parachute", "gadget_parachute"),
    ("Fire Extinguisher", "weapon_fireextinguisher"),
    ("Hazardous Jerry Can", "weapon_hazardcan"),
    ("Fertilizer Can", "weapon_fertilizercan")
};



        // REPLACE MENU - PEDS
        private void OpenReplacePedsMenu()
        {
            if (replaceScreenPanel == null)
            {
                replaceScreenPanel = new Panel
                {
                    BackColor = Color.FromArgb(16, 16, 16),
                    Location = new Point(0, panelMarquee.Height),
                    Size = new Size(ClientSize.Width, ClientSize.Height - panelMarquee.Height),
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };

                Controls.Add(replaceScreenPanel);
            }

            replaceScreenPanel.Controls.Clear();
            replaceScreenPanel.Visible = true;
            replaceScreenPanel.BringToFront();
            PositionReplaceScreen();

            selectedPedReplaceType = "streamed";

            Label title = new Label
            {
                Text = "REPLACE PEDS",
                ForeColor = Color.FromArgb(255, 115, 125),
                Font = new Font("Syne", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 22)
            };

            Label subtitle = new Label
            {
                Text = "Choose what type of ped you want to replace, then select the target and files.",
                ForeColor = Color.FromArgb(190, 160, 160),
                Font = new Font("Segoe UI", 9.5F),
                AutoSize = true,
                Location = new Point(26, 56)
            };

            Button back = CreateSecondaryButton("← Back");
            back.Size = new Size(100, 34);
            back.Location = new Point(24, 92);
            back.Click += (s, e) =>
            {
                replaceScreenPanel.Visible = false;
                panelMarquee.BringToFront();
                btnHamburger.BringToFront();
            };

            Panel leftPanel = CreatePedGlassPanel(new Point(24, 148), new Size(570, 440));
            Label leftTitle = CreatePedSectionLabel("1. CHOOSE WHAT TO REPLACE", 16, 14);
            leftPanel.Controls.Add(leftTitle);

            Panel rightPanel = CreatePedGlassPanel(new Point(610, 148), new Size(Math.Max(380, replaceScreenPanel.Width - 635), 440));
            rightPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            Label targetTitle = CreatePedSectionLabel("2. SELECT TARGET", 22, 14);
            rightPanel.Controls.Add(targetTitle);

            Label pathLbl = CreatePedSmallLabel("RPF / BASE PATH", 22, 58);
            rightPanel.Controls.Add(pathLbl);

            cmbPedBasePath = new ComboBox
            {
                Location = new Point(22, 82),
                Size = new Size(rightPanel.Width - 44, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                DropDownStyle = ComboBoxStyle.DropDown,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(32, 32, 32),
                ForeColor = Color.FromArgb(235, 220, 220),
                Font = new Font("Segoe UI", 10F)
            };
            cmbPedBasePath.Items.AddRange(new object[]
            {
                "x64e.rpf/models/cdimages/streamedpeds_ig.rpf",
                "x64v.rpf/models/cdimages/streamedpeds_players.rpf",
                "x64g.rpf/levels/gta5/generic/cutsobjects.rpf",
                "update/x64/dlcpacks/patchday3ng/dlc.rpf/x64/models/cdimages/streamedpeds.rpf",
                "update/x64/dlcpacks/patchday8ng/dlc.rpf/x64/models/cdimages/streamedpeds.rpf",
                "update/x64/dlcpacks/mpheist/dlc.rpf/x64/models/cdimages/streamedpeds_mp.rpf"
            });
            cmbPedBasePath.SelectedIndex = 0;
            rightPanel.Controls.Add(cmbPedBasePath);

            Label pathHint = CreatePedHintLabel("Choose the RPF where the original ped is located.", 22, 120);
            rightPanel.Controls.Add(pathHint);

            Label modelLbl = CreatePedSmallLabel("PED FOLDER / MODEL NAME", 22, 158);
            rightPanel.Controls.Add(modelLbl);

            txtPedModelName = new TextBox
            {
                Location = new Point(22, 182),
                Size = new Size(rightPanel.Width - 44, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(32, 32, 32),
                ForeColor = Color.FromArgb(235, 220, 220),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Text = "a_m_m_business_01"
            };
            rightPanel.Controls.Add(txtPedModelName);

            Label modelHint = CreatePedHintLabel("Example: a_m_m_business_01, s_m_y_cop_01, player_one, etc.", 22, 220);
            rightPanel.Controls.Add(modelHint);

            Label fileTitle = CreatePedSectionLabel("3. SELECT FILES", 22, 270);
            rightPanel.Controls.Add(fileTitle);

            Label fileHint = CreatePedHintLabel("Allowed: .ydd, .ytd, .ymt, .yft, .ycd. You can select multiple files at once.", 22, 304);
            rightPanel.Controls.Add(fileHint);

            Button replaceBtn = new Button
            {
                Text = "Replace Selected Ped",
                Size = new Size(250, 42),
                Location = new Point(22, 354),
                BackColor = Color.FromArgb(95, 0, 0),
                ForeColor = Color.FromArgb(245, 190, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            replaceBtn.FlatAppearance.BorderColor = Color.FromArgb(190, 55, 55);
            replaceBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(125, 0, 0);
            replaceBtn.Click += (s, e) => ReplaceSelectedPed();
            rightPanel.Controls.Add(replaceBtn);

            lblSelectedPedReplaceType = new Label
            {
                Text = "Selected: Streamed Ped Model",
                ForeColor = Color.FromArgb(255, 115, 125),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(290, 366)
            };
            rightPanel.Controls.Add(lblSelectedPedReplaceType);

            var pedCards = new[]
            {
                CreatePedTypeCard("Streamed Ped", "streamed", "👤", "Most NPC peds", "a_m_m_business_01", "x64e.rpf/models/cdimages/streamedpeds_ig.rpf"),
                CreatePedTypeCard("Player Ped", "player", "★", "Franklin / Michael / Trevor", "player_one", "x64v.rpf/models/cdimages/streamedpeds_players.rpf"),
                CreatePedTypeCard("Cutscene Ped", "cutscene", "🎬", "Cutscene models", "csb_ramp_marine", "x64g.rpf/levels/gta5/generic/cutsobjects.rpf"),
                CreatePedTypeCard("Patchday Ped", "patchday", "📦", "DLC / patchday peds", "s_m_y_cop_01", "update/x64/dlcpacks/patchday8ng/dlc.rpf/x64/models/cdimages/streamedpeds.rpf"),
                CreatePedTypeCard("Ped Variation", "variation", "☷", "YMT / YCD extras", "a_m_m_business_01", "x64e.rpf/models/cdimages/streamedpeds_ig.rpf")
            };

            void LayoutPedCards()
            {
                int cardW = 165;
                int cardH = 180;
                int gap = 18;
                int startX = 16;
                int y1 = 58;
                int y2 = y1 + cardH + 20;

                for (int i = 0; i < pedCards.Length; i++)
                {
                    int row = i < 3 ? 0 : 1;
                    int col = i < 3 ? i : i - 3;
                    int rowCount = row == 0 ? 3 : 2;
                    int totalW = rowCount * cardW + (rowCount - 1) * gap;
                    int xBase = Math.Max(16, (leftPanel.Width - totalW) / 2);
                    pedCards[i].Location = new Point(xBase + col * (cardW + gap), row == 0 ? y1 : y2);
                }
            }

            foreach (Panel card in pedCards)
                leftPanel.Controls.Add(card);

            LayoutPedCards();
            leftPanel.Resize += (s, e) => LayoutPedCards();

            Panel info = new Panel
            {
                Location = new Point(24, 604),
                Size = new Size(570, 70),
                BackColor = Color.FromArgb(22, 22, 22),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            info.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(65, 45, 45));
                e.Graphics.DrawRectangle(p, 0, 0, info.Width - 1, info.Height - 1);
            };
            info.Controls.Add(new Label
            {
                Text = "ℹ  Tip: for streamed peds, the final path should usually be base RPF + / + ped folder name.",
                ForeColor = Color.FromArgb(190, 160, 160),
                Font = new Font("Segoe UI", 9F),
                AutoSize = false,
                Location = new Point(14, 15),
                Size = new Size(info.Width - 28, 44),
                BackColor = Color.Transparent
            });

            replaceScreenPanel.Controls.Add(title);
            replaceScreenPanel.Controls.Add(subtitle);
            replaceScreenPanel.Controls.Add(back);
            replaceScreenPanel.Controls.Add(leftPanel);
            replaceScreenPanel.Controls.Add(rightPanel);
            replaceScreenPanel.Controls.Add(info);

            replaceScreenPanel.BringToFront();
            panelMarquee.BringToFront();
            btnHamburger.BringToFront();
        }

        private Panel CreatePedGlassPanel(Point location, Size size)
        {
            Panel panel = new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.FromArgb(18, 18, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            panel.Paint += (s, e) =>
            {
                using LinearGradientBrush bg = new LinearGradientBrush(panel.ClientRectangle, Color.FromArgb(24, 24, 24), Color.FromArgb(14, 14, 14), 90f);
                e.Graphics.FillRectangle(bg, panel.ClientRectangle);
                using Pen p = new Pen(Color.FromArgb(65, 45, 45));
                e.Graphics.DrawRectangle(p, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            return panel;
        }

        private Label CreatePedSectionLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.FromArgb(255, 115, 125),
                Font = new Font("Syne", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent
            };
        }

        private Label CreatePedSmallLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.FromArgb(190, 150, 150),
                Font = new Font("Syne", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent
            };
        }

        private Label CreatePedHintLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.FromArgb(135, 115, 115),
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent
            };
        }

        private Panel CreatePedTypeCard(string title, string typeId, string icon, string description, string defaultModel, string defaultPath)
        {
            int cardW = 165;
            int cardH = 180;
            Panel card = new Panel
            {
                Size = new Size(cardW, cardH),
                BackColor = Color.FromArgb(25, 25, 25),
                Cursor = Cursors.Hand,
                Tag = new Tuple<string, string, string, string>(typeId, title, defaultModel, defaultPath)
            };

            Label iconLbl = new Label
            {
                Text = icon,
                ForeColor = Color.FromArgb(235, 235, 235),
                Font = new Font("Segoe UI Emoji", 30F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 20),
                Size = new Size(cardW, 54),
                BackColor = Color.Transparent
            };

            Label nameLbl = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(255, 115, 125),
                Font = new Font("Syne", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(6, 88),
                Size = new Size(cardW - 12, 22),
                BackColor = Color.Transparent
            };

            Label descLbl = new Label
            {
                Text = description,
                ForeColor = Color.FromArgb(185, 165, 165),
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.TopCenter,
                Location = new Point(10, 118),
                Size = new Size(cardW - 20, 42),
                BackColor = Color.Transparent
            };

            Label check = new Label
            {
                Text = "●",
                ForeColor = Color.FromArgb(255, 115, 125),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(cardW - 30, cardH - 32),
                Size = new Size(24, 24),
                BackColor = Color.Transparent,
                Visible = typeId == "streamed"
            };

            card.Paint += (s, e) =>
            {
                bool selected = selectedPedReplaceType == typeId;
                bool hover = card.ClientRectangle.Contains(card.PointToClient(Cursor.Position));
                Color c = selected ? Color.FromArgb(255, 85, 95) : hover ? Color.FromArgb(170, 70, 70) : Color.FromArgb(55, 45, 45);
                using LinearGradientBrush bg = new LinearGradientBrush(card.ClientRectangle, Color.FromArgb(34, 34, 34), Color.FromArgb(18, 18, 18), 90f);
                e.Graphics.FillRectangle(bg, card.ClientRectangle);
                using Pen p = new Pen(c, selected ? 2 : 1);
                e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                check.Visible = selected;
            };

            void SelectCard()
            {
                selectedPedReplaceType = typeId;
                if (lblSelectedPedReplaceType != null)
                    lblSelectedPedReplaceType.Text = $"Selected: {title}";
                if (txtPedModelName != null)
                    txtPedModelName.Text = defaultModel;
                if (cmbPedBasePath != null)
                    cmbPedBasePath.Text = defaultPath;

                if (card.Parent != null)
                {
                    foreach (Control c in card.Parent.Controls)
                        c.Invalidate();
                }
            }

            card.Click += (s, e) => SelectCard();
            iconLbl.Click += (s, e) => SelectCard();
            nameLbl.Click += (s, e) => SelectCard();
            descLbl.Click += (s, e) => SelectCard();
            card.MouseEnter += (s, e) => card.Invalidate();
            card.MouseLeave += (s, e) => card.Invalidate();

            card.Controls.Add(iconLbl);
            card.Controls.Add(nameLbl);
            card.Controls.Add(descLbl);
            card.Controls.Add(check);
            return card;
        }

        private void ReplaceSelectedPed()
        {
            string basePath = cmbPedBasePath?.Text.Trim().Replace('\\', '/') ?? string.Empty;
            string modelName = txtPedModelName?.Text.Trim().Trim('/').Replace('\\', '/') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(basePath) || string.IsNullOrWhiteSpace(modelName))
            {
                MessageBox.Show("Choose a base path and type the ped folder/model name first.", "Missing ped target", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new OpenFileDialog
            {
                Title = $"Select replacement files for {modelName}",
                Multiselect = true,
                Filter = "Ped files (*.ydd;*.ytd;*.ymt;*.yft;*.ycd)|*.ydd;*.ytd;*.ymt;*.yft;*.ycd|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string targetPath = basePath.EndsWith("/" + modelName, StringComparison.OrdinalIgnoreCase)
                ? basePath
                : basePath.TrimEnd('/') + "/" + modelName;

            int folderId = EnsureEditorPath(targetPath);

            foreach (string file in dlg.FileNames)
            {
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id = currentProject.NextId++,
                    SourcePath = file,
                    FileName = Path.GetFileName(file),
                    SubPath = string.Empty,
                    Type = "replace",
                    FolderId = folderId
                });
            }

            MarkDirty();

            if (editorExpanded)
            {
                BuildEditorPanel();
                SelectTreeNodeByFolderId(folderId);
            }

            RenderFileList();

            MessageBox.Show($"Ped replacement files added to:\n\n{targetPath}", "Ped Replace Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenReplaceWeaponsMenu()
        {
            if (replaceScreenPanel == null)
            {
                replaceScreenPanel = new Panel
                {
                    BackColor = Color.FromArgb(16, 16, 16),
                    Location = new Point(0, panelMarquee.Height),
                    Size = new Size(ClientSize.Width, ClientSize.Height - panelMarquee.Height),
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };

                Controls.Add(replaceScreenPanel);
            }

            replaceScreenPanel.Controls.Clear();
            replaceScreenPanel.Visible = true;
            replaceScreenPanel.BringToFront();
            PositionReplaceScreen();

            Label title = new Label
            {
                Text = "REPLACE WEAPONS",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 11F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 22)
            };

            Button back = new Button
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

            lblSelectedReplaceWeapon = new Label
            {
                Text = "Selected weapon: none",
                ForeColor = Color.FromArgb(150, 100, 100),
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(140, 68)
            };

            Button replaceBtn = new Button
            {
                Text = "Replace Selected Weapon",
                Size = new Size(220, 36),
                Location = new Point(24, 110),
                BackColor = Color.FromArgb(90, 0, 0),
                ForeColor = Color.FromArgb(240, 180, 180),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 8F, FontStyle.Bold)
            };
            replaceBtn.FlatAppearance.BorderColor = Color.FromArgb(140, 40, 40);
            replaceBtn.Click += (s, e) => ReplaceSelectedWeapon();

            Label searchLabel = new Label
            {
                Text = "SEARCH WEAPON",
                ForeColor = Color.FromArgb(188, 143, 143),
                Font = new Font("Syne", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(620, 82)
            };

            TextBox txtWeaponSearch = new TextBox
            {
                Size = new Size(300, 28),
                Location = new Point(620, 105),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F)
            };

            replaceWeaponViewport = new Panel
            {
                Location = new Point(58, 165),
                Size = new Size(replaceScreenPanel.Width - 120, replaceScreenPanel.Height - 190),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(16, 16, 16)
            };

            replaceWeaponList = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Size = new Size(replaceWeaponViewport.Width, replaceWeaponViewport.Height),
                AutoSize = false,
                AutoScroll = false,
                WrapContents = true,
                Padding = new Padding(34, 0, 0, 0),
                BackColor = Color.FromArgb(16, 16, 16)
            };

            replaceWeaponViewport.Controls.Add(replaceWeaponList);
            replaceWeaponViewport.MouseEnter += (s, e) => replaceWeaponViewport.Focus();
            replaceWeaponViewport.TabStop = true;

            replaceWeaponScrollTrack = new Panel
            {
                Location = new Point(replaceWeaponViewport.Right + 8, replaceWeaponViewport.Top),
                Size = new Size(10, replaceWeaponViewport.Height),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(24, 24, 24),
                Cursor = Cursors.Hand
            };

            replaceWeaponScrollThumb = new Panel
            {
                Location = new Point(1, 0),
                Size = new Size(8, 60),
                BackColor = Color.FromArgb(180, 70, 70),
                Cursor = Cursors.Hand
            };

            replaceWeaponScrollTrack.Controls.Add(replaceWeaponScrollThumb);

            replaceWeaponScrollThumb.MouseDown += ReplaceWeaponScrollThumb_MouseDown;
            replaceWeaponScrollThumb.MouseMove += ReplaceWeaponScrollThumb_MouseMove;
            replaceWeaponScrollThumb.MouseUp += ReplaceWeaponScrollThumb_MouseUp;

            replaceWeaponScrollTrack.MouseDown += ReplaceWeaponScrollTrack_MouseDown;

            replaceWeaponViewport.MouseWheel += ReplaceWeaponViewport_MouseWheel;
            replaceWeaponList.MouseWheel += ReplaceWeaponViewport_MouseWheel;

            replaceScreenPanel.Controls.Add(replaceWeaponViewport);
            replaceScreenPanel.Controls.Add(replaceWeaponScrollTrack);

            void LoadWeapons()
            {
                if (replaceWeaponList == null)
                    return;

                replaceWeaponList.Controls.Clear();
                replaceWeaponList.Top = 0;

                string search = txtWeaponSearch.Text.Trim();

                var filtered = allWeapons.Where(w =>
                    string.IsNullOrWhiteSpace(search) ||
                    w.display.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    w.spawn.Contains(search, StringComparison.OrdinalIgnoreCase)
                );

                foreach (var weapon in filtered)
                {
                    replaceWeaponList.Controls.Add(CreateWeaponReplaceCard(weapon.display, weapon.spawn));
                }

                UpdateCustomReplaceWeaponScrollbar();
            }

            txtWeaponSearch.TextChanged += (s, e) => LoadWeapons();

            replaceScreenPanel.Controls.Add(title);
            replaceScreenPanel.Controls.Add(back);
            replaceScreenPanel.Controls.Add(lblSelectedReplaceWeapon);
            replaceScreenPanel.Controls.Add(replaceBtn);
            replaceScreenPanel.Controls.Add(searchLabel);
            replaceScreenPanel.Controls.Add(txtWeaponSearch);

            LoadWeapons();

            replaceScreenPanel.BringToFront();
            panelMarquee.BringToFront();
            btnHamburger.BringToFront();
        }

        private void UpdateCustomReplaceWeaponScrollbar()
        {
            if (replaceWeaponViewport == null ||
                replaceWeaponList == null ||
                replaceWeaponScrollTrack == null ||
                replaceWeaponScrollThumb == null)
                return;

            replaceWeaponList.Width = replaceWeaponViewport.ClientSize.Width;

            int contentHeight = replaceWeaponList.Controls
                .Cast<Control>()
                .Select(c => c.Bottom + c.Margin.Bottom)
                .DefaultIfEmpty(0)
                .Max();

            int visibleHeight = replaceWeaponViewport.ClientSize.Height;

            replaceWeaponList.Height = Math.Max(contentHeight, visibleHeight);

            if (contentHeight <= visibleHeight)
            {
                replaceWeaponScrollTrack.Visible = false;
                replaceWeaponList.Top = 0;
                return;
            }

            replaceWeaponScrollTrack.Visible = true;

            int thumbHeight = Math.Max(
                35,
                (int)((float)visibleHeight / contentHeight * replaceWeaponScrollTrack.Height)
            );

            replaceWeaponScrollThumb.Height = thumbHeight;

            SetReplaceWeaponScroll(-replaceWeaponList.Top);
        }

        private void SetReplaceWeaponScroll(int scrollY)
        {
            if (replaceWeaponViewport == null ||
                replaceWeaponList == null ||
                replaceWeaponScrollTrack == null ||
                replaceWeaponScrollThumb == null)
                return;

            int maxScroll = Math.Max(0, replaceWeaponList.Height - replaceWeaponViewport.ClientSize.Height);

            scrollY = Math.Max(0, Math.Min(scrollY, maxScroll));

            replaceWeaponList.Top = -scrollY;

            if (maxScroll <= 0)
            {
                replaceWeaponScrollThumb.Top = 0;
                return;
            }

            int maxThumbTop = replaceWeaponScrollTrack.Height - replaceWeaponScrollThumb.Height;

            replaceWeaponScrollThumb.Top = (int)((float)scrollY / maxScroll * maxThumbTop);
        }

        private void ReplaceWeaponViewport_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (replaceWeaponList == null)
                return;

            int currentScroll = -replaceWeaponList.Top;
            int scrollAmount = e.Delta > 0 ? -45 : 45;

            SetReplaceWeaponScroll(currentScroll + scrollAmount);
        }

        private void ReplaceWeaponScrollThumb_MouseDown(object? sender, MouseEventArgs e)
        {
            draggingWeaponScrollbar = true;
            weaponScrollbarDragOffset = e.Y;
        }

        private void ReplaceWeaponScrollThumb_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!draggingWeaponScrollbar ||
                replaceWeaponViewport == null ||
                replaceWeaponList == null ||
                replaceWeaponScrollTrack == null ||
                replaceWeaponScrollThumb == null)
                return;

            int newThumbTop = replaceWeaponScrollThumb.Top + e.Y - weaponScrollbarDragOffset;

            int maxThumbTop = replaceWeaponScrollTrack.Height - replaceWeaponScrollThumb.Height;
            newThumbTop = Math.Max(0, Math.Min(newThumbTop, maxThumbTop));

            replaceWeaponScrollThumb.Top = newThumbTop;

            int maxScroll = Math.Max(0, replaceWeaponList.Height - replaceWeaponViewport.ClientSize.Height);

            int newScrollY = (int)((float)newThumbTop / maxThumbTop * maxScroll);

            SetReplaceWeaponScroll(newScrollY);
        }

        private void ReplaceWeaponScrollThumb_MouseUp(object? sender, MouseEventArgs e)
        {
            draggingWeaponScrollbar = false;
        }

        private void ReplaceWeaponScrollTrack_MouseDown(object? sender, MouseEventArgs e)
        {
            if (replaceWeaponScrollThumb == null || replaceWeaponList == null)
                return;

            int currentScroll = -replaceWeaponList.Top;

            if (e.Y < replaceWeaponScrollThumb.Top)
                SetReplaceWeaponScroll(currentScroll - 120);
            else if (e.Y > replaceWeaponScrollThumb.Bottom)
                SetReplaceWeaponScroll(currentScroll + 120);
        }

        private Panel CreateWeaponReplaceCard(string displayName, string spawnName)
        {
            int cardW = 160;
            int cardH = 105;

            Panel card = new Panel
            {
                Width = cardW,
                Height = cardH,
                Margin = new Padding(8),
                BackColor = Color.FromArgb(16, 16, 16),
                Cursor = Cursors.Hand,
                Tag = spawnName
            };

            PictureBox pic = new PictureBox
            {
                Size = new Size(96, 56),
                Location = new Point((cardW - 96) / 2, 6),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            string iconPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "weapons",
                spawnName + ".png"
            );

            if (File.Exists(iconPath))
            {
                try
                {
                    if (!weaponImageCache.TryGetValue(iconPath, out Image? img))
                    {
                        using Image original = Image.FromFile(iconPath);

                        Bitmap resized = new Bitmap(96, 56);
                        using (Graphics g = Graphics.FromImage(resized))
                        {
                            g.Clear(Color.Transparent);
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            float ratio = Math.Min(96f / original.Width, 56f / original.Height);

                            int newW = (int)(original.Width * ratio);
                            int newH = (int)(original.Height * ratio);

                            int drawX = (96 - newW) / 2;
                            int drawY = (56 - newH) / 2;

                            g.DrawImage(original, drawX, drawY, newW, newH);
                        }

                        img = resized;
                        weaponImageCache[iconPath] = img;
                    }

                    pic.Image = img;
                }
                catch
                {
                    pic.Image = null;
                }
            }

            Label name = new Label
            {
                Text = displayName,
                ForeColor = Color.FromArgb(220, 170, 170),
                Font = new Font("Syne", 8F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 68),
                Size = new Size(cardW - 10, 22),
                BackColor = Color.Transparent
            };

            Label spawn = new Label
            {
                Text = spawnName,
                ForeColor = Color.FromArgb(120, 85, 85),
                Font = new Font("Consolas", 8F),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 88),
                Size = new Size(cardW - 10, 18),
                BackColor = Color.Transparent
            };

            card.Paint += (s, e) =>
            {
                bool selected = selectedReplaceWeapon == spawnName;

                Color borderColor = selected
                    ? Color.FromArgb(220, 50, 50)
                    : Color.FromArgb(50, 25, 25);

                using Pen pen = new Pen(borderColor, selected ? 2 : 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(25, 25, 25);
            card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(16, 16, 16);

            void SelectWeapon()
            {
                selectedReplaceWeapon = spawnName;

                if (lblSelectedReplaceWeapon != null)
                    lblSelectedReplaceWeapon.Text = $"Selected weapon: {displayName} ({spawnName})";

                if (card.Parent != null)
                {
                    foreach (Control c in card.Parent.Controls)
                        c.Invalidate();
                }
            }

            card.Click += (s, e) => SelectWeapon();
            pic.Click += (s, e) => SelectWeapon();
            name.Click += (s, e) => SelectWeapon();
            spawn.Click += (s, e) => SelectWeapon();

            card.Controls.Add(pic);
            card.Controls.Add(name);
            card.Controls.Add(spawn);

            pic.BringToFront();

            return card;
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (var img in weaponImageCache.Values)
                img.Dispose();

            weaponImageCache.Clear();

            base.OnFormClosed(e);
        }
        private void ReplaceSelectedWeapon()
        {
            if (string.IsNullOrWhiteSpace(selectedReplaceWeapon))
            {
                MessageBox.Show(
                    "Select a weapon first.",
                    "No weapon selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            using var dlg = new OpenFileDialog
            {
                Title = $"Select replacement files for {selectedReplaceWeapon}",
                Multiselect = true,
                Filter = "Weapon files (*.ydr;*.ytd;*.ydd)|*.ydr;*.ytd;*.ydd|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            int weaponsRpfId = EnsureEditorPath(
                "x64e.rpf/models/cdimages/weapons.rpf"
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
                    FolderId = weaponsRpfId
                });
            }

            MarkDirty();

            if (editorExpanded)
            {
                BuildEditorPanel();
                SelectTreeNodeByFolderId(weaponsRpfId);
            }

            RenderFileList();

            MessageBox.Show(
                $"Replacement files added for {selectedReplaceWeapon}.",
                "Weapon Replace Added",
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
            //StyleTextbox(txtDescription);
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
        // Auto -updater
        private void StartUpdateCheck()
        {
            if (_sparkle != null)
                return;

            _sparkle = new SparkleUpdater(
                AppCastUrl,
                new Ed25519Checker(SecurityMode.Unsafe)
            )
            {
                UIFactory = new NetSparkleUpdater.UI.WinForms.UIFactory(this.Icon),
                RelaunchAfterUpdate = true
            };

            _sparkle.StartLoop(true);
        }
        //Check for updates manually
        private async void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            await CheckForUpdatesManualAsync();
        }
        private async Task CheckForUpdatesManualAsync()
        {
            try
            {
                using HttpClient client = new HttpClient();
                string xml = await client.GetStringAsync(AppCastUrl);

                XDocument doc = XDocument.Parse(xml);

                XNamespace sparkle = "http://www.andymatuschak.org/xml-namespaces/sparkle";

                string? latestVersionText = doc
                    .Descendants("item")
                    .FirstOrDefault()?
                    .Element(sparkle + "version")?
                    .Value;

                if (string.IsNullOrWhiteSpace(latestVersionText))
                {
                    ShowMagicInfoBox("Could not read latest version from appcast.xml.", "Update Check");
                    return;
                }

                Version latestVersion = new Version(latestVersionText);
                Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version!;

                if (latestVersion > currentVersion)
                {
                    _sparkle?.CheckForUpdatesAtUserRequest();
                }
                else
                {
                    ShowMagicInfoBox(
                        $"You are already using the latest version.\n\nCurrent version: {currentVersion}",
                        "No Updates Found"
                    );
                }
            }
            catch (Exception ex)
            {
                ShowMagicInfoBox(
                  "Could not check for updates.",
                  "Update Check Failed"
                );
            }
        }
        private DialogResult ShowMagicInfoBox(string message, string title)
        {
            using Form dialog = new Form
            {
                Text = title,
                Size = new Size(420, 210),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(16, 16, 16),
                ShowInTaskbar = false,
                TopMost = true
            };

            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(24, 24, 24)
            };

            Label lblTitle = new Label
            {
                Text = title.ToUpper(),
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 9F, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };

            Button btnX = new Button
            {
                Text = "×",
                Dock = DockStyle.Right,
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(220, 90, 90),
                Font = new Font("Segoe UI", 12F)
            };

            btnX.FlatAppearance.BorderSize = 0;
            btnX.Click += (s, e) => dialog.DialogResult = DialogResult.OK;

            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnX);

            Label icon = new Label
            {
                Text = "✓",
                ForeColor = Color.FromArgb(120, 220, 140),
                Font = new Font("Segoe UI", 30F, FontStyle.Bold),
                Location = new Point(25, 62),
                Size = new Size(55, 55),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblMessage = new Label
            {
                Text = message,
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 9F),
                Location = new Point(92, 62),
                Size = new Size(300, 65)
            };

            Button btnOk = MagicDialogButton("OK", new Point(168, 145));
            btnOk.Click += (s, e) => dialog.DialogResult = DialogResult.OK;

            dialog.Controls.Add(titleBar);
            dialog.Controls.Add(icon);
            dialog.Controls.Add(lblMessage);
            dialog.Controls.Add(btnOk);

            return dialog.ShowDialog(this);
        }
        // ─────────────────── INIT ───────────────────

        private async void Form1_Load(object sender, EventArgs e)
        {
            StartUpdateCheck();

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
            panelDrag.MouseUp += PanelDrag_MouseUp;

            // Drop zone
            panelDropZone.DragEnter += PanelDropZone_DragEnter;
            panelDropZone.DragDrop += PanelDropZone_DragDrop;
            panelDropZone.AllowDrop = true;

            // Button events
            btnAddFiles.Click += btnAddFiles_Click;
            btnAddPhoto.Click += btnAddPhoto_Click;
            btnOpenEditor.Click += btnOpenEditor_Click;
            btnBuildOIV.Click += btnBuildOIV_Click;
            panelColorPicker.Click += panelColorPicker_Click;

            btnSidebarOpenProject.Click += btnSidebarOpenProject_Click;
            btnSidebarSaveProjectAs.Click += btnSidebarSaveProjectAs_Click;
            btnSidebarOpenOIV.Click += btnSidebarOpenOIV_Click;
            btnSidebarExtractOIV.Click += btnSidebarExtractOIV_Click;
            btnSidebarBuildOIV.Click += btnBuildOIV_Click;
            btnCheckUpdates.Click += btnCheckUpdates_Click;
            btnSidebarFeedback.Click += btnSidebarFeedback_Click;

            // Timers
            sidebarTimer.Interval = 12;
            sidebarTimer.Tick += SidebarTimer_Tick;
            editorTimer.Interval = 12;
            editorTimer.Tick += EditorTimer_Tick;

            // Button hover effects
            AddButtonHover(btnOpenEditor);
            AddButtonHover(btnBuildOIV);
            AddButtonHover(btnAddFiles);
            AddButtonHover(btnAddPhoto);
            AddButtonHover(btnReplaceMods);

            // Button animations
            AddPressAnimation(btnSidebarExtractOIV);
            AddPressAnimation(btnBuildOIV);
            AddPressAnimation(btnAddFiles);
            AddPressAnimation(btnOpenEditor);
            AddPressAnimation(btnAddPhoto);
            AddPressAnimation(btnReplaceMods);

            // Button colors
            ((AnimatedGlowButton)btnAddPhoto).TransparentIdle = false;
            ((AnimatedGlowButton)btnOpenEditor).TransparentIdle = false;
            ((AnimatedGlowButton)btnReplaceMods).TransparentIdle = false;
            ((AnimatedGlowButton)btnBuildOIV).TransparentIdle = false;

            btnAddPhoto.BackColor = Color.FromArgb(92, 0, 0);
            btnOpenEditor.BackColor = Color.FromArgb(92, 0, 0);
            btnReplaceMods.BackColor = Color.FromArgb(92, 0, 0);
            btnBuildOIV.BackColor = Color.FromArgb(92, 0, 0);

            //Galaxy Sidebar Fill
            panelMatrixTitle.Dock = DockStyle.Fill;
            panelMatrixTitle.SendToBack();

            // Sidebar buttons transparrency
            if (btnSidebarOpenProject is AnimatedGlowButton b1) b1.TransparentIdle = true;
            if (btnSidebarSaveProjectAs is AnimatedGlowButton b2) b2.TransparentIdle = true;
            if (btnSidebarOpenOIV is AnimatedGlowButton b3) b3.TransparentIdle = true;
            if (btnSidebarExtractOIV is AnimatedGlowButton b4) b4.TransparentIdle = true;
            if (btnCheckUpdates is AnimatedGlowButton b5) b5.TransparentIdle = true;
            if (btnSidebarFeedback is AnimatedGlowButton b6) b6.TransparentIdle = true;

            if (btnAddPhoto is AnimatedGlowButton b7) b7.TransparentIdle = false;
            if (btnOpenEditor is AnimatedGlowButton b8) b8.TransparentIdle = false;
            if (btnReplaceMods is AnimatedGlowButton b9) b9.TransparentIdle = false;
            if (btnBuildOIV is AnimatedGlowButton b10) b10.TransparentIdle = false;

            // Photo preview paint
            panelPhotoPreview.Paint += PanelPhotoPreview_Paint;

            //Sidebar Panel Layout
            panelMatrixTitle.Dock = DockStyle.Fill;
            panelMatrixTitle.SendToBack();

            Control[] sidebarButtons =
            {
    btnSidebarOpenProject,
    btnSidebarSaveProjectAs,
    btnSidebarOpenOIV,
    btnSidebarExtractOIV,
    btnSidebarBuildOIV,
    btnCheckUpdates,
    btnSidebarFeedback
};

            foreach (Control btn in sidebarButtons)
            {
                Point oldLocation = btn.Location;

                btn.Parent = panelMatrixTitle;
                btn.Location = oldLocation;
                btn.BackColor = Color.Transparent;
                btn.BringToFront();

                if (btn is AnimatedGlowButton glowBtn)
                    glowBtn.TransparentIdle = true;
            }

            // WebView
            string webViewDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MagicOGK",
                "WebView2",
               "FileList"
            );

            Directory.CreateDirectory(webViewDataFolder);

            var env = await CoreWebView2Environment.CreateAsync(
                null,
                webViewDataFolder
            );

            await webViewFileList.EnsureCoreWebView2Async(env);
            webViewFileList.CoreWebView2.Settings.IsZoomControlEnabled = false;
            webViewFileList.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webViewFileList.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webViewFileList.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            webViewReady = true;
            RenderFileList();

            txtModName.TextChanged += Metadata_Changed;
            txtAuthor.TextChanged += Metadata_Changed;
            txtVersion.TextChanged += Metadata_Changed;
            txtDescription.TextChanged += Metadata_Changed;
            dropdownVersionTag.SelectedIndexChanged += Metadata_Changed;

            LayoutSidebarButtons();
            UpdateWindowButtonsLayout();
            SetupMarquee();
            ApplyThemeColors();

        }

        //Button animations
        private void AddPressAnimation(Control btn, float scale = 0.92f)
        {
            Size originalSize = btn.Size;
            Point originalLocation = btn.Location;

            void Shrink()
            {
                int newW = (int)(originalSize.Width * scale);
                int newH = (int)(originalSize.Height * scale);

                btn.Size = new Size(newW, newH);
                btn.Location = new Point(
                    originalLocation.X + (originalSize.Width - newW) / 2,
                    originalLocation.Y + (originalSize.Height - newH) / 2
                );
            }

            void Restore()
            {
                btn.Size = originalSize;
                btn.Location = originalLocation;
            }

            btn.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    Shrink();
            };

            btn.MouseUp += (s, e) => Restore();
            btn.MouseLeave += (s, e) => Restore();
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

            _marqueeTimer = new System.Windows.Forms.Timer();
            _marqueeTimer.Interval = 16;
            _marqueeTimer.Tick += (s, e) =>
            {
                if (_marqueeTextWidth <= 0)
                {
                    using var g2 = Graphics.FromHwnd(panelMarquee.Handle);
                    using var mf = new Font("Syne", 9.5F, FontStyle.Bold);
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
            var g = e.Graphics;
            var rect = panelMarquee.ClientRectangle;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            g.Clear(Color.FromArgb(10, 10, 10));

            if (_marqueeTextWidth <= 0) return;

            using var font = new Font("Syne", 9.5F, FontStyle.Bold);
            float y = (rect.Height - font.Height) / 2f - 1;

            // Determine per-pixel alpha by how far the text is into the panel:
            // fade zone width on each edge
            int fadeW = 80;

            // Text left edge and right edge in panel coords
            float textLeft = _marqueeX;
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
        private int[] _matrixY = Array.Empty<int>();
        private int[] _matrixSpeed = Array.Empty<int>();
        private char[] _matrixHeadChar = Array.Empty<char>();
        private int _matrixColW = 12;
        private int _matrixCols = 0;

        private static readonly char[] MatrixChars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789<>[]{}|\\/-_=+*#@!?".ToCharArray();
        private static readonly Random MatrixRnd = new Random();

        private void SetupMatrixRain()
        {
            int w = panelMatrixTitle.Width;
            int h = panelMatrixTitle.Height;
            _matrixCols = w / _matrixColW;
            _matrixY = new int[_matrixCols];
            _matrixSpeed = new int[_matrixCols];
            _matrixHeadChar = new char[_matrixCols];

            for (int i = 0; i < _matrixCols; i++)
            {
                _matrixY[i] = MatrixRnd.Next(-h, 0);
                _matrixSpeed[i] = MatrixRnd.Next(1, 4);
                _matrixHeadChar[i] = MatrixChars[MatrixRnd.Next(MatrixChars.Length)];
            }


            _matrixTimer = new System.Windows.Forms.Timer();
            _matrixTimer.Interval = 75;
            _matrixTimer.Tick += (s, e) =>
            {
                int h2 = panelMatrixTitle.Height;
                for (int i = 0; i < _matrixCols; i++)
                {
                    _matrixY[i] += _matrixSpeed[i] * _matrixColW;
                    if (_matrixY[i] > h2 + _matrixColW * 8)
                    {
                        _matrixY[i] = MatrixRnd.Next(-h2, -_matrixColW);
                        _matrixSpeed[i] = MatrixRnd.Next(1, 4);
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
            var g = e.Graphics;
            int w = panelMatrixTitle.Width;
            int h = panelMatrixTitle.Height;
            int cw = _matrixColW;
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
                    int a = (int)(alpha * alpha * 200); // quadratic falloff
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

        // ─────────────────── BUILD SIDEBAR ───────────────────
        private const int SidebarWidth = 220;

        private int sidebarOpenX => 0;
        private int sidebarClosedX => -SidebarWidth;
        private int sidebarTargetX => sidebarExpanded ? sidebarOpenX : sidebarClosedX;

        private void btnHamburger_Click(object sender, EventArgs e)
        {
            sidebarExpanded = !sidebarExpanded;

            panelSidebar.Dock = DockStyle.None;
            panelSidebar.Width = SidebarWidth;
            panelSidebar.Top = panelMarquee.Height;
            panelSidebar.Height = ClientSize.Height - panelMarquee.Height;

            LayoutSidebarButtons();
            ShowSidebarStaggered();
            sidebarTimer.Start();
        }

        private void SidebarTimer_Tick(object sender, EventArgs e)
        {
            int diff = sidebarTargetX - panelSidebar.Left;

            if (Math.Abs(diff) <= 3)
            {
                panelSidebar.Left = sidebarTargetX;
                sidebarTimer.Stop();
            }
            else
            {
                panelSidebar.Left += diff / 3;
            }

            panelSidebar.BringToFront();
            btnHamburger.BringToFront();
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
                    currentProject = proj;
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
                Title = "Save MagicOGK Project",
                Filter = "MagicOGK Project (*.mogk)|*.mogk|All Files (*.*)|*.*",
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

        private async void btnSidebarOpenOIV_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardOrSaveChanges())
                return;

            using var dlg = new OpenFileDialog
            {
                Title = "Open OIV Package",
                Filter = "OpenIV Package (*.oiv)|*.oiv"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            using LoadingForm loading = new LoadingForm("Opening OIV package...");


            loading.StartPosition = FormStartPosition.Manual;
            loading.Location = new Point(
                this.Left + (this.Width - loading.Width) / 2,
                this.Top + (this.Height - loading.Height) / 2
            );

            try
            {
                loading.Show(this);
                loading.Refresh();

                string selectedPath = dlg.FileName;

                await Task.Run(() =>
                {
                    DisassembleOivToProject(selectedPath);
                });

                LoadProjectIntoUI();
                MarkDirty();
            }
            finally
            {
                loading.Close();
            }
        }
        private async void btnSidebarExtractOIV_Click(object sender, EventArgs e)
        {
            DialogResult modeChoice = ShowMagicExtractChoiceBox();

            if (modeChoice == DialogResult.Cancel)
                return;

            bool useNestedFolders = modeChoice == DialogResult.Yes;

            using var openDlg = new OpenFileDialog
            {
                Title = "Extract OIV Package",
                Filter = "OpenIV Package (*.oiv)|*.oiv"
            };

            if (openDlg.ShowDialog() != DialogResult.OK)
                return;

            using var folderDlg = new FolderBrowserDialog
            {
                Description = "Choose where to extract the OIV"
            };

            if (folderDlg.ShowDialog() != DialogResult.OK)
                return;

            using LoadingForm loading = new LoadingForm("Extracting OIV package...");

            loading.StartPosition = FormStartPosition.Manual;
            loading.Location = new Point(
                this.Left + (this.Width - loading.Width) / 2,
                this.Top + (this.Height - loading.Height) / 2
            );

            try
            {
                loading.Show(this);
                loading.Refresh();

                string oivPath = openDlg.FileName;
                string outputFolder = Path.Combine(
                    folderDlg.SelectedPath,
                    Path.GetFileNameWithoutExtension(oivPath) + "_extracted"
                );

                await Task.Run(() =>
                {
                    ExtractOiv(oivPath, outputFolder, useNestedFolders);
                });

                MessageBox.Show(
                    "OIV extracted successfully.",
                    "Extract Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                Process.Start("explorer.exe", outputFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Extract failed:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                loading.Close();
            }
        }
        private DialogResult ShowMagicExtractChoiceBox()
        {
            using Form dialog = new Form
            {
                Text = "Extract OIV",
                Size = new Size(440, 230),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(16, 16, 16),
                ShowInTaskbar = false,
                TopMost = true
            };

            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(24, 24, 24)
            };

            Label lblTitle = new Label
            {
                Text = "EXTRACT OIV",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 9F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };

            Button btnX = new Button
            {
                Text = "×",
                Dock = DockStyle.Right,
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(220, 90, 90),
                Font = new Font("Segoe UI", 12F)
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.Click += (s, e) => dialog.DialogResult = DialogResult.Cancel;

            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnX);

            Label icon = new Label
            {
                Text = "📦",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Segoe UI Emoji", 28F, FontStyle.Regular),
                Location = new Point(28, 68),
                Size = new Size(55, 55),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblMessage = new Label
            {
                Text = "How do you want to extract this OIV?",
                ForeColor = Color.FromArgb(235, 170, 170),
                Font = new Font("Syne", 9F, FontStyle.Bold),
                Location = new Point(96, 70),
                Size = new Size(310, 24)
            };

            Label lblDetails = new Label
            {
                Text = "Nested folders keeps the real folder structure.\nFlat folders creates one folder per install path.",
                ForeColor = Color.FromArgb(170, 120, 120),
                Font = new Font("Syne", 8F, FontStyle.Regular),
                Location = new Point(96, 100),
                Size = new Size(320, 42)
            };

            Button btnNested = MagicDialogButton("NESTED", new Point(70, 165));
            Button btnFlat = MagicDialogButton("FLAT", new Point(178, 165));
            Button btnCancel = MagicDialogButton("CANCEL", new Point(286, 165));

            btnNested.Click += (s, e) => dialog.DialogResult = DialogResult.Yes;
            btnFlat.Click += (s, e) => dialog.DialogResult = DialogResult.No;
            btnCancel.Click += (s, e) => dialog.DialogResult = DialogResult.Cancel;

            dialog.Controls.Add(titleBar);
            dialog.Controls.Add(icon);
            dialog.Controls.Add(lblMessage);
            dialog.Controls.Add(lblDetails);
            dialog.Controls.Add(btnNested);
            dialog.Controls.Add(btnFlat);
            dialog.Controls.Add(btnCancel);

            return dialog.ShowDialog(this);
        }
        private void ExtractOiv(string oivPath, string outputFolder, bool useNestedFolders)
        {
            string tempFolder = Path.Combine(
                Path.GetTempPath(),
                "MagicOGK_ExtractOIV_" + Guid.NewGuid().ToString("N")
            );

            Directory.CreateDirectory(tempFolder);
            Directory.CreateDirectory(outputFolder);

            ZipFile.ExtractToDirectory(oivPath, tempFolder);

            string? assemblyPath = Directory
                .GetFiles(tempFolder, "assembly.xml", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (assemblyPath == null)
                throw new Exception("assembly.xml was not found inside this OIV.");

            XDocument doc = XDocument.Load(assemblyPath);
            string rootFolder = Path.GetDirectoryName(assemblyPath)!;

            var installNodes = doc.Descendants()
                .Where(x =>
                {
                    string n = x.Name.LocalName.ToLowerInvariant();
                    return n == "file" || n == "add" || n == "replace" || n == "import";
                })
                .ToList();

            List<string> reportLines = new();

            foreach (XElement node in installNodes)
            {
                string source = GetAttr(node, "source", "src", "file");

                if (string.IsNullOrWhiteSpace(source))
                    continue;

                string? extractedFile = ResolveExtractedFile(rootFolder, tempFolder, source);

                if (extractedFile == null || !File.Exists(extractedFile))
                    continue;

                string finalTarget = BuildFinalOivTarget(node, source);

                if (string.IsNullOrWhiteSpace(finalTarget))
                    continue;

                finalTarget = finalTarget.Replace("\\", "/").Trim('/');

                string fileName = Path.GetFileName(finalTarget);
                string? folderPath = Path.GetDirectoryName(finalTarget)?.Replace("\\", "/");

                string localFolderName;

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    localFolderName = "_root";
                }
                else
                {
                    localFolderName = useNestedFolders
                        ? MakeSafeRelativePath(folderPath)
                        : MakeFlatOivFolderName(folderPath);
                }

                string localFolder = Path.Combine(outputFolder, localFolderName);
                Directory.CreateDirectory(localFolder);

                string localFilePath = Path.Combine(localFolder, fileName);
                File.Copy(extractedFile, localFilePath, true);

                reportLines.Add($"{localFolderName}/{fileName}  =>  {finalTarget}");
            }

            File.WriteAllLines(
                Path.Combine(outputFolder, "_original_install_paths.txt"),
                reportLines
            );

            Directory.Delete(tempFolder, true);
        }
        private string MakeSafeRelativePath(string path)
        {
            path = path.Replace("\\", "/").Trim('/');

            string[] parts = path
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part =>
                {
                    string safe = part;

                    foreach (char c in Path.GetInvalidFileNameChars())
                        safe = safe.Replace(c, '-');

                    return safe;
                })
                .ToArray();

            return Path.Combine(parts);
        }
        private string MakeFlatOivFolderName(string path)
        {
            path = path.Replace("\\", "/").Trim('/');

            foreach (char c in Path.GetInvalidFileNameChars())
                path = path.Replace(c, '-');

            return path.Replace("/", "-");
        }
        private void DisassembleOivToProject(string oivPath)
        {
            string tempFolder = Path.Combine(
                Path.GetTempPath(),
                "MagicOGK_OIV_" + Guid.NewGuid().ToString("N")
            );

            Directory.CreateDirectory(tempFolder);
            ZipFile.ExtractToDirectory(oivPath, tempFolder);

            string? assemblyPath = Directory
                .GetFiles(tempFolder, "assembly.xml", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (assemblyPath == null)
                throw new Exception("assembly.xml was not found inside this OIV.");

            currentProject = new OIVProject();

            XDocument doc = XDocument.Load(assemblyPath);
            string rootFolder = Path.GetDirectoryName(assemblyPath)!;

            XElement? metadata = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName.Equals("metadata", StringComparison.OrdinalIgnoreCase));

            if (metadata != null)
            {
                currentProject.ModName = GetMetadataText(metadata, "name");
                currentProject.Description = GetMetadataText(metadata, "description");

                ReadOivAuthorAndWebsite(metadata, out string cleanAuthor, out string website);

                currentProject.Author = cleanAuthor;
                currentProject.Website = website;

                ReadOivVersion(metadata, out string cleanVersion, out string versionTag);
                currentProject.Version = cleanVersion;
                currentProject.VersionTag = versionTag;
            }

            string? iconPath = Directory
                .GetFiles(rootFolder, "icon.png", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (iconPath != null)
            {
                selectedPhotoPath = iconPath;
                currentProject.PhotoPath = iconPath;
            }

            var installNodes = doc.Descendants()
                    .Where(x =>
                    {
                        string n = x.Name.LocalName.ToLowerInvariant();
                        return n == "file" || n == "add" || n == "replace" || n == "import";
                    })
                    .ToList();

            foreach (XElement node in installNodes)
            {
                string source = GetAttr(node, "source", "src", "file");

                if (string.IsNullOrWhiteSpace(source))
                    continue;

                string? extractedFile = ResolveExtractedFile(rootFolder, tempFolder, source);

                if (extractedFile == null || !File.Exists(extractedFile))
                    continue;

                string finalTarget = BuildFinalOivTarget(node, source);

                if (string.IsNullOrWhiteSpace(finalTarget))
                    continue;

                finalTarget = finalTarget
                    .Replace("\\", "/")
                    .Trim('/');

                string fileName = Path.GetFileName(finalTarget);
                string? folderPath = Path.GetDirectoryName(finalTarget)?.Replace("\\", "/");

                int? folderId = null;

                if (!string.IsNullOrWhiteSpace(folderPath))
                    folderId = EnsureEditorPath(folderPath);

                currentProject.Files.Add(new OIVFileEntry
                {
                    Id = currentProject.NextId++,
                    SourcePath = extractedFile,
                    FileName = fileName,
                    SubPath = string.Empty,
                    Type = "content",
                    FolderId = folderId
                });
            }
        }

        private string BuildFinalOivTarget(XElement node, string source)
        {
            string nodeTarget = GetAttr(node, "target", "destination", "dest", "path");

            // IMPORTANT:
            // OpenIV <add> nodes usually store the install path as inner text:
            // <add source="dlc.rpf">update\x64\dlcpacks\modname\dlc.rpf</add>
            if (string.IsNullOrWhiteSpace(nodeTarget))
            {
                string innerText = node.Value?.Trim() ?? "";

                if (!string.IsNullOrWhiteSpace(innerText) && !node.HasElements)
                    nodeTarget = innerText;
            }

            string archivePath = GetFullArchivePath(node);

            source = source.Replace("\\", "/").Trim('/');
            nodeTarget = nodeTarget.Replace("\\", "/").Trim('/');
            archivePath = archivePath.Replace("\\", "/").Trim('/');

            if (!string.IsNullOrWhiteSpace(nodeTarget))
            {
                if (!string.IsNullOrWhiteSpace(archivePath) &&
                    !nodeTarget.StartsWith(archivePath, StringComparison.OrdinalIgnoreCase))
                {
                    return archivePath + "/" + nodeTarget;
                }

                return nodeTarget;
            }

            if (!string.IsNullOrWhiteSpace(archivePath))
            {
                string sourceName = Path.GetFileName(source);

                if (sourceName.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    return archivePath;

                return archivePath + "/" + sourceName;
            }

            return Path.GetFileName(source);
        }

        private string GetChildValue(XElement parent, string childName)
        {
            return parent.Elements()
                .FirstOrDefault(x => x.Name.LocalName.Equals(childName, StringComparison.OrdinalIgnoreCase))
                ?.Value ?? "";
        }

        private string GetMetadataText(XElement parent, string childName)
        {
            XElement? node = parent.Elements()
                .FirstOrDefault(x => x.Name.LocalName.Equals(childName, StringComparison.OrdinalIgnoreCase));

            if (node == null)
                return "";

            // Handles normal metadata:
            // <author>XENORT</author>
            if (!node.HasElements)
                return node.Value.Trim();

            // Handles OpenIV-style metadata:
            // <author><displayName>XENORT</displayName></author>
            XElement? displayName = node.Descendants()
                .FirstOrDefault(x => x.Name.LocalName.Equals("displayName", StringComparison.OrdinalIgnoreCase));

            if (displayName != null)
                return displayName.Value.Trim();

            return node.Value.Trim();
        }
        private void LayoutSidebarButtons()
        {
            int margin = 6;
            int buttonWidth = panelSidebar.Width - (margin * 2);
            int buttonHeight = 48;

            Control[] buttons =
            {
        btnSidebarOpenProject,
        btnSidebarSaveProjectAs,
        btnSidebarOpenOIV,
        btnSidebarExtractOIV,
        btnSidebarBuildOIV,
        btnCheckUpdates,
        btnSidebarFeedback
    };

            foreach (Control btn in buttons)
            {
                btn.Width = buttonWidth;
                btn.Height = buttonHeight;
                btn.Left = margin;
            }
        }

        private void ReadOivAuthorAndWebsite(XElement metadata, out string author, out string website)
        {
            author = "";
            website = "";

            XElement? authorNode = metadata.Elements()
                .FirstOrDefault(x => x.Name.LocalName.Equals("author", StringComparison.OrdinalIgnoreCase));

            if (authorNode == null)
                return;

            XElement? displayName = authorNode.Descendants()
                .FirstOrDefault(x => x.Name.LocalName.Equals("displayName", StringComparison.OrdinalIgnoreCase));

            author = displayName != null
                ? displayName.Value.Trim()
                : authorNode.Value.Trim();

            XElement? linkNode = authorNode.Descendants()
                .FirstOrDefault(x =>
                    x.Name.LocalName.Equals("web", StringComparison.OrdinalIgnoreCase) ||
                    x.Name.LocalName.Equals("website", StringComparison.OrdinalIgnoreCase) ||
                    x.Name.LocalName.Equals("url", StringComparison.OrdinalIgnoreCase) ||
                    x.Name.LocalName.Equals("link", StringComparison.OrdinalIgnoreCase));

            if (linkNode != null)
                website = linkNode.Value.Trim();

            if (string.IsNullOrWhiteSpace(website))
            {
                int httpIndex = authorNode.Value.IndexOf("http", StringComparison.OrdinalIgnoreCase);

                if (httpIndex >= 0)
                {
                    string raw = authorNode.Value.Trim();
                    author = raw.Substring(0, httpIndex).Trim();
                    website = raw.Substring(httpIndex).Trim();
                }
            }
        }

        private void SplitAuthorAndWebsite(string rawAuthor, out string author, out string website)
        {
            author = rawAuthor.Trim();
            website = "";

            int httpIndex = rawAuthor.IndexOf("http", StringComparison.OrdinalIgnoreCase);

            if (httpIndex >= 0)
            {
                author = rawAuthor.Substring(0, httpIndex).Trim();
                website = rawAuthor.Substring(httpIndex).Trim();
            }
        }

        private void ReadOivVersion(XElement metadata, out string version, out string versionTag)
        {
            version = "";
            versionTag = "Stable";

            XElement? versionNode = metadata.Elements()
                .FirstOrDefault(x => x.Name.LocalName.Equals("version", StringComparison.OrdinalIgnoreCase));

            if (versionNode == null)
                return;

            string major = versionNode.Elements()
                .FirstOrDefault(x => x.Name.LocalName.Equals("major", StringComparison.OrdinalIgnoreCase))
                ?.Value.Trim() ?? "";

            string minor = versionNode.Elements()
                .FirstOrDefault(x => x.Name.LocalName.Equals("minor", StringComparison.OrdinalIgnoreCase))
                ?.Value.Trim() ?? "";

            string tag = versionNode.Elements()
                .FirstOrDefault(x => x.Name.LocalName.Equals("tag", StringComparison.OrdinalIgnoreCase))
                ?.Value.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(major) || !string.IsNullOrWhiteSpace(minor))
            {
                version = string.IsNullOrWhiteSpace(minor) ? major : $"{major}.{minor}";
            }
            else
            {
                version = versionNode.Value.Trim();
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                if (tag.Equals("MAIN", StringComparison.OrdinalIgnoreCase))
                    versionTag = "Stable";
                else
                    versionTag = tag;
            }

            version = version
                .Replace("Main", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Stable", "", StringComparison.OrdinalIgnoreCase)
                .Trim();
        }

        private string GetAttr(XElement node, params string[] names)
        {
            foreach (string name in names)
            {
                XAttribute? attr = node.Attributes()
                    .FirstOrDefault(a => a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (attr != null)
                    return attr.Value.Trim();
            }

            return "";
        }

        private string GetFullArchivePath(XElement node)
        {
            List<string> parts = new List<string>();

            foreach (XElement archive in node.Ancestors().Reverse())
            {
                if (!archive.Name.LocalName.Equals("archive", StringComparison.OrdinalIgnoreCase))
                    continue;

                string p = GetAttr(archive, "path", "target", "destination");

                if (!string.IsNullOrWhiteSpace(p))
                    parts.Add(p.Replace("\\", "/").Trim('/'));
            }

            return string.Join("/", parts);
        }

        private string CombineOivPath(string archivePath, string target)
        {
            archivePath = archivePath.Replace("\\", "/").Trim('/');
            target = target.Replace("\\", "/").Trim('/');

            if (string.IsNullOrWhiteSpace(archivePath))
                return target;

            if (string.IsNullOrWhiteSpace(target))
                return archivePath;

            if (target.StartsWith(archivePath, StringComparison.OrdinalIgnoreCase))
                return target;

            return archivePath + "/" + target;
        }

        private string? ResolveExtractedFile(string rootFolder, string tempFolder, string source)
        {
            source = source.Replace("/", "\\").TrimStart('\\');

            string p1 = Path.Combine(rootFolder, source);
            if (File.Exists(p1))
                return p1;

            string p2 = Path.Combine(tempFolder, source);
            if (File.Exists(p2))
                return p2;

            string fileName = Path.GetFileName(source);

            return Directory
                .GetFiles(tempFolder, fileName, SearchOption.AllDirectories)
                .FirstOrDefault();
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

        private Button MagicDialogButton(string text, Point location)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(84, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(90, 0, 0),
                ForeColor = Color.FromArgb(235, 170, 170),
                Font = new Font("Syne", 8F, FontStyle.Bold)
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(150, 35, 35);
            btn.FlatAppearance.BorderSize = 1;

            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(125, 0, 0);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(90, 0, 0);

            return btn;
        }
        // SIDEBAR ANIMATION
        private async void ShowSidebarStaggered()
        {
            sidebarStaggerCts?.Cancel();
            sidebarStaggerCts = new CancellationTokenSource();
            var token = sidebarStaggerCts.Token;

            panelSidebar.Visible = true;
            panelSidebar.BringToFront();

            Control[] buttons =
            {
        btnSidebarOpenProject,
        btnSidebarSaveProjectAs,
        btnSidebarOpenOIV,
        btnSidebarExtractOIV,
        btnSidebarBuildOIV,
        btnCheckUpdates,
        btnSidebarFeedback
    };

            LayoutSidebarButtons();

            int[] finalLeft = buttons.Select(b => b.Left).ToArray();

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Visible = false;
                buttons[i].Left = finalLeft[i] - 25;
            }

            try
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    token.ThrowIfCancellationRequested();

                    await Task.Delay(i == 0 ? 0 : 65, token);

                    Control btn = buttons[i];
                    btn.Visible = true;

                    int startLeft = finalLeft[i] - 25;
                    int endLeft = finalLeft[i];

                    for (int step = 0; step <= 10; step++)
                    {
                        token.ThrowIfCancellationRequested();

                        float t = step / 10f;
                        float eased = 1f - (float)Math.Pow(1f - t, 3);

                        btn.Left = startLeft + (int)((endLeft - startLeft) * eased);

                        await Task.Delay(10, token);
                    }

                    btn.Left = endLeft;
                }
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }

        // ─────────────────── CUSTOM POP-UP BOXES ───────────────────
        private Form CreateBuildProgressDialog(out ProgressBar progressBar, out Label lblPercent)
        {
            Label lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "BUILDING OIV.",
                ForeColor = Color.FromArgb(235, 170, 170),
                Font = new Font("Syne", 11F, FontStyle.Bold),
                Location = new Point(28, 62),
                Size = new Size(380, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Form dialog = new Form
            {
                Text = "Building OIV",
                Size = new Size(440, 190),
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(16, 16, 16),
                ShowInTaskbar = false,
                TopMost = true
            };

            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(24, 24, 24)
            };

            Label lblTitle = new Label
            {
                Text = "BUILDING OIV",
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 9F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };

            titleBar.Controls.Add(lblTitle);

            lblPercent = new Label
            {
                Text = "0%",
                ForeColor = Color.FromArgb(235, 170, 170),
                Font = new Font("Syne", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(28, 126),
                Size = new Size(380, 24)
            };

            progressBar = new ProgressBar
            {
                Location = new Point(28, 100),
                Size = new Size(380, 22),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            dialog.Controls.Add(titleBar);
            dialog.Controls.Add(lblStatus);
            dialog.Controls.Add(lblPercent);
            dialog.Controls.Add(progressBar);

            dialog.Load += (s, e) =>
            {
                if (this.WindowState == FormWindowState.Minimized)
                    return;

                dialog.Location = new Point(
                    this.Left + (this.Width - dialog.Width) / 2,
                    this.Top + (this.Height - dialog.Height) / 2
                );
            };

            Label? statusLabel = dialog.Controls["lblStatus"] as Label;

            System.Windows.Forms.Timer dotTimer = new System.Windows.Forms.Timer();
            dotTimer.Interval = 350;

            int dots = 1;

            dotTimer.Tick += (s, e) =>
            {
                if (statusLabel == null) return;

                statusLabel.Text = "BUILDING OIV PACKAGE" + new string('.', dots);

                dots++;

                if (dots > 4)
                    dots = 1;
            };

            dialog.Shown += (s, e) => dotTimer.Start();

            dialog.FormClosed += (s, e) =>
            {
                dotTimer.Stop();
                dotTimer.Dispose();
            };

            return dialog;
        }

        private DialogResult ShowMagicConfirmBox(string message, string title)
        {
            using Form dialog = new Form
            {
                Text = title,
                Size = new Size(420, 210),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(16, 16, 16),
                ShowInTaskbar = false,
                TopMost = true
            };

            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(24, 24, 24)
            };

            Label lblTitle = new Label
            {
                Text = title.ToUpper(),
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 9F, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };

            Button btnX = new Button
            {
                Text = "×",
                Dock = DockStyle.Right,
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(220, 90, 90),
                Font = new Font("Segoe UI", 12F, FontStyle.Regular)
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.Click += (s, e) => dialog.DialogResult = DialogResult.Cancel;

            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnX);

            Label icon = new Label
            {
                Text = "⚠",
                ForeColor = Color.FromArgb(255, 210, 80),
                Font = new Font("Segoe UI", 30F, FontStyle.Bold),
                Location = new Point(25, 62),
                Size = new Size(55, 55),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblMessage = new Label
            {
                Text = message,
                ForeColor = Color.FromArgb(220, 150, 150),
                Font = new Font("Syne", 9F, FontStyle.Regular),
                Location = new Point(92, 62),
                Size = new Size(300, 65)
            };

            Button btnYes = MagicDialogButton("YES", new Point(92, 145));
            Button btnNo = MagicDialogButton("NO", new Point(195, 145));
            Button btnCancel = MagicDialogButton("CANCEL", new Point(298, 145));

            btnYes.Click += (s, e) => dialog.DialogResult = DialogResult.Yes;
            btnNo.Click += (s, e) => dialog.DialogResult = DialogResult.No;
            btnCancel.Click += (s, e) => dialog.DialogResult = DialogResult.Cancel;

            dialog.Controls.Add(titleBar);
            dialog.Controls.Add(icon);
            dialog.Controls.Add(lblMessage);
            dialog.Controls.Add(btnYes);
            dialog.Controls.Add(btnNo);
            dialog.Controls.Add(btnCancel);

            return dialog.ShowDialog(this);
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

            return panel;
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
                    Text = folder.IsRpf ? "RPF ARCHIVE" : "FOLDER",
                    ForeColor = Color.FromArgb(140, 90, 90),
                    Font = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(10, 8)
                });

                editorPropPanel.Controls.Add(new Label
                {
                    Text = "NAME:",
                    ForeColor = Color.FromArgb(120, 80, 80),
                    Font = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(10, 26)
                });
                var txtName = new TextBox
                {
                    BackColor = Color.Black,
                    ForeColor = Color.FromArgb(210, 210, 210),
                    BorderStyle = BorderStyle.FixedSingle,
                    Text = folder.Name,
                    Size = new Size(220, 20),
                    Location = new Point(56, 23),
                    Font = new Font("Consolas", 8F)
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
                    Text = "Add to dlclist.xml on install",
                    ForeColor = Color.FromArgb(170, 130, 130),
                    BackColor = Color.Transparent,
                    Font = new Font("Syne", 7F),
                    Checked = folder.AddToDlcList,
                    AutoSize = true,
                    Location = new Point(10, 50)
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
                    Text = "This is an RPF archive",
                    ForeColor = Color.FromArgb(130, 160, 200),
                    BackColor = Color.Transparent,
                    Font = new Font("Syne", 7F),
                    Checked = folder.IsRpf,
                    AutoSize = true,
                    Location = new Point(10, 70)
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
                    Text = "FILE",
                    ForeColor = Color.FromArgb(140, 90, 90),
                    Font = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(10, 8)
                });

                editorPropPanel.Controls.Add(new Label
                {
                    Text = TruncatePath(file.SourcePath, 48),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Font = new Font("Consolas", 7F),
                    AutoSize = true,
                    Location = new Point(10, 26)
                });

                editorPropPanel.Controls.Add(new Label
                {
                    Text = "TYPE:",
                    ForeColor = Color.FromArgb(120, 80, 80),
                    Font = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(10, 50)
                });

                var typeBox = new ComboBox
                {
                    BackColor = Color.Black,
                    ForeColor = Color.FromArgb(200, 200, 200),
                    FlatStyle = FlatStyle.Flat,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Size = new Size(110, 20),
                    Location = new Point(48, 48),
                    Font = new Font("Syne", 7F)
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
                    Text = "→ " + TruncatePath(resolved, 50),
                    ForeColor = Color.FromArgb(100, 130, 100),
                    Font = new Font("Consolas", 7F),
                    AutoSize = true,
                    Location = new Point(10, 72)
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
        private void TreeCmd_NewRpf() => TreeCmd_NewNode(isRpf: true);

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
                Title = "Add file to selected folder",
                Multiselect = true,
                Filter = "All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            foreach (string path in dlg.FileNames)
            {
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id = currentProject.NextId++,
                    SourcePath = path,
                    FileName = Path.GetFileName(path),
                    SubPath = string.Empty,
                    Type = "content",
                    FolderId = parentFolderId
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
            if (e.KeyCode == Keys.F2) TreeCmd_Rename();
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
                Text = label,
                BackColor = Color.FromArgb(20, 20, 20),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(320, 86),
                MaximizeBox = false,
                MinimizeBox = false,
                ControlBox = false
            };
            dlg.Controls.Add(new Label
            {
                Text = label,
                ForeColor = Color.FromArgb(160, 110, 110),
                Font = new Font("Syne", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 10)
            });
            var txt = new TextBox
            {
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(210, 210, 210),
                BorderStyle = BorderStyle.FixedSingle,
                Text = defaultVal,
                Size = new Size(294, 22),
                Location = new Point(12, 28),
                Font = new Font("Consolas", 9F)
            };
            dlg.Controls.Add(txt);
            var ok = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(70, 0, 0),
                ForeColor = Color.FromArgb(220, 160, 160),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(70, 26),
                Location = new Point(12, 54)
            };
            ok.FlatAppearance.BorderColor = Color.FromArgb(110, 40, 40);
            dlg.Controls.Add(ok);
            dlg.Controls.Add(new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(140, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(70, 26),
                Location = new Point(90, 54)
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
                    string name = System.Net.WebUtility.HtmlEncode(file.FileName);
                    string src = System.Net.WebUtility.HtmlEncode(file.SourcePath);
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
                Title = "Select mod file(s)",
                Multiselect = true,
                Filter = "All Files (*.*)|*.*|YFT (*.yft)|*.yft|YTD (*.ytd)|*.ytd|META (*.meta)|*.meta|XML (*.xml)|*.xml|ASI (*.asi)|*.asi"
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
                Id = currentProject.NextId++,
                SourcePath = path,
                FileName = Path.GetFileName(path),
                TargetPath = "",
                Type = "content"
            });
            MarkDirty();
        }

        // ─────────────────── PHOTO PREVIEW ───────────────────

        private void btnAddPhoto_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select preview image",
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                selectedPhotoPath = dlg.FileName;
                btnAddPhoto.Text = "CHANGE";
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
            using var font = new Font("Syne", 8F);
            using var brush = new SolidBrush(Color.FromArgb(70, 70, 70));
            var rect = panelPhotoPreview.ClientRectangle;
            e.Graphics.DrawString("No photo", font, brush, rect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        // ─────────────────── COLOR PICKER ───────────────────


        // ─────────────────── BUILD OIV ───────────────────

        private async void btnBuildOIV_Click(object sender, EventArgs e)
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
                Title = "Save OIV Package",
                Filter = "OIV Package (*.oiv)|*.oiv",
                FileName = currentProject.ModName.Replace(" ", "_") + ".oiv"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using Form progressDialog = CreateBuildProgressDialog(out ProgressBar progressBar, out Label lblPercent);

            var progress = new Progress<int>(value =>
            {
                value = Math.Max(0, Math.Min(100, value));
                progressBar.Value = value;
                lblPercent.Text = $"{value}%";
            });

            try
            {
                btnBuildOIV.Enabled = false;
                btnSidebarBuildOIV.Enabled = false;

                progressDialog.Show(this);
                progressDialog.Refresh();

                await Task.Run(() =>
                {
                    OIVBuilder.Build(currentProject, dlg.FileName, progress);
                });

                progressDialog.Close();

                MarkClean();

                ShowMagicInfoBox($"OIV package built successfully!\n\n{dlg.FileName}", "Build Complete");
            }
            catch (Exception ex)
            {
                progressDialog.Close();

                MessageBox.Show("Build failed: " + ex.Message, "Build Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnBuildOIV.Enabled = true;
                btnSidebarBuildOIV.Enabled = true;
            }
        }

        // ─────────────────── PROJECT SYNC ───────────────────

        private void SyncUIToProject()
        {
            currentProject.ModName = txtModName.Text.Trim();
            currentProject.Author = txtAuthor.Text.Trim();
            currentProject.Website = txtWebsite?.Text.Trim() ?? "";
            currentProject.Version = txtVersion.Text.Trim();
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
                if (txtWebsite != null)
                    txtWebsite.Text = string.Empty;
                txtVersion.Text = string.Empty;
                txtDescription.Text = string.Empty;

                dropdownVersionTag.SelectedIndex = -1;

                panelColorPicker.BackColor = Color.FromArgb(52, 38, 40); // default banner color
                selectedPhotoPath = null;
                btnAddPhoto.Text = "ADD";
                panelPhotoPreview.Invalidate();

                // Optional: clear editor selection state if editor is open
                if (editorTree != null)
                    editorTree.SelectedNode = null;

                // Now load current project values
                txtModName.Text = currentProject.ModName ?? string.Empty;
                txtAuthor.Text = currentProject.Author ?? string.Empty;
                if (txtWebsite != null)
                    txtWebsite.Text = currentProject.Website ?? string.Empty;
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
            Color normal = Color.FromArgb(92, 18, 22);
            Color hover = Color.FromArgb(125, 32, 38);

            btn.BackColor = normal;
            btn.ForeColor = Color.FromArgb(210, 150, 150);

            btn.MouseEnter += (s, e) => btn.BackColor = hover;
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
            if (allowCloseWithoutPrompt)
                return;

            if (!isDirty)
                return;

            e.Cancel = true;

            var result = ShowMagicConfirmBox(
                "You have unsaved changes.\n\nDo you want to save before exiting?",
                "Unsaved Changes"
            );

            if (result == DialogResult.Cancel)
                return;

            if (result == DialogResult.Yes)
            {
                bool saved = TrySaveProject();

                if (!saved)
                    return;
            }

            // YES saved, or NO = close safely
            allowCloseWithoutPrompt = true;

            BeginInvoke(new Action(() =>
            {
                Close();
            }));
        }

        private bool ConfirmDiscardOrSaveChanges()
        {
            if (!isDirty)
                return true;

            var result = ShowMagicConfirmBox(
             "You have unsaved changes.\n\nDo you want to save before continuing?",
              "Unsaved Changes"
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
            logo.Parent = panelMatrixTitle;
            logo.Size = new Size(370, 170);

            int topZoneHeight = 120; // adjust this

            logo.Location = new Point(
                    (panelMatrixTitle.Width - logo.Width) / 2,
                    (topZoneHeight - logo.Height) / 2
             );

            panelMatrixTitle.Resize += (s, e) =>
            {
                int topZoneHeight = 120;

                logo.Location = new Point(
                    (panelMatrixTitle.Width - logo.Width) / 2,
                    (topZoneHeight - logo.Height) / 2
                );
            };

            panelMatrixTitle.Controls.Add(logo);

            foreach (Control c in panelMatrixTitle.Controls)
            {
                c.BringToFront();
            }
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
