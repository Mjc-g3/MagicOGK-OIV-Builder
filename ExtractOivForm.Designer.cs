namespace MagicOGK_OIV_Builder
{
    partial class ExtractOivForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            borderPanel = new Panel();
            mainPanel = new Panel();
            contentPanel = new Panel();
            centerPanel = new Panel();
            txtLog = new CustomScrollTextBox();
            lblLog = new Label();
            btnExtract = new AnimatedGlowButton();
            dragDropPanel = new Panel();
            lblDragDropHint = new Label();
            rbFlat = new RadioButton();
            rbNested = new RadioButton();
            btnBrowseOutput = new AnimatedGlowButton();
            txtOutputPath = new TextBox();
            lblOutput = new Label();
            panelTitleBar = new Panel();
            lblTitle = new Label();
            btnMinimize = new Button();
            btnClose = new Button();
            toolTip = new ToolTip(components);
            chkNestedFolders = new CheckBox();
            borderPanel.SuspendLayout();
            mainPanel.SuspendLayout();
            contentPanel.SuspendLayout();
            centerPanel.SuspendLayout();
            dragDropPanel.SuspendLayout();
            panelTitleBar.SuspendLayout();
            SuspendLayout();
            //
            // borderPanel
            //
            borderPanel.BackColor = Color.FromArgb(180, 70, 70);
            borderPanel.Controls.Add(mainPanel);
            borderPanel.Dock = DockStyle.Fill;
            borderPanel.Location = new Point(0, 0);
            borderPanel.Name = "borderPanel";
            borderPanel.Padding = new Padding(1);
            borderPanel.Size = new Size(584, 712);
            borderPanel.TabIndex = 0;
            //
            // mainPanel
            //
            mainPanel.BackColor = Color.FromArgb(16, 16, 16);
            mainPanel.Controls.Add(contentPanel);
            mainPanel.Controls.Add(panelTitleBar);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(1, 1);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(582, 710);
            mainPanel.TabIndex = 0;
            //
            // contentPanel
            //
            contentPanel.Controls.Add(centerPanel);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 32);
            contentPanel.Name = "contentPanel";
            contentPanel.Padding = new Padding(24);
            contentPanel.Size = new Size(582, 678);
            contentPanel.TabIndex = 1;
            //
            // centerPanel
            //
            centerPanel.Anchor = AnchorStyles.None;
            centerPanel.Controls.Add(txtLog);
            centerPanel.Controls.Add(lblLog);
            centerPanel.Controls.Add(btnExtract);
            centerPanel.Controls.Add(dragDropPanel);
            centerPanel.Controls.Add(rbFlat);
            centerPanel.Controls.Add(rbNested);
            centerPanel.Controls.Add(btnBrowseOutput);
            centerPanel.Controls.Add(txtOutputPath);
            centerPanel.Controls.Add(lblOutput);
            centerPanel.Location = new Point(22, 6);
            centerPanel.Name = "centerPanel";
            centerPanel.Size = new Size(540, 640);
            centerPanel.TabIndex = 0;
            //
            // txtLog
            //
            txtLog.BackColor = Color.FromArgb(35, 35, 35);
            txtLog.Font = new Font("Consolas", 8.5F);
            txtLog.Location = new Point(0, 350);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(540, 295);
            txtLog.TabIndex = 8;
            //
            // lblLog
            //
            lblLog.AutoSize = true;
            lblLog.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold);
            lblLog.ForeColor = Color.FromArgb(210, 150, 150);
            lblLog.Location = new Point(0, 325);
            lblLog.Name = "lblLog";
            lblLog.Size = new Size(32, 13);
            lblLog.TabIndex = 7;
            lblLog.Text = "LOG";
            //
            // btnExtract
            //
            btnExtract.BackColor = Color.FromArgb(92, 0, 0);
            btnExtract.FlatStyle = FlatStyle.Flat;
            btnExtract.ForeColor = Color.FromArgb(235, 165, 165);
            btnExtract.Location = new Point(0, 270);
            btnExtract.Name = "btnExtract";
            btnExtract.Size = new Size(540, 40);
            btnExtract.TabIndex = 6;
            btnExtract.Text = "EXTRACT OIV";
            btnExtract.UseVisualStyleBackColor = false;
            btnExtract.Click += btnExtract_Click;
            //
            // dragDropPanel
            //
            dragDropPanel.AllowDrop = true;
            dragDropPanel.BackColor = Color.FromArgb(18, 18, 18);
            dragDropPanel.BorderStyle = BorderStyle.FixedSingle;
            dragDropPanel.Controls.Add(lblDragDropHint);
            dragDropPanel.Cursor = Cursors.Hand;
            dragDropPanel.Location = new Point(0, 105);
            dragDropPanel.Name = "dragDropPanel";
            dragDropPanel.Size = new Size(540, 150);
            dragDropPanel.TabIndex = 5;
            dragDropPanel.DragDrop += dragDropPanel_DragDrop;
            dragDropPanel.DragEnter += dragDropPanel_DragEnter;
            //
            // lblDragDropHint
            //
            lblDragDropHint.BackColor = Color.Transparent;
            lblDragDropHint.Dock = DockStyle.Fill;
            lblDragDropHint.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            lblDragDropHint.ForeColor = Color.FromArgb(170, 120, 120);
            lblDragDropHint.Location = new Point(0, 0);
            lblDragDropHint.Name = "lblDragDropHint";
            lblDragDropHint.Size = new Size(538, 148);
            lblDragDropHint.TabIndex = 0;
            lblDragDropHint.Text = "Drag && Drop OIV Package Here";
            lblDragDropHint.TextAlign = ContentAlignment.MiddleCenter;
            //
            // rbFlat
            //
            rbFlat.AutoSize = true;
            rbFlat.BackColor = Color.Transparent;
            rbFlat.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            rbFlat.ForeColor = Color.FromArgb(210, 150, 150);
            rbFlat.Location = new Point(80, 72);
            rbFlat.Name = "rbFlat";
            rbFlat.Size = new Size(57, 19);
            rbFlat.TabIndex = 4;
            rbFlat.Text = "FLAT";
            toolTip.SetToolTip(rbFlat, "Creates one folder per install path");
            rbFlat.UseVisualStyleBackColor = false;
            rbFlat.CheckedChanged += rbFlat_CheckedChanged;
            //
            // rbNested
            //
            rbNested.AutoSize = true;
            rbNested.BackColor = Color.Transparent;
            rbNested.Checked = true;
            rbNested.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            rbNested.ForeColor = Color.FromArgb(210, 150, 150);
            rbNested.Location = new Point(0, 72);
            rbNested.Name = "rbNested";
            rbNested.Size = new Size(80, 19);
            rbNested.TabIndex = 3;
            rbNested.TabStop = true;
            rbNested.Text = "NESTED";
            toolTip.SetToolTip(rbNested, "Keeps the real folder structure");
            rbNested.UseVisualStyleBackColor = false;
            rbNested.CheckedChanged += rbNested_CheckedChanged;
            //
            // btnBrowseOutput
            //
            btnBrowseOutput.BackColor = Color.FromArgb(92, 0, 0);
            btnBrowseOutput.FlatStyle = FlatStyle.Flat;
            btnBrowseOutput.ForeColor = Color.FromArgb(235, 165, 165);
            btnBrowseOutput.Location = new Point(425, 26);
            btnBrowseOutput.Name = "btnBrowseOutput";
            btnBrowseOutput.Size = new Size(115, 32);
            btnBrowseOutput.TabIndex = 2;
            btnBrowseOutput.Text = "BROWSE";
            btnBrowseOutput.UseVisualStyleBackColor = false;
            btnBrowseOutput.Click += btnBrowseOutput_Click;
            //
            // txtOutputPath
            //
            txtOutputPath.AllowDrop = true;
            txtOutputPath.BackColor = Color.Black;
            txtOutputPath.BorderStyle = BorderStyle.FixedSingle;
            txtOutputPath.Font = new Font("Segoe UI", 9F);
            txtOutputPath.ForeColor = Color.FromArgb(200, 200, 200);
            txtOutputPath.Location = new Point(0, 28);
            txtOutputPath.Name = "txtOutputPath";
            txtOutputPath.Size = new Size(420, 23);
            txtOutputPath.TabIndex = 1;
            txtOutputPath.DragDrop += txtOutputPath_DragDrop;
            txtOutputPath.DragEnter += txtOutputPath_DragEnter;
            //
            // lblOutput
            //
            lblOutput.AutoSize = true;
            lblOutput.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold);
            lblOutput.ForeColor = Color.FromArgb(210, 150, 150);
            lblOutput.Location = new Point(0, 0);
            lblOutput.Name = "lblOutput";
            lblOutput.Size = new Size(134, 13);
            lblOutput.TabIndex = 0;
            lblOutput.Text = "OUTPUT DIRECTORY";
            //
            // panelTitleBar
            //
            panelTitleBar.BackColor = Color.FromArgb(10, 10, 10);
            panelTitleBar.Controls.Add(lblTitle);
            panelTitleBar.Controls.Add(btnMinimize);
            panelTitleBar.Controls.Add(btnClose);
            panelTitleBar.Dock = DockStyle.Top;
            panelTitleBar.Location = new Point(0, 0);
            panelTitleBar.Name = "panelTitleBar";
            panelTitleBar.Size = new Size(582, 32);
            panelTitleBar.TabIndex = 0;
            panelTitleBar.MouseDown += panelTitleBar_MouseDown;
            panelTitleBar.MouseMove += panelTitleBar_MouseMove;
            panelTitleBar.MouseUp += panelTitleBar_MouseUp;
            //
            // lblTitle
            //
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(210, 150, 150);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(12, 0, 0, 0);
            lblTitle.Size = new Size(492, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "EXTRACT OIV";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.MouseDown += lblTitle_MouseDown;
            lblTitle.MouseMove += lblTitle_MouseMove;
            lblTitle.MouseUp += lblTitle_MouseUp;
            //
            // btnMinimize
            //
            btnMinimize.BackColor = Color.Transparent;
            btnMinimize.Dock = DockStyle.Right;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.Font = new Font("Segoe UI", 12F);
            btnMinimize.ForeColor = Color.FromArgb(210, 150, 150);
            btnMinimize.Location = new Point(492, 0);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(45, 32);
            btnMinimize.TabIndex = 2;
            btnMinimize.Text = "−";
            btnMinimize.UseVisualStyleBackColor = false;
            btnMinimize.Click += btnMinimize_Click;
            btnMinimize.MouseEnter += btnMinimize_MouseEnter;
            btnMinimize.MouseLeave += btnMinimize_MouseLeave;
            //
            // btnClose
            //
            btnClose.BackColor = Color.Transparent;
            btnClose.Dock = DockStyle.Right;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 14F);
            btnClose.ForeColor = Color.FromArgb(210, 150, 150);
            btnClose.Location = new Point(537, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(45, 32);
            btnClose.TabIndex = 1;
            btnClose.Text = "×";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            btnClose.MouseEnter += btnClose_MouseEnter;
            btnClose.MouseLeave += btnClose_MouseLeave;
            //
            // toolTip
            //
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            //
            // chkNestedFolders
            //
            chkNestedFolders.AutoSize = true;
            chkNestedFolders.Checked = true;
            chkNestedFolders.CheckState = CheckState.Checked;
            chkNestedFolders.Location = new Point(0, 0);
            chkNestedFolders.Name = "chkNestedFolders";
            chkNestedFolders.Size = new Size(15, 14);
            chkNestedFolders.TabIndex = 0;
            chkNestedFolders.UseVisualStyleBackColor = true;
            chkNestedFolders.Visible = false;
            //
            // ExtractOivForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(16, 16, 16);
            ClientSize = new Size(584, 712);
            ControlBox = false;
            Controls.Add(borderPanel);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExtractOivForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Extract OIV Package";
            borderPanel.ResumeLayout(false);
            mainPanel.ResumeLayout(false);
            contentPanel.ResumeLayout(false);
            centerPanel.ResumeLayout(false);
            centerPanel.PerformLayout();
            dragDropPanel.ResumeLayout(false);
            panelTitleBar.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel borderPanel;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Panel centerPanel;
        private CustomScrollTextBox txtLog;
        private System.Windows.Forms.Label lblLog;
        private AnimatedGlowButton btnExtract;
        private System.Windows.Forms.Panel dragDropPanel;
        private System.Windows.Forms.Label lblDragDropHint;
        private System.Windows.Forms.RadioButton rbFlat;
        private System.Windows.Forms.RadioButton rbNested;
        private AnimatedGlowButton btnBrowseOutput;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox chkNestedFolders;
    }
}
