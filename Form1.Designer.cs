using System.Drawing;
using System.Windows.Forms;

namespace MagicOGK_OIV_Builder
{
    partial class main
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            sidebarTimer    = new System.Windows.Forms.Timer(components);
            editorTimer     = new System.Windows.Forms.Timer(components);

            panelDrag       = new Panel();
            panelLeft       = new Panel();
            panelEditorRight= new Panel();
            panelRight      = new Panel();
            panelSidebar    = new Panel();
            panelDropZone   = new Panel();
            panelPhotoPreview = new Panel();
            panelColorPicker  = new Panel();

            button5         = new Button();
            button6         = new Button();
            button7         = new Button();
            btnHamburger    = new Button();

            lblAuthor       = new Label();
            lblModName      = new Label();
            lblVersionTag   = new Label();
            lblVersion      = new Label();
            lblDescription  = new Label();
            lblPhotoLabel   = new Label();
            lblColorLabel   = new Label();
            lblPackageFiles = new Label();
            lblAddFilesHint = new Label();
            lblNoFiles      = new Label();
            lblSidebarTitle = new Label();

            txtAuthor       = new TextBox();
            txtModName      = new TextBox();
            txtVersion      = new TextBox();
            txtDescription  = new TextBox();
            dropdownVersionTag = new ComboBox();

            btnAddPhoto     = new Button();
            btnOpenEditor   = new Button();
            btnBuildOIV     = new Button();
            btnAddFiles     = new Button();

            btnSidebarOpenProject   = new Button();
            btnSidebarOpenOIV       = new Button();
            btnSidebarSaveProjectAs = new Button();
            btnSidebarBuildOIV      = new Button();
            btnSidebarFeedback      = new Button();

