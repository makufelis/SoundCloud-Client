using System;
using System.Windows.Forms;

namespace SoundCloudPlayer
{
    public class NoHorizontalScrollPanel : Panel
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                const int WS_HSCROLL = 0x00100000; // Horizontal scroll bar style
                cp.Style &= ~WS_HSCROLL; // Disable horizontal scroll bar
                return cp;
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            // Prevent horizontal scrolling by always resetting the horizontal scroll position to 0
            if (se.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                this.HorizontalScroll.Value = 0;
                return;
            }
            base.OnScroll(se);
        }
    }
}
