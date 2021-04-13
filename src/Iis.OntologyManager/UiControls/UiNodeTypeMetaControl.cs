using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiNodeTypeMetaControl: UIBaseControl
    {
        protected override void CreateControls()
        {
            _container.Add(new TextBox { ReadOnly = true }, "Id");
        }
    }
}
