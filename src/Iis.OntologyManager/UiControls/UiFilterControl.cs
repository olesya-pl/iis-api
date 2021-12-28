using Iis.Desktop.Common.Controls;
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
    public class UiFilterControl: UiBaseControl
    {
        private TextBox txtFilterName;
        private IOntologyManagerStyle _appStyle;

        private List<RadioButton> radioButtons = new List<RadioButton>();

        public delegate bool NodeTypeFilterFunc(INodeTypeLinked nodeType);
        private NodeTypeFilterFunc _filterFunc;

        public delegate void OnChangeHandler(NodeTypeFilterFunc filter);
        public event OnChangeHandler OnChange;

        public UiFilterControl(IOntologyManagerStyle appStyle)
        {
            _appStyle = appStyle;
        }

        protected override void CreateControls()
        {
            _container.SetColWidth(_style.ButtonWidthDefault / 2);

            var radioAll = new RadioButton
            {
                Text = "All",
                Checked = true,
                BackColor = _appStyle.EntityOtherColor
            };
            _container.Add(radioAll);
            radioAll.CheckedChanged += OnFilterChanged;
            radioButtons.Add(radioAll);

            foreach (var typeName in _appStyle.EntityColors.Keys)
            {
                var radio = new RadioButton
                {
                    Text = typeName,
                    Checked = false,
                    BackColor = _appStyle.EntityColors[typeName]
                };
                _container.Add(radio);
                radio.CheckedChanged += OnFilterChanged;
                radioButtons.Add(radio);
            }

            _container.GoToNewColumn();
            _container.Add(txtFilterName = new TextBox(), "Name");
            txtFilterName.TextChanged += OnFilterChanged;

            Log.Logger.Verbose("Filter controls:");
            foreach (Control control in _container.RootControl.Controls)
            {
                Log.Logger.Verbose($"... {control.GetType()}");
                Log.Logger.Verbose($"... ({control.Left},{control.Top},{control.Width},{control.Height})");
            }
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            var radio = radioButtons.Where(rb => rb.Checked).FirstOrDefault();

            _filterFunc = nt =>
                (radio.Text == "All" || nt.Name == radio.Text || nt.IsInheritedFrom(radio.Text)) &&
                (string.IsNullOrEmpty(txtFilterName.Text) || nt.Name.Contains(txtFilterName.Text, StringComparison.OrdinalIgnoreCase));

            OnChange(_filterFunc);
        }

        public NodeTypeFilterFunc GetModel() => _filterFunc;
    }
}
