using Iis.OntologyManager.Style;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiRelationEntityControl: UIBaseControl
    {
        private TextBox txtId;
        private TextBox txtName;
        private TextBox txtTitle;
        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "Id");
            _container.Add(txtName = new TextBox(), "Name");
            _container.Add(txtTitle = new TextBox(), "Title");
            _container.GoToNewColumn();
        }
    }
}
