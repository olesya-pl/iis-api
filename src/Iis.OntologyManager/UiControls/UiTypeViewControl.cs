using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiTypeViewControl: UIBaseControl
    {
        protected TextBox txtId;
        protected Guid? Id => string.IsNullOrEmpty(txtId.Text) ? (Guid?)null : new Guid(txtId.Text);
        protected override void CreateControls() { }
        protected Guid? _parentTypeId;
        public void SetParentTypeId(Guid? parentTypeId)
        {
            _parentTypeId = parentTypeId;
        }
    }
}
