using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.Desktop.SecurityManager.Style
{
    public class SecurityManagerStyle : ISecurityManagerStyle
    {
        public int MarginVer { get; set; }
        public int MarginVerSmall { get; set; }
        public int MarginHor { get; set; }
        public int ControlWidthDefault { get; set; }
        public int ButtonHeightDefault { get; private set; }
        public int ButtonWidthDefault { get; set; }
        public int CheckboxHeightDefault { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ComparisonBackColor { get; set; }
        public Color EntityTypeBackColor { get; set; }
        public Color AttributeTypeBackColor { get; set; }
        public Color RelationTypeBackColor { get; set; }
        public Font DefaultFont { get; set; }
        public Font SelectedFont { get; set; }
        public Font TypeHeaderNameFont { get; set; }

        public Color EntityOtherColor { get; } = Color.Khaki; //  Color.FromArgb(220, 220, 220);

        public Dictionary<string, Color> EntityColors { get; } = new Dictionary<string, Color>
        {
            { "ObjectOfStudy", Color.SkyBlue },
            { "Wiki", Color.FromArgb(230, 200, 255) },
            { "Enum", Color.LightPink },
            { "ObjectSign", Color.FromArgb(245, 192, 162) },
        };

        public static ISecurityManagerStyle GetDefaultStyle(Control control)
        {
            SizeF size;

            using (var graphics = control.CreateGraphics())
            {
                size = graphics.MeasureString("abcdefghij", control.Font);
            }
            var charWidth = size.Width / 10;
            var charHeight = size.Height;

            return new SecurityManagerStyle
            {
                MarginVer = (int)(charHeight / 8),
                MarginVerSmall = (int)(charHeight / 16),
                MarginHor = (int)(charWidth * 2),
                ControlWidthDefault = (int)(charWidth * 34),
                ButtonHeightDefault = (int)(charHeight * 2),
                ButtonWidthDefault = (int)(charWidth * 34),
                CheckboxHeightDefault = (int)(charHeight),
                BackgroundColor = Color.LightYellow,
                EntityTypeBackColor = Color.Khaki,
                AttributeTypeBackColor = Color.PaleGreen,
                RelationTypeBackColor = Color.Lavender,
                ComparisonBackColor = Color.Honeydew,
                DefaultFont = SystemFonts.DefaultFont,
                SelectedFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                TypeHeaderNameFont = new Font("Arial", 16, FontStyle.Bold)
            };
        }
    }
}