            webViewFileList = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewFileList).BeginInit();

            SuspendLayout();

            // ────────── FORM ──────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            BackColor           = Color.FromArgb(13, 13, 13);
            ClientSize          = new Size(1030, 660);
            ControlBox          = false;
            Font                = new Font("Syne", 9F);
            FormBorderStyle     = FormBorderStyle.None;
            Name                = "main";
            StartPosition       = FormStartPosition.CenterScreen;
            Text                = "MagicOGK OIV Builder";

            // ────────── panelDrag (DockTop) ──────────
            panelDrag.BackColor = Color.FromArgb(10, 10, 10);
            panelDrag.Dock      = DockStyle.Top;
            panelDrag.Height    = 28;
            panelDrag.Controls.Add(btnHamburger);
            panelDrag.Controls.Add(button7);
            panelDrag.Controls.Add(button6);
            panelDrag.Controls.Add(button5);

            btnHamburger.BackColor = Color.Transparent;
            btnHamburger.FlatStyle = FlatStyle.Flat;
            btnHamburger.FlatAppearance.BorderSize = 0;
            btnHamburger.ForeColor = Color.FromArgb(188, 143, 143);
            btnHamburger.Font      = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnHamburger.Text      = "\u2630";
            btnHamburger.Size      = new Size(36, 28);
            btnHamburger.Location  = new Point(4, 0);
            btnHamburger.TabStop   = false;

            button7.BackColor = Color.LimeGreen;
            button7.FlatStyle = FlatStyle.Flat;
            button7.FlatAppearance.BorderColor = Color.DimGray;
            button7.FlatAppearance.BorderSize  = 1;
            button7.Size     = new Size(14, 14);
            button7.Location = new Point(969, 7);
            button7.TabStop  = false;

            button6.BackColor = Color.Khaki;
            button6.FlatStyle = FlatStyle.Flat;
            button6.FlatAppearance.BorderColor = Color.DimGray;
            button6.FlatAppearance.BorderSize  = 1;
            button6.Size     = new Size(14, 14);
            button6.Location = new Point(986, 7);
            button6.TabStop  = false;

            button5.BackColor = Color.IndianRed;
            button5.FlatStyle = FlatStyle.Flat;
            button5.FlatAppearance.BorderColor = Color.DimGray;
            button5.FlatAppearance.BorderSize  = 1;
            button5.Size     = new Size(14, 14);
            button5.Location = new Point(1003, 7);
            button5.TabStop  = false;

            // ────────── panelSidebar (DockLeft, starts width=0) ──────────
            panelSidebar.BackColor     = Color.FromArgb(15, 15, 15);
            panelSidebar.Dock          = DockStyle.Left;
            panelSidebar.Width         = 0;
            panelSidebar.Name          = "panelSidebar";

            lblSidebarTitle.Text      = "MAGICOGK";
            lblSidebarTitle.ForeColor = Color.FromArgb(139, 58, 58);
            lblSidebarTitle.Font      = new Font("Syne", 9F, FontStyle.Bold);
            lblSidebarTitle.Location  = new Point(14, 14);
            lblSidebarTitle.AutoSize  = true;

            StyleSidebarBtn(btnSidebarOpenProject,   "  Open Project",   55);
            StyleSidebarBtn(btnSidebarSaveProjectAs, "  Save Project As", 105);
            StyleSidebarBtn(btnSidebarOpenOIV,       "  Open OIV",        155);
            StyleSidebarBtn(btnSidebarBuildOIV,      "  Build OIV",       205);
            StyleSidebarBtn(btnSidebarFeedback,      "  Feedback",        530);

            panelSidebar.Controls.Add(lblSidebarTitle);
            panelSidebar.Controls.Add(btnSidebarOpenProject);
            panelSidebar.Controls.Add(btnSidebarSaveProjectAs);
            panelSidebar.Controls.Add(btnSidebarOpenOIV);
            panelSidebar.Controls.Add(btnSidebarBuildOIV);
            panelSidebar.Controls.Add(btnSidebarFeedback);

            // ────────── panelLeft (DockLeft) ──────────
            panelLeft.BackColor  = Color.FromArgb(18, 18, 18);
            panelLeft.Dock       = DockStyle.Left;
            panelLeft.Width      = 360;
            panelLeft.AutoScroll = false;

            SetupLeftPanelControls();

            // ────────── panelEditorRight (DockRight, starts width=0) ──────────
            panelEditorRight.BackColor = Color.FromArgb(22, 22, 22);
            panelEditorRight.Dock      = DockStyle.Right;
            panelEditorRight.Width     = 0;
            panelEditorRight.AutoScroll = true;

            // ────────── panelRight (DockFill) ──────────
            panelRight.BackColor = Color.FromArgb(13, 13, 13);
            panelRight.Dock      = DockStyle.Fill;

            SetupRightPanelControls();

            // ────────── Add to form in correct dock order ──────────
            // DockTop first, then Left panels left-to-right, then Right, then Fill
            Controls.Add(panelRight);        // Fill
            Controls.Add(panelEditorRight);  // Right
            Controls.Add(panelLeft);         // Left (inner)
            Controls.Add(panelSidebar);      // Left (outer, over panelLeft)
            Controls.Add(panelDrag);         // Top

            // wire window button events
            button5.Click += button5_Click;
            button6.Click += button6_Click;
            button7.Click += button7_Click;
            btnHamburger.Click += btnHamburger_Click;

            ((System.ComponentModel.ISupportInitialize)webViewFileList).EndInit();
            ResumeLayout(false);
        }

        private void StyleSidebarBtn(Button btn, string text, int y)
        {
            btn.Text      = text;
            btn.ForeColor = Color.FromArgb(188, 143, 143);
            btn.BackColor = Color.FromArgb(15, 15, 15);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize  = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 0, 0);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Font      = new Font("Syne", 9F);
            btn.Size      = new Size(200, 40);
            btn.Location  = new Point(0, y);
            btn.TabStop   = false;
        }

        private void SetupLeftPanelControls()
        {
            // ── Author ──
            lblAuthor.Text      = "AUTHOR";
            lblAuthor.ForeColor = Color.FromArgb(188, 143, 143);
            lblAuthor.Font      = new Font("Syne", 8F, FontStyle.Bold);
            lblAuthor.Location  = new Point(30, 28);
            lblAuthor.AutoSize  = true;

            txtAuthor.BackColor       = Color.Black;
            txtAuthor.ForeColor       = Color.FromArgb(200, 200, 200);
            txtAuthor.BorderStyle     = BorderStyle.FixedSingle;
            txtAuthor.PlaceholderText = "Mod author's name...";
            txtAuthor.Size            = new Size(300, 24);
            txtAuthor.Location        = new Point(30, 50);
            txtAuthor.Font            = new Font("Syne", 9F);

            // ── Mod Name ──
            lblModName.Text      = "MOD NAME";
            lblModName.ForeColor = Color.FromArgb(188, 143, 143);
            lblModName.Font      = new Font("Syne", 8F, FontStyle.Bold);
            lblModName.Location  = new Point(30, 88);
            lblModName.AutoSize  = true;

            txtModName.BackColor       = Color.Black;
            txtModName.ForeColor       = Color.FromArgb(200, 200, 200);
            txtModName.BorderStyle     = BorderStyle.FixedSingle;
            txtModName.PlaceholderText = "Mod name...";
            txtModName.Size            = new Size(300, 24);
            txtModName.Location        = new Point(30, 110);
            txtModName.Font            = new Font("Syne", 9F);

            // ── Version Tag ──
            lblVersionTag.Text      = "VERSION TAG";
            lblVersionTag.ForeColor = Color.FromArgb(188, 143, 143);
            lblVersionTag.Font      = new Font("Syne", 8F, FontStyle.Bold);
            lblVersionTag.Location  = new Point(30, 150);
            lblVersionTag.AutoSize  = true;

            dropdownVersionTag.BackColor     = Color.Black;
            dropdownVersionTag.ForeColor     = Color.FromArgb(200, 200, 200);
            dropdownVersionTag.DropDownStyle = ComboBoxStyle.DropDownList;
            dropdownVersionTag.FlatStyle     = FlatStyle.Flat;
            dropdownVersionTag.Size          = new Size(140, 24);
            dropdownVersionTag.Location      = new Point(30, 172);
            dropdownVersionTag.Font          = new Font("Syne", 9F);
            dropdownVersionTag.Items.AddRange(new object[] { "Test", "Alpha", "Beta", "Stable" });
            dropdownVersionTag.SelectedIndex = 3;

            // ── Version ──
            lblVersion.Text      = "VERSION";
            lblVersion.ForeColor = Color.FromArgb(188, 143, 143);
            lblVersion.Font      = new Font("Syne", 8F, FontStyle.Bold);
            lblVersion.Location  = new Point(185, 150);
            lblVersion.AutoSize  = true;

            txtVersion.BackColor   = Color.Black;
            txtVersion.ForeColor   = Color.FromArgb(200, 200, 200);
            txtVersion.BorderStyle = BorderStyle.FixedSingle;
            txtVersion.Text        = "1.0";
            txtVersion.Size        = new Size(145, 24);
            txtVersion.Location    = new Point(185, 172);
            txtVersion.Font        = new Font("Syne", 9F);

            // ── Description ──
            lblDescription.Text      = "DESCRIPTION";
            lblDescription.ForeColor = Color.FromArgb(188, 143, 143);
            lblDescription.Font      = new Font("Syne", 8F, FontStyle.Bold);
            lblDescription.Location  = new Point(30, 212);
            lblDescription.AutoSize  = true;

            txtDescription.BackColor       = Color.Black;
            txtDescription.ForeColor       = Color.FromArgb(200, 200, 200);
            txtDescription.BorderStyle     = BorderStyle.FixedSingle;
            txtDescription.PlaceholderText = "Write a description for your mod...";
            txtDescription.Multiline       = true;
            txtDescription.Size            = new Size(300, 75);
            txtDescription.Location        = new Point(30, 234);
            txtDescription.Font            = new Font("Syne", 9F);
            txtDescription.ScrollBars      = ScrollBars.Vertical;

            // ── Photo preview ──
            lblPhotoLabel.Text      = "PHOTO PREVIEW";
            lblPhotoLabel.ForeColor = Color.FromArgb(188, 143, 143);
            lblPhotoLabel.Font      = new Font("Syne", 8F, FontStyle.Bold);
            lblPhotoLabel.Location  = new Point(30, 325);
            lblPhotoLabel.AutoSize  = true;

            panelPhotoPreview.BackColor   = Color.FromArgb(35, 35, 35);
            panelPhotoPreview.BorderStyle = BorderStyle.FixedSingle;
            panelPhotoPreview.Size        = new Size(140, 100);
            panelPhotoPreview.Location    = new Point(30, 347);

            btnAddPhoto.BackColor = Color.FromArgb(50, 50, 50);
            btnAddPhoto.ForeColor = Color.FromArgb(200, 200, 200);
            btnAddPhoto.FlatStyle = FlatStyle.Flat;
            btnAddPhoto.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnAddPhoto.Text      = "ADD PHOTO";
            btnAddPhoto.Size      = new Size(140, 28);
            btnAddPhoto.Location  = new Point(30, 452);
            btnAddPhoto.Font      = new Font("Syne", 8F, FontStyle.Bold);

            // ── Banner color picker ──
            lblColorLabel.Text      = "BANNER COLOR";
            lblColorLabel.ForeColor = Color.FromArgb(188, 143, 143);
            lblColorLabel.Font      = new Font("Syne", 8F, FontStyle.Bold);
            lblColorLabel.Location  = new Point(185, 325);
            lblColorLabel.AutoSize  = true;

            panelColorPicker.BackColor   = Color.FromArgb(160, 0, 0);
            panelColorPicker.BorderStyle = BorderStyle.FixedSingle;
            panelColorPicker.Size        = new Size(145, 100);
            panelColorPicker.Location    = new Point(185, 347);
            panelColorPicker.Cursor      = Cursors.Hand;

            // ── Open Editor ──
            btnOpenEditor.BackColor = Color.FromArgb(64, 0, 0);
            btnOpenEditor.ForeColor = Color.FromArgb(200, 140, 140);
            btnOpenEditor.FlatStyle = FlatStyle.Flat;
            btnOpenEditor.FlatAppearance.BorderColor = Color.FromArgb(110, 40, 40);
            btnOpenEditor.Text     = "Open Editor";
            btnOpenEditor.Size     = new Size(300, 46);
            btnOpenEditor.Location = new Point(30, 496);
            btnOpenEditor.Font     = new Font("Syne", 11F);

            // ── Build OIV ──
            btnBuildOIV.BackColor = Color.FromArgb(80, 0, 0);
            btnBuildOIV.ForeColor = Color.FromArgb(220, 160, 160);
            btnBuildOIV.FlatStyle = FlatStyle.Flat;
            btnBuildOIV.FlatAppearance.BorderColor = Color.FromArgb(110, 40, 40);
            btnBuildOIV.Text     = "Build OIV";
            btnBuildOIV.Size     = new Size(300, 46);
            btnBuildOIV.Location = new Point(30, 554);
            btnBuildOIV.Font     = new Font("Syne", 11F);

            // Separator line at bottom of left panel
            var sep = new Panel
            {
                BackColor = Color.FromArgb(35, 35, 35),
                Dock      = DockStyle.Bottom,
                Height    = 1
            };

            panelLeft.Controls.AddRange(new Control[] {
                lblAuthor, txtAuthor,
                lblModName, txtModName,
                lblVersionTag, dropdownVersionTag,
                lblVersion, txtVersion,
                lblDescription, txtDescription,
                lblPhotoLabel, panelPhotoPreview, btnAddPhoto,
                lblColorLabel, panelColorPicker,
                btnOpenEditor, btnBuildOIV,
                sep
            });
        }

        private void SetupRightPanelControls()
        {
            // ── Top toolbar panel (DockTop) ──
            var panelRightTop = new Panel
            {
                BackColor = Color.FromArgb(13, 13, 13),
                Dock      = DockStyle.Top,
                Height    = 240,
                Padding   = new Padding(30, 20, 30, 10)
            };

            // PACKAGE FILES label
            lblPackageFiles.Text      = "PACKAGE FILES";
            lblPackageFiles.ForeColor = Color.FromArgb(188, 143, 143);
            lblPackageFiles.Font      = new Font("Syne", 11F, FontStyle.Bold);
            lblPackageFiles.Location  = new Point(30, 20);
            lblPackageFiles.AutoSize  = true;

            // Add Files button
            btnAddFiles.BackColor = Color.FromArgb(139, 30, 30);
            btnAddFiles.ForeColor = Color.FromArgb(240, 192, 192);
            btnAddFiles.FlatStyle = FlatStyle.Flat;
            btnAddFiles.FlatAppearance.BorderColor = Color.FromArgb(139, 58, 58);
            btnAddFiles.Text     = "+ Add Files";
            btnAddFiles.Size     = new Size(105, 30);
            btnAddFiles.Location = new Point(30, 55);
            btnAddFiles.Font     = new Font("Syne", 9F);

            lblAddFilesHint.Text      = "or drag & drop files below";
            lblAddFilesHint.ForeColor = Color.FromArgb(90, 90, 90);
            lblAddFilesHint.AutoSize  = true;
            lblAddFilesHint.Location  = new Point(145, 63);
            lblAddFilesHint.Font      = new Font("Syne", 8F);

            // Drop zone
            panelDropZone.BackColor   = Color.FromArgb(17, 17, 17);
            panelDropZone.BorderStyle = BorderStyle.FixedSingle;
            panelDropZone.Location    = new Point(30, 96);
            panelDropZone.Size        = new Size(530, 120);
            panelDropZone.AllowDrop   = true;

            lblNoFiles.Text      = "Drop mod files here\r\nSupports .yft .ytd .meta .xml .asi .dlc and more";
            lblNoFiles.ForeColor = Color.FromArgb(90, 90, 90);
            lblNoFiles.TextAlign = ContentAlignment.MiddleCenter;
            lblNoFiles.Dock      = DockStyle.Fill;
            lblNoFiles.Font      = new Font("Syne", 9F);

            panelDropZone.Controls.Add(lblNoFiles);

            panelRightTop.Controls.AddRange(new Control[] {
                lblPackageFiles, btnAddFiles, lblAddFilesHint, panelDropZone
            });

            // WebView fills the rest
            webViewFileList.Dock = DockStyle.Fill;

            panelRight.Controls.Add(webViewFileList);
            panelRight.Controls.Add(panelRightTop);
        }

        // ── Field declarations ──────────────────────────────────────────────
        private Panel       panelDrag;
        private Panel       panelLeft;
        private Panel       panelRight;
        private Panel       panelEditorRight;
        private Panel       panelSidebar;
        private Panel       panelDropZone;
        private Panel       panelPhotoPreview;
        private Panel       panelColorPicker;

        private Label       lblAuthor;
        private Label       lblModName;
        private Label       lblVersionTag;
        private Label       lblVersion;
        private Label       lblDescription;
        private Label       lblPhotoLabel;
        private Label       lblColorLabel;
        private Label       lblPackageFiles;
        private Label       lblAddFilesHint;
        private Label       lblNoFiles;
        private Label       lblSidebarTitle;

        private TextBox     txtAuthor;
        private TextBox     txtModName;
        private TextBox     txtVersion;
        private TextBox     txtDescription;
        private ComboBox    dropdownVersionTag;

        private Button      button5;
        private Button      button6;
        private Button      button7;
        private Button      btnHamburger;
        private Button      btnAddPhoto;
        private Button      btnOpenEditor;
        private Button      btnBuildOIV;
        private Button      btnAddFiles;
        private Button      btnSidebarOpenProject;
        private Button      btnSidebarOpenOIV;
        private Button      btnSidebarSaveProjectAs;
        private Button      btnSidebarBuildOIV;
        private Button      btnSidebarFeedback;

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewFileList;

        private System.Windows.Forms.Timer sidebarTimer;
        private System.Windows.Forms.Timer editorTimer;
    }
}
