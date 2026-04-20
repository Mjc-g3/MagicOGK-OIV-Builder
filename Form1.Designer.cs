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
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            timerHover = new System.Windows.Forms.Timer(components);
            webViewBackground = new Microsoft.Web.WebView2.WinForms.WebView2();
            panelDrag = new Panel();
            AnimationTimer = new System.Windows.Forms.Timer(components);
            panelSidebar = new Panel();
            btnSidebarFeedback = new Button();
            btnSidebarBuildOIV = new Button();
            btnSidebarSaveProjectAs = new Button();
            btnSidebarOpenOIV = new Button();
            btnSidebarOpenProject = new Button();
            lblSidebarTitle = new Label();
            btnSidebarToggle = new Button();
            sidebarTimer = new System.Windows.Forms.Timer(components);
            sidebarTextTimer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)webViewBackground).BeginInit();
            panelSidebar.SuspendLayout();
            SuspendLayout();
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
            // timerHover
            // 
            timerHover.Interval = 15;
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
            // panelSidebar
            // 
            panelSidebar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panelSidebar.Controls.Add(btnSidebarFeedback);
            panelSidebar.Controls.Add(btnSidebarBuildOIV);
            panelSidebar.Controls.Add(btnSidebarSaveProjectAs);
            panelSidebar.Controls.Add(btnSidebarOpenOIV);
            panelSidebar.Controls.Add(btnSidebarOpenProject);
            panelSidebar.Controls.Add(lblSidebarTitle);
            panelSidebar.Location = new Point(0, 22);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(0, 638);
            panelSidebar.TabIndex = 22;
            // 
            // btnSidebarFeedback
            // 
            btnSidebarFeedback.FlatAppearance.BorderColor = Color.FromArgb(64, 0, 0);
            btnSidebarFeedback.FlatStyle = FlatStyle.Flat;
            btnSidebarFeedback.Font = new Font("Syne SemiBold", 9.749999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSidebarFeedback.ForeColor = Color.White;
            btnSidebarFeedback.Location = new Point(15, 412);
            btnSidebarFeedback.Name = "btnSidebarFeedback";
            btnSidebarFeedback.Size = new Size(200, 50);
            btnSidebarFeedback.TabIndex = 5;
            btnSidebarFeedback.Text = "Feedback";
            btnSidebarFeedback.UseVisualStyleBackColor = true;
            // 
            // btnSidebarBuildOIV
            // 
            btnSidebarBuildOIV.FlatAppearance.BorderColor = Color.FromArgb(64, 0, 0);
            btnSidebarBuildOIV.FlatStyle = FlatStyle.Flat;
            btnSidebarBuildOIV.Font = new Font("Syne SemiBold", 9.749999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSidebarBuildOIV.ForeColor = Color.White;
            btnSidebarBuildOIV.Location = new Point(15, 339);
            btnSidebarBuildOIV.Name = "btnSidebarBuildOIV";
            btnSidebarBuildOIV.Size = new Size(200, 50);
            btnSidebarBuildOIV.TabIndex = 4;
            btnSidebarBuildOIV.Text = "Build OIV";
            btnSidebarBuildOIV.UseVisualStyleBackColor = true;
            // 
            // btnSidebarSaveProjectAs
            // 
            btnSidebarSaveProjectAs.FlatAppearance.BorderColor = Color.FromArgb(64, 0, 0);
            btnSidebarSaveProjectAs.FlatStyle = FlatStyle.Flat;
            btnSidebarSaveProjectAs.Font = new Font("Syne SemiBold", 9.749999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSidebarSaveProjectAs.ForeColor = Color.White;
            btnSidebarSaveProjectAs.Location = new Point(15, 265);
            btnSidebarSaveProjectAs.Name = "btnSidebarSaveProjectAs";
            btnSidebarSaveProjectAs.Size = new Size(200, 50);
            btnSidebarSaveProjectAs.TabIndex = 3;
            btnSidebarSaveProjectAs.Text = "Save Project As";
            btnSidebarSaveProjectAs.UseVisualStyleBackColor = true;
            // 
            // btnSidebarOpenOIV
            // 
            btnSidebarOpenOIV.FlatAppearance.BorderColor = Color.FromArgb(64, 0, 0);
            btnSidebarOpenOIV.FlatStyle = FlatStyle.Flat;
            btnSidebarOpenOIV.Font = new Font("Syne SemiBold", 9.749999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSidebarOpenOIV.ForeColor = Color.White;
            btnSidebarOpenOIV.Location = new Point(15, 195);
            btnSidebarOpenOIV.Name = "btnSidebarOpenOIV";
            btnSidebarOpenOIV.Size = new Size(200, 50);
            btnSidebarOpenOIV.TabIndex = 2;
            btnSidebarOpenOIV.Text = "Open OIV";
            btnSidebarOpenOIV.UseVisualStyleBackColor = true;
            // 
            // btnSidebarOpenProject
            // 
            btnSidebarOpenProject.FlatAppearance.BorderColor = Color.FromArgb(64, 0, 0);
            btnSidebarOpenProject.FlatStyle = FlatStyle.Flat;
            btnSidebarOpenProject.Font = new Font("Syne SemiBold", 9.749999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSidebarOpenProject.ForeColor = Color.White;
            btnSidebarOpenProject.Location = new Point(15, 128);
            btnSidebarOpenProject.Name = "btnSidebarOpenProject";
            btnSidebarOpenProject.Size = new Size(200, 50);
            btnSidebarOpenProject.TabIndex = 1;
            btnSidebarOpenProject.Text = "Open Project";
            btnSidebarOpenProject.UseVisualStyleBackColor = true;
            // 
            // lblSidebarTitle
            // 
            lblSidebarTitle.BackColor = Color.Transparent;
            lblSidebarTitle.ForeColor = Color.RosyBrown;
            lblSidebarTitle.Location = new Point(15, 20);
            lblSidebarTitle.Name = "lblSidebarTitle";
            lblSidebarTitle.Size = new Size(220, 50);
            lblSidebarTitle.TabIndex = 0;
            lblSidebarTitle.Text = "placeholder text";
            lblSidebarTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnSidebarToggle
            // 
            btnSidebarToggle.BackColor = Color.FromArgb(64, 0, 0);
            btnSidebarToggle.FlatAppearance.BorderSize = 0;
            btnSidebarToggle.FlatStyle = FlatStyle.Flat;
            btnSidebarToggle.Location = new Point(0, 0);
            btnSidebarToggle.Name = "btnSidebarToggle";
            btnSidebarToggle.Size = new Size(34, 34);
            btnSidebarToggle.TabIndex = 0;
            btnSidebarToggle.UseVisualStyleBackColor = false;
            // 
            // sidebarTimer
            // 
            sidebarTimer.Interval = 10;
            // 
            // sidebarTextTimer
            // 
            sidebarTextTimer.Interval = 60;
            // 
            // main
            // 
            AutoScaleDimensions = new SizeF(7F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1030, 660);
            Controls.Add(btnSidebarToggle);
            Controls.Add(panelSidebar);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(panelDrag);
            Controls.Add(webViewBackground);
            Font = new Font("Syne", 9.749999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 4, 3, 4);
            Name = "main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MagicOGK OIV Builer";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)webViewBackground).EndInit();
            panelSidebar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button button5;
        private Button button6;
        private Button button7;
        private System.Windows.Forms.Timer timerHover;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewBackground;
        private Panel panelDrag;
        private System.Windows.Forms.Timer AnimationTimer;
        private Panel panelSidebar;
        private Label lblSidebarTitle;
        private Button btnSidebarToggle;
        private Button btnSidebarFeedback;
        private Button btnSidebarBuildOIV;
        private Button btnSidebarSaveProjectAs;
        private Button btnSidebarOpenOIV;
        private Button btnSidebarOpenProject;
        private System.Windows.Forms.Timer sidebarTimer;
        private System.Windows.Forms.Timer sidebarTextTimer;
    }
}
