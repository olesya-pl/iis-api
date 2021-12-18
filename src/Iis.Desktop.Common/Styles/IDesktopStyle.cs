using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Iis.Desktop.Common.Styles
{
    public interface IDesktopStyle
    {
        Color BackgroundColor { get; }
        int ControlWidthDefault { get; }
        int ButtonHeightDefault { get; }
        int ButtonWidthDefault { get; }
        int CheckboxHeightDefault { get; }
        int MarginHor { get; }
        int MarginVer { get; }
        int MarginVerSmall { get; }
        Font DefaultFont { get; }
        Font SelectedFont { get; }
    }
}