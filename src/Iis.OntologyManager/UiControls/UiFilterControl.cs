using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiFilterControl
    {
        IGetTypesFilter _model;
        
        UiContainerManager _container;
        IOntologyManagerStyle _style;
        
        CheckBox cbFilterEntities;
        CheckBox cbFilterAttributes;
        CheckBox cbFilterRelations;
        TextBox txtFilterName;

        public Panel MainPanel { get; private set; }
        public int Width => _container.Right + _style.MarginHor;
        public int Height => _container.Bottom + _style.MarginVer * 2;

        public delegate void OnChangeHandler(IGetTypesFilter filter);
        public event OnChangeHandler OnChange;

        public Panel Initialize(IOntologyManagerStyle style)
        {
            _style = style;
            MainPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = _style.BackgroundColor
            };

            _container = new UiContainerManager(MainPanel, style);
            CreateControls();
            MainPanel.Width = _container.Right;
            MainPanel.Height = _container.Bottom + _style.MarginVer;
            return MainPanel;
        }

        public void CreateControls()
        {
            _container.SetColWidth(_style.ButtonWidthDefault);
            _container.Add(cbFilterEntities = new CheckBox { Text = "Entities", Checked = true });
            _container.Add(cbFilterAttributes = new CheckBox { Text = "Attributes" });
            _container.Add(cbFilterRelations = new CheckBox { Text = "Relations" });

            cbFilterEntities.CheckedChanged += OnUserInput;
            cbFilterAttributes.CheckedChanged += OnUserInput;
            cbFilterRelations.CheckedChanged += OnUserInput;

            _container.GoToNewColumn();
            _container.Add(txtFilterName = new TextBox(), "Name");
            txtFilterName.TextChanged += OnUserInput;
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
