using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.Desktop.Common.Styles
{
    public interface IDesktopStyleFactory
    {
        IDesktopStyle GetDefaultStyle(Control control);
    }
}
