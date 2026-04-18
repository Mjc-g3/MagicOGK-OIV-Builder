namespace MagicOGK_OIV_Builder
{
    partial class main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btnPackage = new Button();
            btnDLCPacks = new Button();
            btnReplaceV = new Button();
            btnXML = new Button();
            label1 = new Label();
            txtboxModName = new TextBox();
            label2 = new Label();
            txtboxAuthor = new TextBox();
            label3 = new Label();
            txtboxVersion = new TextBox();
            label4 = new Label();
            dropdownVersionTag = new ComboBox();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            label5 = new Label();
            label6 = new Label();
            dropdownOIVSpec = new ComboBox();
            panelSidebar = new Panel();
            btnBuildOIV = new Button();
            btnSaveAs = new Button();
            btnOpenOIV = new Button();
            btnSave = new Button();
            btnOpenP = new Button();
            btnShowSidebar = new Button();
            timerHover = new System.Windows.Forms.Timer(components);
            timerSidebar = new System.Windows.Forms.Timer(components);
            webViewBackground = new Microsoft.Web.WebView2.WinForms.WebView2();
            panelDrag = new Panel();
            AnimationTimer = new System.Windows.Forms.Timer(components);
            SidebarBtn1 = new Button();
            panelSidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webViewBackground).BeginInit();
            SuspendLayout();
            // 
            // btnPackage
            // 
            btnPackage.BackColor = Color.FromArgb(64, 0, 0);
            btnPackage.FlatAppearance.BorderColor = Color.FromArgb(64, 0, 0);
            btnPackage.FlatAppearance.BorderSize = 0;
            btnPackage.FlatStyle = FlatStyle.Flat;
            btnPackage.Font = new Font("Syne", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnPackage.ForeColor = Color.RosyBrown;
            btnPackage.Location = new Point(40, 70);
            btnPackage.Name = "btnPackage";
            btnPackage.Size = new Size(100, 40);
            btnPackage.TabIndex = 0;
            btnPackage.Text = "Package";
            btnPackage.UseVisualStyleBackColor = false;
            btnPackage.Click += button1_Click;
            btnPackage.MouseEnter += MenuButton_MouseEnter;
            btnPackage.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnDLCPacks
            // 
            btnDLCPacks.BackColor = Color.FromArgb(64, 0, 0);
            btnDLCPacks.FlatAppearance.BorderColor = Color.FromArgb(64, 0, 0);
            btnDLCPacks.FlatAppearance.BorderSize = 0;
            btnDLCPacks.FlatStyle = FlatStyle.Flat;
            btnDLCPacks.Font = new Font("Syne", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnDLCPacks.ForeColor = Color.RosyBrown;
            btnDLCPacks.Location = new Point(180, 70);
            btnDLCPacks.Name = "btnDLCPacks";
            btnDLCPacks.Size = new Size(100, 40);
            btnDLCPacks.TabIndex = 1;
            btnDLCPacks.Text = "DLC Packs";
            btnDLCPacks.UseVisualStyleBackColor = false;
            btnDLCPacks.MouseEnter += MenuButton_MouseEnter;
            btnDLCPacks.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnReplaceV
            // 
            btnReplaceV.BackColor = Color.FromArgb(64, 0, 0);
            btnReplaceV.FlatAppearance.BorderColor = Color.FromArgb(64, 0, 0);
            btnReplaceV.FlatAppearance.BorderSize = 0;
            btnReplaceV.FlatStyle = FlatStyle.Flat;
            btnReplaceV.Font = new Font("Syne", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnReplaceV.ForeColor = Color.RosyBrown;
            btnReplaceV.Location = new Point(320, 70);
            btnReplaceV.Name = "btnReplaceV";
            btnReplaceV.Size = new Size(100, 40);
            btnReplaceV.TabIndex = 2;
            btnReplaceV.Text = "Replace Vehicles";
            btnReplaceV.UseVisualStyleBackColor = false;
            btnReplaceV.MouseEnter += MenuButton_MouseEnter;
            btnReplaceV.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnXML
            // 
            btnXML.BackColor = Color.FromArgb(64, 0, 0);
            btnXML.FlatAppearance.BorderColor = Color.FromArgb(64, 0, 0);
            btnXML.FlatAppearance.BorderSize = 0;
            btnXML.FlatStyle = FlatStyle.Flat;
            btnXML.Font = new Font("Syne", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnXML.ForeColor = Color.RosyBrown;
            btnXML.Location = new Point(460, 70);
            btnXML.Name = "btnXML";
            btnXML.Size = new Size(100, 40);
            btnXML.TabIndex = 3;
            btnXML.Text = "XLM";
            btnXML.UseVisualStyleBackColor = false;
            btnXML.MouseEnter += MenuButton_MouseEnter;
            btnXML.MouseLeave += MenuButton_MouseLeave;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Syne", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.RosyBrown;
            label1.Location = new Point(40, 140);
            label1.Name = "label1";
            label1.Size = new Size(70, 20);
            label1.TabIndex = 4;
            label1.Text = "Metadata";
            // 
            // txtboxModName
            // 
            txtboxModName.BackColor = Color.Black;
            txtboxModName.BorderStyle = BorderStyle.FixedSingle;
            txtboxModName.ForeColor = Color.White;
            txtboxModName.Location = new Point(50, 190);
            txtboxModName.Name = "txtboxModName";
            txtboxModName.Size = new Size(260, 23);
            txtboxModName.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = Color.RosyBrown;
            label2.Location = new Point(50, 169);
            label2.Name = "label2";
            label2.Size = new Size(72, 18);
            label2.TabIndex = 6;
            label2.Text = "Mod Name";
            // 
            // txtboxAuthor
            // 
            txtboxAuthor.BackColor = Color.Black;
            txtboxAuthor.BorderStyle = BorderStyle.FixedSingle;
            txtboxAuthor.ForeColor = Color.White;
            txtboxAuthor.Location = new Point(320, 190);
            txtboxAuthor.Name = "txtboxAuthor";
            txtboxAuthor.Size = new Size(260, 23);
            txtboxAuthor.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = Color.RosyBrown;
            label3.Location = new Point(320, 169);
            label3.Name = "label3";
            label3.Size = new Size(46, 18);
            label3.TabIndex = 8;
            label3.Text = "Author";
            // 
            // txtboxVersion
            // 
            txtboxVersion.BackColor = Color.Black;
            txtboxVersion.BorderStyle = BorderStyle.FixedSingle;
            txtboxVersion.ForeColor = Color.White;
            txtboxVersion.Location = new Point(590, 190);
            txtboxVersion.Name = "txtboxVersion";
            txtboxVersion.Size = new Size(120, 23);
            txtboxVersion.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = Color.RosyBrown;
            label4.Location = new Point(590, 169);
            label4.Name = "label4";
            label4.Size = new Size(49, 18);
            label4.TabIndex = 10;
            label4.Text = "Version";
            // 
            // dropdownVersionTag
            // 
            dropdownVersionTag.BackColor = Color.Black;
            dropdownVersionTag.DropDownStyle = ComboBoxStyle.DropDownList;
            dropdownVersionTag.FlatStyle = FlatStyle.Flat;
            dropdownVersionTag.Font = new Font("Syne", 9F);
            dropdownVersionTag.ForeColor = Color.White;
            dropdownVersionTag.FormattingEnabled = true;
            dropdownVersionTag.Items.AddRange(new object[] { "Test", "Alpha", "Beta", "Stable" });
            dropdownVersionTag.Location = new Point(720, 189);
            dropdownVersionTag.Name = "dropdownVersionTag";
            dropdownVersionTag.Size = new Size(120, 25);
            dropdownVersionTag.TabIndex = 11;
            dropdownVersionTag.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // button5
            // 
            button5.BackColor = Color.IndianRed;
            button5.FlatAppearance.BorderColor = Color.DimGray;
            button5.FlatAppearance.BorderSize = 2;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Location = new Point(1011, 7);
            button5.Name = "button5";
            button5.Size = new Size(10, 10);
            button5.TabIndex = 12;
            button5.Text = "button5";
            button5.UseVisualStyleBackColor = false;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.BackColor = Color.Khaki;
            button6.FlatAppearance.BorderColor = Color.DimGray;
            button6.FlatAppearance.BorderSize = 2;
            button6.FlatStyle = FlatStyle.Flat;
            button6.Location = new Point(995, 7);
            button6.Name = "button6";
            button6.Size = new Size(10, 10);
            button6.TabIndex = 13;
            button6.Text = "button6";
            button6.UseVisualStyleBackColor = false;
            button6.Click += button6_Click;
            // 
            // button7
            // 
            button7.BackColor = Color.LightGreen;
            button7.FlatAppearance.BorderColor = Color.DimGray;
            button7.FlatAppearance.BorderSize = 2;
            button7.FlatStyle = FlatStyle.Flat;
            button7.Location = new Point(979, 7);
            button7.Name = "button7";
            button7.Size = new Size(10, 10);
            button7.TabIndex = 14;
            button7.Text = "button7";
            button7.UseVisualStyleBackColor = false;
            button7.Click += button7_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = Color.RosyBrown;
            label5.Location = new Point(720, 169);
            label5.Name = "label5";
            label5.Size = new Size(71, 18);
            label5.TabIndex = 15;
            label5.Text = "Version Tag";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = Color.RosyBrown;
            label6.Location = new Point(850, 169);
            label6.Name = "label6";
            label6.Size = new Size(61, 18);
            label6.TabIndex = 17;
            label6.Text = "OIV Spec";
            // 
            // dropdownOIVSpec
            // 
            dropdownOIVSpec.BackColor = Color.Black;
            dropdownOIVSpec.DropDownStyle = ComboBoxStyle.DropDownList;
            dropdownOIVSpec.FlatStyle = FlatStyle.Flat;
            dropdownOIVSpec.Font = new Font("Syne", 9F);
            dropdownOIVSpec.ForeColor = Color.White;
            dropdownOIVSpec.FormattingEnabled = true;
            dropdownOIVSpec.Items.AddRange(new object[] { "Test", "Alpha", "Beta", "Stable" });
            dropdownOIVSpec.Location = new Point(850, 189);
            dropdownOIVSpec.Name = "dropdownOIVSpec";
            dropdownOIVSpec.Size = new Size(120, 25);
            dropdownOIVSpec.TabIndex = 16;
            dropdownOIVSpec.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            // 
            // panelSidebar
            // 
            panelSidebar.BackColor = Color.Black;
            panelSidebar.Controls.Add(SidebarBtn1);
            panelSidebar.Controls.Add(btnBuildOIV);
            panelSidebar.Controls.Add(btnSaveAs);
            panelSidebar.Controls.Add(btnOpenOIV);
            panelSidebar.Controls.Add(btnSave);
            panelSidebar.Controls.Add(btnOpenP);
            panelSidebar.Location = new Point(-160, 0);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(200, 660);
            panelSidebar.TabIndex = 18;
            panelSidebar.Visible = false;
            // 
            // btnBuildOIV
            // 
            btnBuildOIV.FlatAppearance.BorderColor = Color.Black;
            btnBuildOIV.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 0, 0);
            btnBuildOIV.FlatStyle = FlatStyle.Flat;
            btnBuildOIV.ForeColor = Color.White;
            btnBuildOIV.Location = new Point(10, 442);
            btnBuildOIV.Margin = new Padding(0);
            btnBuildOIV.Name = "btnBuildOIV";
            btnBuildOIV.Size = new Size(180, 75);
            btnBuildOIV.TabIndex = 4;
            btnBuildOIV.Text = "Build OIV";
            btnBuildOIV.UseVisualStyleBackColor = true;
            // 
            // btnSaveAs
            // 
            btnSaveAs.FlatAppearance.BorderColor = Color.Black;
            btnSaveAs.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 0, 0);
            btnSaveAs.FlatStyle = FlatStyle.Flat;
            btnSaveAs.ForeColor = Color.White;
            btnSaveAs.Location = new Point(10, 292);
            btnSaveAs.Margin = new Padding(0);
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.Size = new Size(180, 75);
            btnSaveAs.TabIndex = 3;
            btnSaveAs.Text = "Save Project As";
            btnSaveAs.UseVisualStyleBackColor = true;
            // 
            // btnOpenOIV
            // 
            btnOpenOIV.FlatAppearance.BorderColor = Color.Black;
            btnOpenOIV.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 0, 0);
            btnOpenOIV.FlatStyle = FlatStyle.Flat;
            btnOpenOIV.ForeColor = Color.White;
            btnOpenOIV.Location = new Point(10, 367);
            btnOpenOIV.Margin = new Padding(0);
            btnOpenOIV.Name = "btnOpenOIV";
            btnOpenOIV.Size = new Size(180, 75);
            btnOpenOIV.TabIndex = 2;
            btnOpenOIV.Text = "Open OIV";
            btnOpenOIV.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.FlatAppearance.BorderColor = Color.Black;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 0, 0);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(10, 217);
            btnSave.Margin = new Padding(0);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(180, 75);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save Project";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnOpenP
            // 
            btnOpenP.FlatAppearance.BorderColor = Color.Black;
            btnOpenP.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 0, 0);
            btnOpenP.FlatStyle = FlatStyle.Flat;
            btnOpenP.ForeColor = Color.White;
            btnOpenP.Location = new Point(10, 142);
            btnOpenP.Margin = new Padding(0);
            btnOpenP.Name = "btnOpenP";
            btnOpenP.Size = new Size(180, 75);
            btnOpenP.TabIndex = 0;
            btnOpenP.Text = "Open Project";
            btnOpenP.UseVisualStyleBackColor = true;
            // 
            // btnShowSidebar
            // 
            btnShowSidebar.BackColor = Color.Transparent;
            btnShowSidebar.FlatAppearance.BorderColor = Color.RosyBrown;
            btnShowSidebar.FlatAppearance.BorderSize = 0;
            btnShowSidebar.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnShowSidebar.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnShowSidebar.FlatStyle = FlatStyle.Flat;
            btnShowSidebar.ForeColor = Color.Transparent;
            btnShowSidebar.Image = Properties.Resources.Sidebar_open;
            btnShowSidebar.ImageAlign = ContentAlignment.TopLeft;
            btnShowSidebar.Location = new Point(46, 70);
            btnShowSidebar.Name = "btnShowSidebar";
            btnShowSidebar.Size = new Size(40, 40);
            btnShowSidebar.TabIndex = 19;
            btnShowSidebar.UseVisualStyleBackColor = false;
            btnShowSidebar.Click += btnShowSidebar_Click;
            // 
            // timerHover
            // 
            timerHover.Interval = 15;
            // 
            // timerSidebar
            // 
            timerSidebar.Interval = 10;
            timerSidebar.Tick += timerSidebar_Tick;
            // 
            // webViewBackground
            // 
            webViewBackground.AllowExternalDrop = true;
            webViewBackground.CreationProperties = null;
            webViewBackground.DefaultBackgroundColor = Color.White;
            webViewBackground.Dock = DockStyle.Fill;
            webViewBackground.Location = new Point(0, 0);
            webViewBackground.Name = "webViewBackground";
            webViewBackground.Size = new Size(1030, 660);
            webViewBackground.TabIndex = 20;
            webViewBackground.ZoomFactor = 1D;
            // 
            // panelDrag
            // 
            panelDrag.BackColor = Color.Transparent;
            panelDrag.Dock = DockStyle.Top;
            panelDrag.Location = new Point(0, 0);
            panelDrag.Name = "panelDrag";
            panelDrag.Size = new Size(1030, 22);
            panelDrag.TabIndex = 21;
            // 
            // SidebarBtn1
            // 
            SidebarBtn1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SidebarBtn1.FlatAppearance.BorderSize = 0;
            SidebarBtn1.FlatStyle = FlatStyle.Flat;
            SidebarBtn1.Image = Properties.Resources.Sidebar_open;
            SidebarBtn1.Location = new Point(170, 22);
            SidebarBtn1.Name = "SidebarBtn1";
            SidebarBtn1.Size = new Size(30, 30);
            SidebarBtn1.TabIndex = 5;
            SidebarBtn1.UseVisualStyleBackColor = true;
            SidebarBtn1.Click += SidebarBtn1_Click;
            // 
            // main
            // 
            AutoScaleDimensions = new SizeF(7F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1030, 660);
            Controls.Add(btnShowSidebar);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(panelDrag);
            Controls.Add(panelSidebar);
            Controls.Add(label6);
            Controls.Add(dropdownOIVSpec);
            Controls.Add(label5);
            Controls.Add(dropdownVersionTag);
            Controls.Add(label4);
            Controls.Add(txtboxVersion);
            Controls.Add(label3);
            Controls.Add(txtboxAuthor);
            Controls.Add(label2);
            Controls.Add(txtboxModName);
            Controls.Add(label1);
            Controls.Add(btnXML);
            Controls.Add(btnReplaceV);
            Controls.Add(btnDLCPacks);
            Controls.Add(btnPackage);
            Controls.Add(webViewBackground);
            Font = new Font("Syne", 9.749999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 4, 3, 4);
            Name = "main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MagicOGK OIV Builer";
            Load += Form1_Load;
            panelSidebar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webViewBackground).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnPackage;
        private Button btnDLCPacks;
        private Button btnReplaceV;
        private Button btnXML;
        private Label label1;
        private TextBox txtboxModName;
        private Label label2;
        private TextBox txtboxAuthor;
        private Label label3;
        private TextBox txtboxVersion;
        private Label label4;
        private ComboBox dropdownVersionTag;
        private Button button5;
        private Button button6;
        private Button button7;
        private Label label5;
        private Label label6;
        private ComboBox dropdownOIVSpec;
        private Panel panelSidebar;
        private Button btnOpenOIV;
        private Button btnSave;
        private Button btnOpenP;
        private Button btnBuildOIV;
        private Button btnSaveAs;
        private Button btnShowSidebar;
        private System.Windows.Forms.Timer timerHover;
        private System.Windows.Forms.Timer timerSidebar;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewBackground;
        private Panel panelDrag;
        private System.Windows.Forms.Timer AnimationTimer;
        private Button SidebarBtn1;
    }
}
