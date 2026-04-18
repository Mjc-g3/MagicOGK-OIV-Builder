using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagicOGK_OIV_Builder
{
    public partial class main : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private Size smallSize = new Size(100, 40);
        private Size bigSize = new Size(140, 60);
        private int speed = 3;

        private System.Windows.Forms.Button currentButton = null;
        private bool isHovering = false;

        private bool sidebarOpen = false;
        private bool sidebarAnimating = false;
        private bool sidebarOpening = false;
        private int sidebarSpeed = 20;

        public main()
        {
            InitializeComponent();

            this.Shown += main_Shown;

            this.Opacity = 0;
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = true;
        }

        private async void main_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Activate();

            for (double i = 0; i <= 1; i += 0.05)
            {
                this.Opacity = i;
                await Task.Delay(15);
            }

            this.Opacity = 1;
        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0x112, 0xf012, 0);
            }
        }

        private void button5_Click(object sender, EventArgs e) //rød exit knapp
        {
            Application.Exit();
        }

        private void button6_Click(object sender, EventArgs e) //gul minimize knapp
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button7_Click(object sender, EventArgs e) //grønn maximize knapp
        {
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string valgt = dropdownVersionTag.SelectedItem?.ToString();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string valgt = dropdownOIVSpec.SelectedItem?.ToString();
        }



        private async void Form1_Load(object sender, EventArgs e)
        {
            dropdownVersionTag.Items.Clear();
            dropdownOIVSpec.Items.Clear();

            dropdownVersionTag.Items.Add("Test");
            dropdownVersionTag.Items.Add("Alpha");
            dropdownVersionTag.Items.Add("Beta");
            dropdownVersionTag.Items.Add("Stable");

            dropdownOIVSpec.Items.Add("2.2");

            dropdownVersionTag.SelectedIndex = 0;
            dropdownOIVSpec.SelectedIndex = 0;

            btnPackage.Size = smallSize;
            btnDLCPacks.Size = smallSize;
            btnReplaceV.Size = smallSize;
            btnXML.Size = smallSize;
            btnShowSidebar.Size = new Size(40, 40);

            panelSidebar.Left = -panelSidebar.Width;
            panelSidebar.BringToFront();

            webViewBackground.SendToBack();

            int bannerLeft = 60;
            int bannerRightMargin = 130;

            await webViewBackground.EnsureCoreWebView2Async();

            webViewBackground.CoreWebView2.Settings.IsStatusBarEnabled = false;
            webViewBackground.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webViewBackground.CoreWebView2.Settings.AreDevToolsEnabled = false;

            webViewBackground.CoreWebView2.NavigateToString(GetBackgroundHtml());

            // Legg top controls foran banneret
            btnShowSidebar.BringToFront();
            button5.BringToFront(); // close
            button6.BringToFront(); // minimize
            button7.BringToFront(); // maximize
            panelSidebar.BringToFront();

            panelDrag.MouseDown += DragWindow;

            btnShowSidebar.Text = "";
            btnShowSidebar.ImageAlign = ContentAlignment.MiddleCenter;
            btnShowSidebar.FlatStyle = FlatStyle.Flat;
            btnShowSidebar.FlatAppearance.BorderSize = 0;
            var img = Properties.Resources.Sidebar_open;
            btnShowSidebar.Image = new Bitmap(img, new Size(50, 50));
        }

        private string GetBackgroundHtml()
        {
            return @"
<!DOCTYPE html>
<html>
<body style='margin:0; overflow:hidden; background:black;'>
<canvas id='c'></canvas>

<script>
const canvas = document.getElementById('c');
const ctx = canvas.getContext('2d');

function resizeCanvas() {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
}
resizeCanvas();
window.addEventListener('resize', resizeCanvas);

const STAR_COUNT = 260;
const SPEED = 4;

const stars = [];

function resetStar(star, randomZ = true) {
    star.x = (Math.random() - 0.5) * canvas.width;
    star.y = (Math.random() - 0.5) * canvas.height;
    star.z = randomZ ? Math.random() * canvas.width : canvas.width;
    star.pz = star.z;
}

for (let i = 0; i < STAR_COUNT; i++) {
    const star = {};
    resetStar(star);
    stars.push(star);
}

function drawGrain() {
    const grainAmount = 1800;

    for (let i = 0; i < grainAmount; i++) {
        const x = Math.random() * canvas.width;
        const y = Math.random() * canvas.height;
        const alpha = Math.random() * 0.03;

        ctx.fillStyle = `rgba(255,255,255,${alpha})`;
        ctx.fillRect(x, y, 1, 1);
    }
}

function draw() {
    ctx.fillStyle = '#161616';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    drawGrain();

    const cx = canvas.width / 2;
    const cy = canvas.height / 2;

    for (let s of stars) {
        s.z -= SPEED;

        if (s.z <= 1) {
            resetStar(s, false);
            continue;
        }

        const sx = (s.x / s.z) * canvas.width + cx;
        const sy = (s.y / s.z) * canvas.width + cy;

        const px = (s.x / s.pz) * canvas.width + cx;
        const py = (s.y / s.pz) * canvas.width + cy;

        s.pz = s.z;

        if (sx < 0 || sx >= canvas.width || sy < 0 || sy >= canvas.height) {
            resetStar(s, false);
            continue;
        }

        const radius = Math.max(0.6, (1 - s.z / canvas.width) * 3.5);
        const alpha = Math.max(0.25, 1 - s.z / canvas.width);

        ctx.beginPath();
        ctx.strokeStyle = `rgba(180,220,255,${alpha})`;
        ctx.lineWidth = radius;
        ctx.shadowBlur = 10;
        ctx.shadowColor = 'rgba(140,200,255,0.9)';
        ctx.moveTo(px, py);
        ctx.lineTo(sx, sy);
        ctx.stroke();
    }

    ctx.shadowBlur = 0;
    requestAnimationFrame(draw);
}

draw();
</script>
</body>
</html>
";
        }


        private void btnShowSidebar_Click(object sender, EventArgs e)
        {
            if (sidebarAnimating)
                return;

            panelSidebar.BringToFront();
            panelSidebar.Visible = true;

            sidebarOpening = !sidebarOpen;
            sidebarAnimating = true;
            timerSidebar.Start();
        }

        //mouse hover
        private void MenuButton_MouseEnter(object sender, EventArgs e)
        {
            currentButton = sender as System.Windows.Forms.Button;
            isHovering = true;
            timerHover.Start();
        }

        private void MenuButton_MouseLeave(object sender, EventArgs e)
        {
            currentButton = sender as System.Windows.Forms.Button;
            isHovering = false;
            timerHover.Start();
        }

        private void timerHover_Tick(object sender, EventArgs e) //mouse hover timer
        {
            if (currentButton == null)
            {
                timerHover.Stop();
                return;
            }

            Size targetSize;

            if (currentButton == btnShowSidebar)
                targetSize = isHovering ? new Size(48, 48) : new Size(40, 40);
            else
                targetSize = isHovering ? bigSize : smallSize;

            int newWidth = currentButton.Width;
            int newHeight = currentButton.Height;

            if (currentButton.Width < targetSize.Width)
                newWidth = Math.Min(currentButton.Width + speed, targetSize.Width);
            else if (currentButton.Width > targetSize.Width)
                newWidth = Math.Max(currentButton.Width - speed, targetSize.Width);

            if (currentButton.Height < targetSize.Height)
                newHeight = Math.Min(currentButton.Height + speed, targetSize.Height);
            else if (currentButton.Height > targetSize.Height)
                newHeight = Math.Max(currentButton.Height - speed, targetSize.Height);

            int centerX = currentButton.Left + currentButton.Width / 2;
            int centerY = currentButton.Top + currentButton.Height / 2;

            currentButton.Size = new Size(newWidth, newHeight);
            currentButton.Left = centerX - currentButton.Width / 2;
            currentButton.Top = centerY - currentButton.Height / 2;

            if (currentButton.Width == targetSize.Width && currentButton.Height == targetSize.Height)
                timerHover.Stop();
        }
        private void timerSidebar_Tick(object sender, EventArgs e)
        {
            if (sidebarOpening)
            {
                panelSidebar.Left += sidebarSpeed;

                if (panelSidebar.Left >= 0)
                {
                    panelSidebar.Left = 0;
                    timerSidebar.Stop();
                    sidebarAnimating = false;
                    sidebarOpen = true;
                }
            }
            else
            {
                panelSidebar.Left -= sidebarSpeed;

                if (panelSidebar.Left <= -panelSidebar.Width)
                {
                    panelSidebar.Left = -panelSidebar.Width;
                    timerSidebar.Stop();
                    sidebarAnimating = false;
                    sidebarOpen = false;
                }
            }
        }

        private void btnCloseSidebar_Click(object sender, EventArgs e)
        {

            if (sidebarAnimating || !sidebarOpen)
                return;

            sidebarOpening = false;
            sidebarAnimating = true;
            timerSidebar.Start();
        }
        private void Form1_Click(object sender, EventArgs e)
        {
            if (sidebarOpen && !sidebarAnimating)
            {
                sidebarOpening = false;
                sidebarAnimating = true;
                timerSidebar.Start();
            }
        }

        private void SidebarBtn1_Click(object sender, EventArgs e)
        {
            if (sidebarAnimating)
                return;

            panelSidebar.BringToFront();
            panelSidebar.Visible = true;

            sidebarOpening = !sidebarOpen;
            sidebarAnimating = true;
            timerSidebar.Start();
        }
    }
}
