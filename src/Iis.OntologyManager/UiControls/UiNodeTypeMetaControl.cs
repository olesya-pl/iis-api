using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiNodeTypeMetaControl: UIBaseControl
    {
        ISchemaMeta _meta;
        CheckBox cbHidden;
        TextBox txtFormula;
        CheckBox cbAggregated;
        public UiNodeTypeMetaControl()
        {
        }
        protected override void CreateControls()
        {
            _container.Add(cbHidden = new CheckBox(), "Не активний");
            _container.Add(txtFormula = new TextBox { ReadOnly = false }, "Формула");
            _container.Add(cbAggregated = new CheckBox(), "Aggregated");
        }
        public void SetUiValues(ISchemaMeta meta)
        {
            _meta = meta;
            cbHidden.Checked = _meta.Hidden ?? false;
            txtFormula.Text = _meta.Formula;
            cbAggregated.Checked = _meta.IsAggregated ?? false;
        }
    }
}
