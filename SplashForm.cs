using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace MagicOGK_OIV_Builder
{
    public partial class SplashForm : Form
    {
        public SplashForm()
        {
            InitializeComponent();
            this.Shown += SplashForm_Shown;
        }

        private async void SplashForm_Shown(object sender, EventArgs e)
        {
            try
            {
                webViewBackground.SendToBack();

                await webViewBackground.EnsureCoreWebView2Async();

                string assetsPath = Path.Combine(Application.StartupPath, "Assets");

                webViewBackground.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                webViewBackground.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "appassets",
                    assetsPath,
                    Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
                );

                webViewBackground.CoreWebView2.Settings.IsZoomControlEnabled = false;
                webViewBackground.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                webViewBackground.CoreWebView2.Settings.AreDevToolsEnabled = false;

                webViewBackground.CoreWebView2.NavigateToString(@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
    html, body {
        margin: 0;
        width: 100%;
        height: 100%;
        overflow: hidden;
        background: #161616;
        font-family: Arial, sans-serif;
    }

    canvas {
        position: fixed;
        inset: 0;
        z-index: 1;
    }

    .logo {
        position: absolute;
        top: 42%;
        left: 50%;
        transform: translate(-50%, -50%);
        z-index: 2;
        width: 350px;
        height: auto;
        pointer-events: none;
        user-select: none;
    }

    .typing-container {
    position: absolute;
    top: calc(42% + 128px);
    left: 50%;
    transform: translateX(-50%);
    z-index: 2;
    min-width: 620px;
    text-align: center;
    color: rgba(255,255,255,0.96);
    font-size: 23px;
    font-weight: 500;
    letter-spacing: 0.4px;
    text-shadow: 0 0 14px rgba(255,255,255,0.10);
    white-space: nowrap;
    pointer-events: none;
    user-select: none;
}

    .cursor {
    display: inline-block;
    width: 7px;
    height: 24px;
    background: rgba(180,220,255,0.95);
    margin-left: 5px;
    animation: blink 0.85s step-end infinite;
    vertical-align: -3px;
}

    @keyframes blink {
        50% { opacity: 0; }
    }

    .progress-wrapper {
    position: absolute;
    top: calc(42% + 182px);
    left: 50%;
    transform: translateX(-50%);
    width: 380px;
    z-index: 2;
    pointer-events: none;
    user-select: none;
}

    .progress-container {
    position: relative;
    width: 100%;
    height: 10px;
    background: linear-gradient(180deg, rgba(255,255,255,0.08), rgba(255,255,255,0.04));
    border: 1px solid rgba(255,255,255,0.08);
    border-radius: 999px;
    overflow: hidden;
    box-shadow:
        inset 0 1px 0 rgba(255,255,255,0.04),
        inset 0 0 12px rgba(0,0,0,0.22),
        0 0 18px rgba(0,0,0,0.10);
    backdrop-filter: blur(4px);
}

    #progressBar {
    position: relative;
    width: 0%;
    height: 100%;
    border-radius: 999px;
    background: linear-gradient(90deg,
        rgba(120,190,255,0.96) 0%,
        rgba(102,170,255,1) 40%,
        rgba(150,220,255,0.98) 100%);
    box-shadow:
        0 0 10px rgba(120,190,255,0.35),
        0 0 22px rgba(120,190,255,0.20);
    transition: width 0.16s ease;
}

#progressBar::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(
        180deg,
        rgba(255,255,255,0.28) 0%,
        rgba(255,255,255,0.10) 38%,
        rgba(255,255,255,0.00) 100%
    );
    pointer-events: none;
}

    .progress-text {
    margin-top: 10px;
    text-align: center;
    font-size: 13px;
    font-weight: 700;
    letter-spacing: 1.2px;
    color: rgba(255,255,255,0.75);
    font-variant-numeric: tabular-nums;
}

    .fade-out {
        animation: fadeOut 0.45s ease forwards;
    }

    @keyframes fadeOut {
        from { opacity: 1; }
        to { opacity: 0; }
    }
</style>
</head>
<body>

<canvas id='c'></canvas>

<img class='logo' src='https://appassets/LogoSplash.png' />

<div class='typing-container'>
    <span id='typedText'></span><span class='cursor'></span>
</div>

<div class='progress-wrapper'>

    <div class='progress-container'>
        <div id='progressBar'></div>
    </div>

    <div class='progress-text' id='progressText'>Initializing 0%</div>

</div>

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
    const grainAmount = 1400;
    for (let i = 0; i < grainAmount; i++) {
        const x = Math.random() * canvas.width;
        const y = Math.random() * canvas.height;
        const alpha = Math.random() * 0.025;
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
        const alpha = Math.max(0.22, 1 - s.z / canvas.width);

        ctx.beginPath();
        ctx.strokeStyle = `rgba(180,220,255,${alpha})`;
        ctx.lineWidth = radius;
        ctx.shadowBlur = 10;
        ctx.shadowColor = 'rgba(140,200,255,0.85)';
        ctx.moveTo(px, py);
        ctx.lineTo(sx, sy);
        ctx.stroke();
    }

    ctx.shadowBlur = 0;
    requestAnimationFrame(draw);
}

