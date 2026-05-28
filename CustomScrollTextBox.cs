using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MagicOGK_OIV_Builder
{
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("")]
        public string PlaceholderText
        {
            get => innerTextBox.PlaceholderText;
            set => innerTextBox.PlaceholderText = value;
        }

        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => innerTextBox.ReadOnly;
            set => innerTextBox.ReadOnly = value;
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

        public void AppendText(string text)
        {
            innerTextBox.AppendText(text);
        }

        public void ScrollToCaret()
        {
            innerTextBox.ScrollToCaret();
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
}
