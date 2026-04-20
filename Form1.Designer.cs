namespace MagicOGK_OIV_Builder
{
    partial class main
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            panelDrag = new Panel();
            panelLeft = new Panel();
            panelRight = new Panel();
            panelEditorRight = new Panel();

            panelSidebar = new Panel();
            btnSidebarToggle = new Button();
            btnSidebarOpenProject = new Button();
            btnSidebarOpenOIV = new Button();
            btnSidebarSaveProjectAs = new Button();
            btnSidebarBuildOIV = new Button();
            btnSidebarFeedback = new Button();
            lblSidebarTitle = new Label();

            lblAuthor = new Label();
            txtAuthor = new TextBox();
            lblModName = new Label();
            txtModName = new TextBox();
            lblVersionTag = new Label();
            dropdownVersionTag = new ComboBox();
            lblVersion = new Label();
            txtVersion = new TextBox();
            lblDescription = new Label();
            txtDescription = new TextBox();
            lblPhoto = new Label();
            btnAddPhoto = new Button();
            panelColorPicker = new Panel();
            btnOpenEditor = new Button();
            btnBuildOIV = new Button();

            lblPackageFiles = new Label();
            btnAddFiles = new Button();
            lblAddFilesHint = new Label();
            panelDropZone = new Panel();
            lblNoFiles = new Label();
            webViewFileList = new Microsoft.Web.WebView2.WinForms.WebView2();

            sidebarTimer = new System.Windows.Forms.Timer(components);
            sidebarTextTimer = new System.Windows.Forms.Timer(components);
            editorTimer = new System.Windows.Forms.Timer(components);

            ((System.ComponentModel.ISupportInitialize)webViewFileList).BeginInit();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            panelSidebar.SuspendLayout();
            SuspendLayout();

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(13, 13, 13);
            this.ClientSize = new System.Drawing.Size(1030, 660);
            this.ControlBox = false;
            this.Font = new System.Drawing.Font("Syne", 9F);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "main";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "MagicOGK OIV Builder";

            panelDrag.BackColor = System.Drawing.Color.Transparent;
            panelDrag.Dock = DockStyle.Top;
            panelDrag.Height = 22;
            panelDrag.Name = "panelDrag";
            this.Controls.Add(panelDrag);

            button5.BackColor = System.Drawing.Color.IndianRed;
            button5.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            button5.FlatAppearance.BorderSize = 2;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Location = new System.Drawing.Point(1011, 7);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(20, 20);
            button5.TabIndex = 0;
            button5.Click += button5_Click;
            panelDrag.Controls.Add(button5);

            button6.BackColor = System.Drawing.Color.Khaki;
            button6.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            button6.FlatAppearance.BorderSize = 2;
            button6.FlatStyle = FlatStyle.Flat;
            button6.Location = new System.Drawing.Point(990, 7);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(20, 20);
            button6.TabIndex = 1;
            button6.Click += button6_Click;
            panelDrag.Controls.Add(button6);

            button7.BackColor = System.Drawing.Color.LimeGreen;
            button7.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            button7.FlatAppearance.BorderSize = 2;
            button7.FlatStyle = FlatStyle.Flat;
            button7.Location = new System.Drawing.Point(969, 7);
            button7.Name = "button7";
            button7.Size = new System.Drawing.Size(20, 20);
            button7.TabIndex = 2;
            button7.Click += button7_Click;
            panelDrag.Controls.Add(button7);

            panelLeft.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Width = 360;
            panelLeft.Name = "panelLeft";
            panelLeft.AutoScroll = true;
            this.Controls.Add(panelLeft);

            panelRight.BackColor = System.Drawing.Color.FromArgb(13, 13, 13);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Name = "panelRight";
            this.Controls.Add(panelRight);

            panelSidebar.BackColor = System.Drawing.Color.Black;
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Width = 0;
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Controls.Add(btnSidebarToggle);
            panelSidebar.Controls.Add(btnSidebarOpenProject);
            panelSidebar.Controls.Add(btnSidebarOpenOIV);
            panelSidebar.Controls.Add(btnSidebarSaveProjectAs);
            panelSidebar.Controls.Add(btnSidebarBuildOIV);
            panelSidebar.Controls.Add(btnSidebarFeedback);
            panelSidebar.Controls.Add(lblSidebarTitle);
            this.Controls.Add(panelSidebar);

            SetupLeftPanel();
            SetupRightPanel();
            SetupSidebarControls();
            SetupEditorPanel();

            ((System.ComponentModel.ISupportInitialize)webViewFileList).EndInit();
            panelLeft.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            panelSidebar.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void SetupLeftPanel()
        {
            lblAuthor = new Label
            {
                Text = "AUTHOR",
                ForeColor = System.Drawing.Color.RosyBrown,
                AutoSize = true,
                Font = new System.Drawing.Font("Syne", 10F, System.Drawing.FontStyle.Bold)
            };
            lblAuthor.Location = new System.Drawing.Point(30, 30);
            panelLeft.Controls.Add(lblAuthor);

            txtAuthor = new TextBox
            {
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Mod author's name...",
                Size = new System.Drawing.Size(300, 23)
            };
            txtAuthor.Location = new System.Drawing.Point(30, 55);
            panelLeft.Controls.Add(txtAuthor);

            lblModName = new Label
            {
                Text = "MOD NAME",
                ForeColor = System.Drawing.Color.RosyBrown,
                AutoSize = true,
                Font = new System.Drawing.Font("Syne", 10F, System.Drawing.FontStyle.Bold)
            };
            lblModName.Location = new System.Drawing.Point(30, 95);
            panelLeft.Controls.Add(lblModName);

            txtModName = new TextBox
            {
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Mod name...",
                Size = new System.Drawing.Size(300, 23)
            };
            txtModName.Location = new System.Drawing.Point(30, 120);
            panelLeft.Controls.Add(txtModName);

            lblVersionTag = new Label
            {
                Text = "VERSION TAG",
                ForeColor = System.Drawing.Color.RosyBrown,
                AutoSize = true,
                Font = new System.Drawing.Font("Syne", 10F, System.Drawing.FontStyle.Bold)
            };
            lblVersionTag.Location = new System.Drawing.Point(30, 160);
            panelLeft.Controls.Add(lblVersionTag);

            dropdownVersionTag = new ComboBox
            {
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Size = new System.Drawing.Size(140, 25)
            };
            dropdownVersionTag.Items.AddRange(new object[] { "Test", "Alpha", "Beta", "Stable" });
            dropdownVersionTag.SelectedIndex = 3;
            dropdownVersionTag.Location = new System.Drawing.Point(30, 185);
            panelLeft.Controls.Add(dropdownVersionTag);

            lblVersion = new Label
            {
                Text = "VERSION",
                ForeColor = System.Drawing.Color.RosyBrown,
                AutoSize = true,
                Font = new System.Drawing.Font("Syne", 10F, System.Drawing.FontStyle.Bold)
            };
            lblVersion.Location = new System.Drawing.Point(180, 160);
            panelLeft.Controls.Add(lblVersion);

            txtVersion = new TextBox
            {
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "1.0",
                Size = new System.Drawing.Size(150, 23)
            };
            txtVersion.Location = new System.Drawing.Point(180, 185);
            panelLeft.Controls.Add(txtVersion);

            lblDescription = new Label
            {
                Text = "DESCRIPTION",
                ForeColor = System.Drawing.Color.RosyBrown,
                AutoSize = true,
                Font = new System.Drawing.Font("Syne", 10F, System.Drawing.FontStyle.Bold)
            };
            lblDescription.Location = new System.Drawing.Point(30, 225);
            panelLeft.Controls.Add(lblDescription);

            txtDescription = new TextBox
            {
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Write a description for you mod...",
                Multiline = true,
                Size = new System.Drawing.Size(300, 80),
                ScrollBars = ScrollBars.Vertical
            };
            txtDescription.Location = new System.Drawing.Point(30, 250);
            panelLeft.Controls.Add(txtDescription);

            lblPhoto = new Label
            {
                Text = "PHOTO PREVIEW",
                ForeColor = System.Drawing.Color.RosyBrown,
                AutoSize = true,
                Font = new System.Drawing.Font("Syne", 10F, System.Drawing.FontStyle.Bold)
            };
            lblPhoto.Location = new System.Drawing.Point(30, 350);
            panelLeft.Controls.Add(lblPhoto);

            btnAddPhoto = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(100, 100, 100),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Text = "ADD PHOTO",
                Size = new System.Drawing.Size(140, 100),
                Location = new System.Drawing.Point(30, 375)
            };
            btnAddPhoto.Click += btnAddPhoto_Click;
            panelLeft.Controls.Add(btnAddPhoto);

            panelColorPicker = new Panel
            {
                BackColor = System.Drawing.Color.FromArgb(200, 0, 0),
                Size = new System.Drawing.Size(140, 100),
                Location = new System.Drawing.Point(180, 375),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelColorPicker.Click += panelColorPicker_Click;
            panelLeft.Controls.Add(panelColorPicker);

            btnOpenEditor = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(64, 0, 0),
                ForeColor = System.Drawing.Color.RosyBrown,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = new FlatButtonAppearance { BorderColor = System.Drawing.Color.FromArgb(139, 58, 58), BorderSize = 1 },
                Text = "Open Editor",
                Size = new System.Drawing.Size(300, 45),
                Location = new System.Drawing.Point(30, 490),
                Font = new System.Drawing.Font("Syne", 11F)
            };
            btnOpenEditor.Click += btnOpenEditor_Click;
            panelLeft.Controls.Add(btnOpenEditor);

            btnBuildOIV = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(64, 0, 0),
                ForeColor = System.Drawing.Color.RosyBrown,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = new FlatButtonAppearance { BorderColor = System.Drawing.Color.FromArgb(139, 58, 58), BorderSize = 1 },
                Text = "Build OIV",
                Size = new System.Drawing.Size(300, 45),
                Location = new System.Drawing.Point(30, 550),
                Font = new System.Drawing.Font("Syne", 11F)
            };
            btnBuildOIV.Click += btnBuildOIV_Click;
            panelLeft.Controls.Add(btnBuildOIV);
        }

        private void SetupRightPanel()
        {
            lblPackageFiles = new Label
            {
                Text = "PACKAGE FILES",
                ForeColor = System.Drawing.Color.FromArgb(188, 143, 143),
                AutoSize = true,
                Font = new System.Drawing.Font("Syne", 11F, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(30, 30)
            };
            panelRight.Controls.Add(lblPackageFiles);

            btnAddFiles = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(139, 30, 30),
                ForeColor = System.Drawing.Color.FromArgb(240, 192, 192),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = new FlatButtonAppearance { BorderColor = System.Drawing.Color.FromArgb(139, 58, 58), BorderSize = 1 },
                Text = "+ Add Files",
                Size = new System.Drawing.Size(100, 30),
                Location = new System.Drawing.Point(30, 55),
                Font = new System.Drawing.Font("Syne", 9F)
            };
            btnAddFiles.Click += btnAddFiles_Click;
            panelRight.Controls.Add(btnAddFiles);

            lblAddFilesHint = new Label
            {
                Text = "or drag & drop files below",
                ForeColor = System.Drawing.Color.FromArgb(100, 100, 100),
                AutoSize = true,
                Location = new System.Drawing.Point(140, 62),
                Font = new System.Drawing.Font("Syne", 8F)
            };
            panelRight.Controls.Add(lblAddFilesHint);

            panelDropZone = new Panel
            {
                BackColor = System.Drawing.Color.FromArgb(17, 17, 17),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new System.Drawing.Point(30, 100),
                Size = new System.Drawing.Size(430, 130),
                AllowDrop = true
            };
            panelDropZone.DragEnter += PanelDropZone_DragEnter;
            panelDropZone.DragDrop += PanelDropZone_DragDrop;
            panelRight.Controls.Add(panelDropZone);

            lblNoFiles = new Label
            {
                Text = "No files added yet\nAdd mod files to include in your OIV package",
                ForeColor = System.Drawing.Color.FromArgb(100, 100, 100),
                AutoSize = false,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Size = new System.Drawing.Size(430, 130),
                Location = new System.Drawing.Point(0, 0),
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Syne", 9F)
            };
            panelDropZone.Controls.Add(lblNoFiles);

            webViewFileList = new Microsoft.Web.WebView2.WinForms.WebView2
            {
                Location = new System.Drawing.Point(30, 250),
                Size = new System.Drawing.Size(430, 350),
                Dock = DockStyle.None
            };
            panelRight.Controls.Add(webViewFileList);
        }

        private void SetupSidebarControls()
        {
            btnSidebarToggle = new Button
            {
                BackColor = System.Drawing.Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = new FlatButtonAppearance { BorderSize = 0 },
                Size = new System.Drawing.Size(40, 40),
                Location = new System.Drawing.Point(0, 0),
                Text = ""
            };
            panelSidebar.Controls.Add(btnSidebarToggle);

            btnSidebarOpenProject = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(64, 0, 0),
                ForeColor = System.Drawing.Color.RosyBrown,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = new FlatButtonAppearance { BorderSize = 0 },
                Text = "Open Project",
                Size = new System.Drawing.Size(200, 45),
                Location = new System.Drawing.Point(0, 50),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            panelSidebar.Controls.Add(btnSidebarOpenProject);

            btnSidebarOpenOIV = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(64, 0, 0),
                ForeColor = System.Drawing.Color.RosyBrown,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = new FlatButtonAppearance { BorderSize = 0 },
                Text = "Open OIV",
                Size = new System.Drawing.Size(200, 45),
                Location = new System.Drawing.Point(0, 100),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            panelSidebar.Controls.Add(btnSidebarOpenOIV);

            btnSidebarSaveProjectAs = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(64, 0, 0),
                ForeColor = System.Drawing.Color.RosyBrown,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = new FlatButtonAppearance { BorderSize = 0 },
                Text = "Save Project As",
                Size = new System.Drawing.Size(200, 45),
                Location = new System.Drawing.Point(0, 150),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            panelSidebar.Controls.Add(btnSidebarSaveProjectAs);

            btnSidebarBuildOIV = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(64, 0, 0),
                ForeColor = System.Drawing.Color.RosyBrown,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = new FlatButtonAppearance { BorderSize = 0 },
                Text = "Build OIV",
                Size = new System.Drawing.Size(200, 45),
                Location = new System.Drawing.Point(0, 200),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            panelSidebar.Controls.Add(btnSidebarBuildOIV);

            btnSidebarFeedback = new Button
            {
                BackColor = System.Drawing.Color.FromArgb(64, 0, 0),
                ForeColor = System.Drawing.Color.RosyBrown,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = new FlatButtonAppearance { BorderSize = 0 },
                Text = "Feedback",
                Size = new System.Drawing.Size(200, 45),
                Location = new System.Drawing.Point(0, 250),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            panelSidebar.Controls.Add(btnSidebarFeedback);

            lblSidebarTitle = new Label
            {
                ForeColor = System.Drawing.Color.RosyBrown,
                Location = new System.Drawing.Point(10, 550),
                Size = new System.Drawing.Size(180, 100)
            };
            panelSidebar.Controls.Add(lblSidebarTitle);
        }

        private void SetupEditorPanel()
        {
            panelEditorRight = new Panel
            {
                BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
                Dock = DockStyle.Right,
                Width = 0,
                Name = "panelEditorRight"
            };
            this.Controls.Add(panelEditorRight);
        }

        private Panel panelLeft;
        private Panel panelRight;
        private Panel panelEditorRight;
        private Panel panelSidebar;
        private Panel panelDrag;
        private Panel panelDropZone;
        private Panel panelColorPicker;

        private Label lblAuthor;
        private Label lblModName;
        private Label lblVersionTag;
        private Label lblVersion;
        private Label lblDescription;
        private Label lblPhoto;
        private Label lblPackageFiles;
        private Label lblAddFilesHint;
        private Label lblNoFiles;
        private Label lblSidebarTitle;

        private TextBox txtAuthor;
        private TextBox txtModName;
        private TextBox txtVersion;
        private TextBox txtDescription;

        private ComboBox dropdownVersionTag;

        private Button button5;
        private Button button6;
        private Button button7;
        private Button btnAddPhoto;
        private Button btnOpenEditor;
        private Button btnBuildOIV;
        private Button btnAddFiles;
        private Button btnSidebarToggle;
        private Button btnSidebarOpenProject;
        private Button btnSidebarOpenOIV;
        private Button btnSidebarSaveProjectAs;
        private Button btnSidebarBuildOIV;
        private Button btnSidebarFeedback;

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewFileList;

        private System.Windows.Forms.Timer sidebarTimer;
        private System.Windows.Forms.Timer sidebarTextTimer;
        private System.Windows.Forms.Timer editorTimer;
    }
}
