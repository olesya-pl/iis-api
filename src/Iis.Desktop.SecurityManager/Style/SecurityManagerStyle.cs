using Iis.Desktop.Common.Styles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.Desktop.SecurityManager.Style
{
    public class SecurityManagerStyle : ISecurityManagerStyle
    {
        public static ISecurityManagerStyle GetDefaultStyle(Control control)
        {
            return new SecurityManagerStyle();
        }
    }
}
