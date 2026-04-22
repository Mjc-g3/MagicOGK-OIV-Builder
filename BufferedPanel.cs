using System.Windows.Forms;

namespace MagicOGK_OIV_Builder
{
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }
}