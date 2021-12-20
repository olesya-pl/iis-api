using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.Desktop.Common.Styles
{
    public class DesktopStyleFactory: IDesktopStyleFactory
    {
        public IDesktopStyle GetDefaultStyle(Control control)
        {
            SizeF size;

            using (var graphics = control.CreateGraphics())
            {
                size = graphics.MeasureString("abcdefghij", control.Font);
            }
            var charWidth = size.Width / 10;
            var charHeight = size.Height;

            return new DesktopStyle
            {
                MarginVer = (int)(charHeight / 8),
                MarginVerSmall = (int)(charHeight / 16),
                MarginHor = (int)(charWidth * 2),
                ControlWidthDefault = (int)(charWidth * 34),
                ButtonHeightDefault = (int)(charHeight * 2),
                ButtonWidthDefault = (int)(charWidth * 34),
                CheckboxHeightDefault = (int)charHeight,
                BackgroundColor = Color.LightYellow,
                DefaultFont = SystemFonts.DefaultFont,
                SelectedFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            };
        }
    }
}