draw();

// Elements
const typedText = document.getElementById('typedText');
const progressBar = document.getElementById('progressBar');
const progressText = document.getElementById('progressText');

// Typing config
const lines = [
    'Welcome to MagicOGK OIV Builder',
    'Modding GTA 5 Made Easy',
    'Loading your workspace...'
];

const typingSpeed = 55;
const deletingSpeed = 34;
const pauseDuration = 900;
const nextLineDelay = 220;

// State
let lineIndex = 0;
let charIndex = 0;
let isDeleting = false;
let typingFinished = false;
let splashFinished = false;

let progress = 0;
let displayedPercent = 0;
let progressPhase = 0;

// Helpers
function setProgress(value) {
    const safeValue = Math.max(0, Math.min(100, value));
    progress = safeValue;
    progressBar.style.width = safeValue + '%';

    const rounded = Math.round(safeValue);
    if (rounded !== displayedPercent) {
        displayedPercent = rounded;
        progressText.textContent = 'Initializing ' + rounded + '%';
    }
}

function randomBetween(min, max) {
    return Math.random() * (max - min) + min;
}

function finishSplash() {
    if (splashFinished) return;
    splashFinished = true;

    setProgress(100);
    document.body.classList.add('fade-out');

    setTimeout(() => {
        window.chrome.webview.postMessage('done');
    }, 420);
}

// Progress loop with ""real app"" feel
function progressLoop() {
    if (splashFinished) return;

    if (!typingFinished) {
        let increment = 0;
        let nextDelay = 0;

        // Phase 1: fast jump to ~20%
        if (progress < 20) {
            increment = randomBetween(1.2, 3.6);
            nextDelay = randomBetween(35, 80);
        }
        // Phase 2: smoother climb to ~55%
        else if (progress < 55) {
            increment = randomBetween(0.35, 1.1);
            nextDelay = randomBetween(50, 110);
        }
        // Phase 3: slower movement with occasional tiny jumps
        else if (progress < 82) {
            const burst = Math.random() < 0.18;
            increment = burst ? randomBetween(0.9, 1.8) : randomBetween(0.12, 0.45);
            nextDelay = burst ? randomBetween(70, 120) : randomBetween(60, 140);
        }
        // Phase 4: hover near the end, but don't finish yet
        else if (progress < 95) {
            const microJump = Math.random() < 0.22;
            increment = microJump ? randomBetween(0.4, 0.9) : randomBetween(0.04, 0.16);
            nextDelay = randomBetween(85, 170);
        }
        else {
            increment = randomBetween(0.01, 0.04);
            nextDelay = randomBetween(120, 220);
        }

        // Hard cap before typing is done
        const capped = Math.min(progress + increment, 96.2);
        setProgress(capped);

        setTimeout(progressLoop, nextDelay);
        return;
    }

    // Final flush to 100 after typing is done
    const remaining = 100 - progress;

    if (remaining <= 0.12) {
        finishSplash();
        return;
    }

    let increment;
    if (remaining > 20) {
        increment = remaining * 0.16;
    } else if (remaining > 8) {
        increment = remaining * 0.22;
    } else {
        increment = Math.max(0.22, remaining * 0.30);
    }

    setProgress(Math.min(100, progress + increment));
    setTimeout(progressLoop, 38);
}

// Typing loop
function typeLoop() {
    if (splashFinished) return;

    const currentLine = lines[lineIndex];

    if (!isDeleting) {
        typedText.textContent = currentLine.substring(0, charIndex + 1);
        charIndex++;

        if (charIndex === currentLine.length) {
            if (lineIndex === lines.length - 1) {
                setTimeout(() => {
                    typingFinished = true;
                }, 700);
                return;
            }

            isDeleting = true;
            setTimeout(typeLoop, pauseDuration);
            return;
        }

        setTimeout(typeLoop, typingSpeed);
    } else {
        typedText.textContent = currentLine.substring(0, charIndex - 1);
        charIndex--;

        if (charIndex === 0) {
            isDeleting = false;
            lineIndex++;
            setTimeout(typeLoop, nextLineDelay);
            return;
        }

        setTimeout(typeLoop, deletingSpeed);
    }
}

// Start
setProgress(0);
typeLoop();
progressLoop();
</script>

</body>
</html>
");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Feil i splash screen: " + ex.Message);
                this.Close();
            }
        }   // <- denne avslutter SplashForm_Shown

        private async void FadeToMain()
        {
            for (double i = 1.0; i >= 0; i -= 0.05)
            {
                this.Opacity = i;
                await Task.Delay(15);
            }

            this.Close();
        }

        private void CoreWebView2_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            string msg = e.TryGetWebMessageAsString();

            if (msg == "done")
            {
                FadeToMain();
            }
        }
    }
}