using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiFilterControl: UIBaseControl
    {
        IGetTypesFilter _model;

        CheckBox cbFilterEntities;
        CheckBox cbFilterAttributes;
        CheckBox cbFilterRelations;
        TextBox txtFilterName;

        public delegate void OnChangeHandler(IGetTypesFilter filter);
        public event OnChangeHandler OnChange;

        protected override void CreateControls()
        {
            _container.SetColWidth(_style.ButtonWidthDefault / 2);
            _container.Add(cbFilterEntities = new CheckBox {
                Text = "Entities",
                Checked = true,
                MinimumSize = new Size { Height = _style.ButtonHeightDefault }
            });
            _container.Add(cbFilterAttributes = new CheckBox {
                Text = "Attributes",
                MinimumSize = new Size { Height = _style.ButtonHeightDefault }
            });
            _container.Add(cbFilterRelations = new CheckBox {
                Text = "Relations",
                MinimumSize = new Size { Height = _style.ButtonHeightDefault }
            });

            cbFilterEntities.CheckedChanged += OnUserInput;
            cbFilterAttributes.CheckedChanged += OnUserInput;
            cbFilterRelations.CheckedChanged += OnUserInput;

            _container.GoToNewColumn();
            _container.Add(txtFilterName = new TextBox(), "Name");
            txtFilterName.TextChanged += OnUserInput;

            Log.Logger.Verbose("Filter controls:");
            foreach (Control control in _container.RootControl.Controls)
            {
                Log.Logger.Verbose($"... {control.GetType()}");
                Log.Logger.Verbose($"... ({control.Left},{control.Top},{control.Width},{control.Height})");
            }
        }
        public void OnUserInput(object sender, EventArgs e)
        {
            if (OnChange != null)
            {
                var filter = GetModel();
                OnChange(filter);
            }
        }
        public IGetTypesFilter GetModel()
        {
            var kinds = new List<Kind>();
            if (cbFilterEntities.Checked) kinds.Add(Kind.Entity);
            if (cbFilterAttributes.Checked) kinds.Add(Kind.Attribute);
            if (cbFilterRelations.Checked) kinds.Add(Kind.Relation);
            return new GetTypesFilter
            {
                Kinds = kinds,
                Name = txtFilterName.Text
            };
        }
        public void SetModel(IGetTypesFilter model)
        {
            _model = model;
            cbFilterEntities.Checked = model.Kinds.Contains(Kind.Entity);
            cbFilterAttributes.Checked = model.Kinds.Contains(Kind.Attribute);
            cbFilterRelations.Checked = model.Kinds.Contains(Kind.Relation);
            txtFilterName.Text = model.Name;
        }
    }
}
