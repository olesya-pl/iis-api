using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class ControlsStorage
    {
        public TextBox txtId{ get; set; }
        public TextBox txtName{ get; set; }
        public TextBox txtTitle{ get; set; }
        public CheckBox cbFilterEntities{ get; set; }
        public CheckBox cbFilterAttributes{ get; set; }
        public CheckBox cbFilterRelations{ get; set; }
        public TextBox txtFilterName{ get; set; }
        public DataGridView gridChildren{ get; set; }
        public Button btnTypeBack{ get; set; }
    }
}
