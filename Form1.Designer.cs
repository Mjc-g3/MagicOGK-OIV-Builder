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
            //buttons animated
            btnReplaceMods = new AnimatedGlowButton();
            btnSidebarOpenProject = new AnimatedGlowButton();
            btnSidebarSaveProjectAs = new AnimatedGlowButton();
            btnSidebarOpenOIV = new AnimatedGlowButton();
            btnSidebarExtractOIV = new AnimatedGlowButton();
            btnSidebarBuildOIV = new AnimatedGlowButton();
            btnCheckUpdates = new AnimatedGlowButton();
            btnSidebarFeedback = new AnimatedGlowButton();
            btnAddPhoto = new AnimatedGlowButton();
            btnOpenEditor = new AnimatedGlowButton();
            btnBuildOIV = new AnimatedGlowButton();
            btnAddFiles = new AnimatedGlowButton();
            //panel animated
            panelColorPicker = new AnimatedGlowPanel();

            //rest
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(main));
            sidebarTimer = new System.Windows.Forms.Timer(components);
            editorTimer = new System.Windows.Forms.Timer(components);
            panelDrag = new Panel();
            btnHamburger = new Button();
            panelMarquee = new BufferedPanel();
            button7 = new Button();
            button6 = new Button();
            button5 = new Button();
            panelLeft = new Panel();
            panelRight = new Panel();
            panelSidebar = new Panel();
            panelMatrixTitle = new GalaxySidebarPanel();
            panelEditorRight = new BufferedPanel();
            panelDropZone = new Panel();
            panelPhotoPreview = new Panel();
            
            lblAuthor = new Label();
            lblModName = new Label();
            lblVersionTag = new Label();
            lblVersion = new Label();
            lblDescription = new Label();
            lblPhotoLabel = new Label();
            lblColorLabel = new Label();
            lblPackageFiles = new Label();
            lblAddFilesHint = new Label();
            lblNoFiles = new Label();
            txtAuthor = new TextBox();
            txtModName = new TextBox();
            txtVersion = new TextBox();
            txtDescription = new CustomScrollTextBox();
            dropdownVersionTag = new CustomDropdown();
            webViewFileList = new Microsoft.Web.WebView2.WinForms.WebView2();
            panelDrag.SuspendLayout();
            panelLeft.SuspendLayout();
            panelSidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webViewFileList).BeginInit();
            SuspendLayout();
            // 
            // panelDrag
            // 
            panelDrag.BackColor = Color.FromArgb(10, 10, 10);
            panelDrag.Controls.Add(btnHamburger);
            panelDrag.Controls.Add(panelMarquee);
            panelDrag.Controls.Add(button7);
            panelDrag.Controls.Add(button6);
            panelDrag.Controls.Add(button5);
            panelDrag.Dock = DockStyle.Top;
            panelDrag.Location = new Point(0, 0);
            panelDrag.Name = "panelDrag";
            panelDrag.Size = new Size(1030, 28);
            panelDrag.TabIndex = 4;
            
            // 
            // btnHamburger
            // 
            btnHamburger.BackColor = Color.Transparent;
            btnHamburger.FlatAppearance.BorderSize = 0;
            btnHamburger.FlatStyle = FlatStyle.Flat;
            btnHamburger.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnHamburger.ForeColor = Color.FromArgb(188, 143, 143);
            btnHamburger.Location = new Point(4, 0);
            btnHamburger.Name = "btnHamburger";
            btnHamburger.Size = new Size(36, 28);
            btnHamburger.TabIndex = 0;
            btnHamburger.TabStop = false;
            btnHamburger.Text = "☰";
            btnHamburger.UseVisualStyleBackColor = false;
            btnHamburger.Click += btnHamburger_Click;
            
            //
            // panelMarquee
            // 
            panelMarquee.BackColor = Color.Transparent;
            panelMarquee.Location = new Point(46, 0);
            panelMarquee.Name = "panelMarquee";
            panelMarquee.Size = new Size(900, 28);
            panelMarquee.TabIndex = 1;
            // 
            // button7
            // 
            button7.BackColor = Color.LimeGreen;
            button7.FlatAppearance.BorderColor = Color.DimGray;
            button7.FlatStyle = FlatStyle.Flat;
            button7.Location = new Point(969, 7);
            button7.Name = "button7";
            button7.Size = new Size(14, 14);
            button7.TabIndex = 1;
            button7.TabStop = false;
            button7.UseVisualStyleBackColor = false;
            button7.Click += button7_Click;
            // 
            // button6
            // 
            button6.BackColor = Color.Khaki;
            button6.FlatAppearance.BorderColor = Color.DimGray;
            button6.FlatStyle = FlatStyle.Flat;
            button6.Location = new Point(986, 7);
            button6.Name = "button6";
            button6.Size = new Size(14, 14);
            button6.TabIndex = 2;
            button6.TabStop = false;
            button6.UseVisualStyleBackColor = false;
            button6.Click += button6_Click;
            // 
            // button5
            // 
            button5.BackColor = Color.IndianRed;
            button5.FlatAppearance.BorderColor = Color.DimGray;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Location = new Point(1003, 7);
            button5.Name = "button5";
            button5.Size = new Size(14, 14);
            button5.TabIndex = 3;
            button5.TabStop = false;
            button5.UseVisualStyleBackColor = false;
            button5.Click += button5_Click;
            // 
            // panelLeft
            // 
            panelLeft.BackColor = Color.FromArgb(18, 18, 18);
            panelLeft.Controls.Add(btnReplaceMods);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 28);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(360, 632);
            panelLeft.TabIndex = 2;
            // 
            // btnReplaceMods
            // 
            btnReplaceMods.BackColor = Color.FromArgb(64, 0, 0);
            btnReplaceMods.FlatAppearance.BorderColor = Color.FromArgb(110, 40, 40);
            btnReplaceMods.FlatStyle = FlatStyle.Flat;
            btnReplaceMods.Font = new Font("Syne", 11F);
            btnReplaceMods.ForeColor = Color.FromArgb(200, 140, 140);
            btnReplaceMods.Location = new Point(185, 452);
            btnReplaceMods.Name = "btnReplaceMods";
            btnReplaceMods.Size = new Size(146, 28);
            btnReplaceMods.TabIndex = 0;
            btnReplaceMods.Text = "Replace Menu";
            btnReplaceMods.UseVisualStyleBackColor = false;
            // 
            // panelEditorRight
            // 
            panelEditorRight.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panelEditorRight.BackColor = Color.FromArgb(22, 22, 22);
            panelEditorRight.Location = new Point(1030, 28);
            panelEditorRight.Name = "panelEditorRight";
            panelEditorRight.Size = new Size(0, 632);
            panelEditorRight.TabIndex = 1;
            // 
            // panelRight
            // 
            panelRight.BackColor = Color.FromArgb(13, 13, 13);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(360, 28);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(670, 632);
            panelRight.TabIndex = 0;
            // 
            // panelSidebar
            // 
            panelSidebar.BackColor = Color.FromArgb(15, 15, 15);
            panelSidebar.Controls.Add(panelMatrixTitle);
            panelSidebar.Controls.Add(btnSidebarOpenProject);
            panelSidebar.Controls.Add(btnSidebarSaveProjectAs);
            panelSidebar.Controls.Add(btnSidebarOpenOIV);
            panelSidebar.Controls.Add(btnSidebarExtractOIV);
            panelSidebar.Controls.Add(btnSidebarBuildOIV);
            panelSidebar.Controls.Add(btnCheckUpdates);
            panelSidebar.Controls.Add(btnSidebarFeedback);
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Location = new Point(0, 28);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(0, 632);
            panelSidebar.TabIndex = 3;
            // 
            // panelMatrixTitle
            // 
            panelMatrixTitle.BackColor = Color.FromArgb(15, 15, 15);
            panelMatrixTitle.Location = new Point(0, 0);
            panelMatrixTitle.Name = "panelMatrixTitle";
            panelMatrixTitle.Size = new Size(200, 110);
            panelMatrixTitle.TabIndex = 0;
            // 
            // btnSidebarOpenProject
            // 
            btnSidebarOpenProject.Location = new Point(0, 0);
            btnSidebarOpenProject.Name = "btnSidebarOpenProject";
            btnSidebarOpenProject.Size = new Size(75, 23);
            btnSidebarOpenProject.TabIndex = 1;
            // 
            // btnSidebarSaveProjectAs
            // 
            btnSidebarSaveProjectAs.Location = new Point(0, 0);
            btnSidebarSaveProjectAs.Name = "btnSidebarSaveProjectAs";
            btnSidebarSaveProjectAs.Size = new Size(75, 23);
            btnSidebarSaveProjectAs.TabIndex = 2;
            // 
            // btnSidebarOpenOIV
            // 
            btnSidebarOpenOIV.Location = new Point(0, 0);
            btnSidebarOpenOIV.Name = "btnSidebarOpenOIV";
            btnSidebarOpenOIV.Size = new Size(75, 23);
            btnSidebarOpenOIV.TabIndex = 3;
            // 
            // btnSidebarExtractOIV
            // 
            btnSidebarExtractOIV.Location = new Point(0, 0);
            btnSidebarExtractOIV.Name = "btnSidebarExtractOIV";
            btnSidebarExtractOIV.Size = new Size(75, 23);
            btnSidebarExtractOIV.TabIndex = 3;
            // 
            // btnSidebarBuildOIV
            // 
            btnSidebarBuildOIV.Location = new Point(0, 0);
            btnSidebarBuildOIV.Name = "btnSidebarBuildOIV";
            btnSidebarBuildOIV.Size = new Size(75, 23);
            btnSidebarBuildOIV.TabIndex = 4;
            // 
            // btnCheckUpdates
            // 
            btnCheckUpdates.Location = new Point(0, 0);
            btnCheckUpdates.Name = "btnCheckUpdates";
            btnCheckUpdates.Size = new Size(75, 23);
            btnCheckUpdates.TabIndex = 4;
            // 
            // btnSidebarFeedback
            // 
            btnSidebarFeedback.Location = new Point(0, 0);
            btnSidebarFeedback.Name = "btnSidebarFeedback";
            btnSidebarFeedback.Size = new Size(75, 23);
            btnSidebarFeedback.TabIndex = 5;
            // 
            // panelDropZone
            // 
            panelDropZone.Location = new Point(0, 0);
            panelDropZone.Name = "panelDropZone";
            panelDropZone.Size = new Size(200, 100);
            panelDropZone.TabIndex = 0;
            // 
            // panelPhotoPreview
            // 
            panelPhotoPreview.Location = new Point(0, 0);
            panelPhotoPreview.Name = "panelPhotoPreview";
            panelPhotoPreview.Size = new Size(200, 100);
            panelPhotoPreview.TabIndex = 0;
            // 
            // panelColorPicker
            // 
            panelColorPicker.Location = new Point(0, 0);
            panelColorPicker.Name = "panelColorPicker";
            panelColorPicker.Size = new Size(200, 100);
            panelColorPicker.TabIndex = 0;
            // 
            // lblAuthor
            // 
            lblAuthor.Location = new Point(0, 0);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(100, 23);
            lblAuthor.TabIndex = 0;
            // 
            // lblModName
            // 
            lblModName.Location = new Point(0, 0);
            lblModName.Name = "lblModName";
            lblModName.Size = new Size(100, 23);
            lblModName.TabIndex = 0;
            // 
            // lblVersionTag
            // 
            lblVersionTag.Location = new Point(0, 0);
            lblVersionTag.Name = "lblVersionTag";
            lblVersionTag.Size = new Size(100, 23);
            lblVersionTag.TabIndex = 0;
            // 
            // lblVersion
            // 
            lblVersion.Location = new Point(0, 0);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(100, 23);
            lblVersion.TabIndex = 0;
            // 
            // lblDescription
            // 
            lblDescription.Location = new Point(0, 0);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(100, 23);
            lblDescription.TabIndex = 0;
            // 
            // lblPhotoLabel
            // 
            lblPhotoLabel.Location = new Point(0, 0);
            lblPhotoLabel.Name = "lblPhotoLabel";
            lblPhotoLabel.Size = new Size(100, 23);
            lblPhotoLabel.TabIndex = 0;
            // 
            // lblColorLabel
            // 
            lblColorLabel.Location = new Point(0, 0);
            lblColorLabel.Name = "lblColorLabel";
            lblColorLabel.Size = new Size(100, 23);
            lblColorLabel.TabIndex = 0;
            // 
            // lblPackageFiles
            // 
            lblPackageFiles.Location = new Point(0, 0);
            lblPackageFiles.Name = "lblPackageFiles";
            lblPackageFiles.Size = new Size(100, 23);
            lblPackageFiles.TabIndex = 0;
            // 
            // lblAddFilesHint
            // 
            lblAddFilesHint.Location = new Point(0, 0);
            lblAddFilesHint.Name = "lblAddFilesHint";
            lblAddFilesHint.Size = new Size(100, 23);
            lblAddFilesHint.TabIndex = 0;
            // 
            // lblNoFiles
            // 
            lblNoFiles.Location = new Point(0, 0);
            lblNoFiles.Name = "lblNoFiles";
            lblNoFiles.Size = new Size(100, 23);
            lblNoFiles.TabIndex = 0;
            // 
            // txtAuthor
            // 
            txtAuthor.Location = new Point(0, 0);
            txtAuthor.Name = "txtAuthor";
            txtAuthor.Size = new Size(100, 23);
            txtAuthor.TabIndex = 0;
            // 
            // txtModName
            // 
            txtModName.Location = new Point(0, 0);
            txtModName.Name = "txtModName";
            txtModName.Size = new Size(100, 23);
            txtModName.TabIndex = 0;
            // 
            // txtVersion
            // 
            txtVersion.Location = new Point(0, 0);
            txtVersion.Name = "txtVersion";
            txtVersion.Size = new Size(100, 23);
            txtVersion.TabIndex = 0;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(0, 0);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(100, 23);
            txtDescription.TabIndex = 0;
            // 
            // dropdownVersionTag
            // 
            dropdownVersionTag.Location = new Point(0, 0);
            dropdownVersionTag.Name = "dropdownVersionTag";
            dropdownVersionTag.Size = new Size(121, 23);
            dropdownVersionTag.TabIndex = 0;
            // 
            // btnAddPhoto
            // 
            btnAddPhoto.Location = new Point(0, 0);
            btnAddPhoto.Name = "btnAddPhoto";
            btnAddPhoto.Size = new Size(75, 23);
            btnAddPhoto.TabIndex = 0;
            // 
            // btnOpenEditor
            // 
            btnOpenEditor.Location = new Point(0, 0);
            btnOpenEditor.Name = "btnOpenEditor";
            btnOpenEditor.Size = new Size(75, 23);
            btnOpenEditor.TabIndex = 0;
            // 
            // btnBuildOIV
            // 
            btnBuildOIV.Location = new Point(0, 0);
            btnBuildOIV.Name = "btnBuildOIV";
            btnBuildOIV.Size = new Size(75, 23);
            btnBuildOIV.TabIndex = 0;
            // 
            // btnAddFiles
            // 
            btnAddFiles.Location = new Point(0, 0);
            btnAddFiles.Name = "btnAddFiles";
            btnAddFiles.Size = new Size(75, 23);
            btnAddFiles.TabIndex = 0;
            // 
            // webViewFileList
            // 
            webViewFileList.AllowExternalDrop = true;
            webViewFileList.CreationProperties = null;
            webViewFileList.DefaultBackgroundColor = Color.White;
            webViewFileList.Location = new Point(0, 0);
            webViewFileList.Name = "webViewFileList";
            webViewFileList.Size = new Size(0, 0);
            webViewFileList.TabIndex = 0;
            webViewFileList.ZoomFactor = 1D;
            // 
            // main
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(13, 13, 13);
            ClientSize = new Size(1030, 660);
            ControlBox = false;
            Controls.Add(panelRight);
            Controls.Add(panelEditorRight);
            Controls.Add(panelLeft);
            Controls.Add(panelSidebar);
            Controls.Add(panelDrag);
            Font = new Font("Syne", 9F);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MagicOGK OIV Builder";
            panelDrag.ResumeLayout(false);
            panelLeft.ResumeLayout(false);
            panelSidebar.ResumeLayout(false);
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
            btn.Size      = new Size(200, 50);
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
/*
            txtDescription.BackColor       = Color.Black;
            txtDescription.ForeColor       = Color.FromArgb(200, 200, 200);
            txtDescription.BorderStyle     = BorderStyle.FixedSingle;
            txtDescription.PlaceholderText = "Write a description for your mod...";
            txtDescription.Multiline       = true;
            txtDescription.Size            = new Size(300, 75);
            txtDescription.Location        = new Point(30, 234);
            txtDescription.Font            = new Font("Syne", 9F);
            txtDescription.ScrollBars      = ScrollBars.None;
*/
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
                Dock = DockStyle.Top,
                Height = 350,
                Padding = new Padding(30, 20, 30, 10)
            };

            // INSTALL OIV label
            Label lblInstallOiv = new Label
            {
                Text = "OIV INSTALLER",
                ForeColor = Color.FromArgb(255, 170, 170),
                Font = new Font("Syne", 11F, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true
            };

            Label lblInstallHint = new Label
            {
                Text = "Install a built .oiv package directly to your GTA V folder",
                ForeColor = Color.FromArgb(130, 80, 80),
                Font = new Font("Syne", 8F),
                Location = new Point(30, 50),
                AutoSize = true
            };

            txtGta5Path = new TextBox
            {
                Text = "",
                PlaceholderText = "Please select your GTA5 directory...",
                Location = new Point(30, 75),
                Size = new Size(460, 40),
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(220, 170, 170),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Cursor = Cursors.Hand,
                Font = new Font("Syne", 8F)
            };

            txtGta5Path.Click += BtnSelectGtaDirectory_Click;
            
            Button btnSelectGtaDirectory = new Button
            {
                Text = "SELECT DIR",
                Size = new Size(130, 30),
                Location = new Point(500, 73),
                BackColor = Color.FromArgb(120, 18, 24),
                ForeColor = Color.FromArgb(255, 185, 185),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 8F),
                Cursor = Cursors.Hand
            };

            btnSelectGtaDirectory.FlatAppearance.BorderSize = 0;

            btnSelectGtaDirectory.Click += BtnSelectGtaDirectory_Click;

            lblGta5PathStatus = new Label
            {
                Text = "Please select your GTA5 directory",
                Location = new Point(30, 98),
                AutoSize = true,
                ForeColor = Color.FromArgb(220, 70, 70),
                Font = new Font("Syne", 8F, FontStyle.Bold)
            };

            Button btnInstallOiv = new Button
            {
                Text = "INSTALL OIV",
                Size = new Size(130, 30),
                Location = new Point(500, 35),
                BackColor = Color.FromArgb(120, 18, 24),
                ForeColor = Color.FromArgb(255, 185, 185),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Syne", 8F),
                Cursor = Cursors.Hand
            };

            btnInstallOiv.FlatAppearance.BorderSize = 0;
            btnInstallOiv.Click += BtnInstallOiv_Click;

            // PACKAGE FILES label
            lblPackageFiles.Text = "PACKAGE FILES";
            lblPackageFiles.ForeColor = Color.FromArgb(188, 143, 143);
            lblPackageFiles.Font = new Font("Syne", 11F, FontStyle.Bold);
            lblPackageFiles.Location = new Point(30, 125);
            lblPackageFiles.AutoSize = true;

            // Add Files button
            btnAddFiles.BackColor = Color.FromArgb(139, 30, 30);
            btnAddFiles.ForeColor = Color.FromArgb(240, 192, 192);
            btnAddFiles.FlatStyle = FlatStyle.Flat;
            btnAddFiles.FlatAppearance.BorderColor = Color.FromArgb(139, 58, 58);
            btnAddFiles.Text = "+ Add Files";
            btnAddFiles.Size = new Size(105, 30);
            btnAddFiles.Location = new Point(30, 160);
            btnAddFiles.Font = new Font("Syne", 9F);

            lblAddFilesHint.Text = "or drag & drop files below";
            lblAddFilesHint.ForeColor = Color.FromArgb(90, 90, 90);
            lblAddFilesHint.AutoSize = true;
            lblAddFilesHint.Location = new Point(145, 168);
            lblAddFilesHint.Font = new Font("Syne", 8F);

            // Drop zone
            panelDropZone.BackColor = Color.FromArgb(17, 17, 17);
            panelDropZone.BorderStyle = BorderStyle.FixedSingle;
            panelDropZone.Location = new Point(30, 205);
            panelDropZone.Size = new Size(600, 120);
            panelDropZone.AllowDrop = true;

            lblNoFiles.Text = "Drop mod files here\r\nSupports .yft .ytd .meta .xml .asi .dlc and more";
            lblNoFiles.ForeColor = Color.FromArgb(90, 90, 90);
            lblNoFiles.TextAlign = ContentAlignment.MiddleCenter;
            lblNoFiles.Dock = DockStyle.Fill;
            lblNoFiles.Font = new Font("Syne", 9F);

            panelDropZone.Controls.Clear();
            panelDropZone.Controls.Add(lblNoFiles);

            panelRightTop.Controls.AddRange(new Control[]
            {
                lblInstallOiv,
                lblInstallHint,
                txtGta5Path,
                btnSelectGtaDirectory,
                lblGta5PathStatus,
                btnInstallOiv,
                lblPackageFiles,
                btnAddFiles,
                lblAddFilesHint,
                panelDropZone
            });

            // WebView fills everything BELOW panelRightTop
            webViewFileList.Dock = DockStyle.Fill;

            panelRight.Controls.Add(webViewFileList);
            panelRight.Controls.Add(panelRightTop);
        }

        // ── Field declarations ──────────────────────────────────────────────
        private BufferedPanel panelMarquee;
        private GalaxySidebarPanel panelMatrixTitle;
        private Panel       panelDrag;
        private Panel       panelLeft;
        private Panel       panelRight;
        private Panel       panelEditorRight;
        private Panel       panelSidebar;
        private Panel       panelDropZone;
        private Panel       panelPhotoPreview;
        private AnimatedGlowPanel panelColorPicker;

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
        // lblSidebarTitle removed  –  replaced by panelMatrixTitle

        private TextBox     txtAuthor;
        private TextBox     txtModName;
        private TextBox     txtVersion;
        private CustomScrollTextBox txtDescription;
        private CustomDropdown dropdownVersionTag;

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
        private Button      btnSidebarExtractOIV;
        private Button      btnSidebarSaveProjectAs;
        private Button      btnSidebarBuildOIV;
        private Button      btnCheckUpdates;
        private Button      btnSidebarFeedback;

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewFileList;

        private System.Windows.Forms.Timer sidebarTimer;
        private System.Windows.Forms.Timer editorTimer;
        private Button btnReplaceMods;
    }
}
