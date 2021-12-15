using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.Style
{
    public class DesktopStyle : IDesktopStyle
    {
        public int MarginVer { get; set; }
        public int MarginVerSmall { get; set; }
        public int MarginHor { get; set; }
        public int ControlWidthDefault { get; set; }
        public int ButtonHeightDefault { get; set; }
        public int ButtonWidthDefault { get; set; }
        public int CheckboxHeightDefault { get; set; }
        public Color BackgroundColor { get; set; }
        public Font DefaultFont { get; set; }
        public Font SelectedFont { get; set; }
    }
}
