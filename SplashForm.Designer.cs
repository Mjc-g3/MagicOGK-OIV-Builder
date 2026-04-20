namespace MagicOGK_OIV_Builder
{
    partial class SplashForm
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
            webViewBackground = new Microsoft.Web.WebView2.WinForms.WebView2();
            timer1 = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)webViewBackground).BeginInit();
            SuspendLayout();
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
            webViewBackground.TabIndex = 21;
            webViewBackground.ZoomFactor = 1D;
            // 
            // timer1
            // 
            timer1.Interval = 25;
            // 
            // SplashForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1030, 660);
            Controls.Add(webViewBackground);
            FormBorderStyle = FormBorderStyle.None;
            Name = "SplashForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SplashForm";
            ((System.ComponentModel.ISupportInitialize)webViewBackground).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewBackground;
        private System.Windows.Forms.Timer timer1;
    }
}