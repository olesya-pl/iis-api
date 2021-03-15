using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiDataEditControl: UIBaseControl
    {
        INode _node;
        List<INodeTypeLinked> _attributes;
        IOntologyNodesData _ontologyData;
        INodeTypeLinked _nodeType;
        Dictionary<string, Control> _controls = new Dictionary<string, Control>();
        Button btnSave;

        public event Action OnSave;

        public UiDataEditControl(INode node, INodeTypeLinked nodeType, List<INodeTypeLinked> attributes, IOntologyNodesData ontologyData)
        {
            _node = node;
            _attributes = attributes;
            _nodeType = nodeType;
            _ontologyData = ontologyData;
        }
        protected override void CreateControls()
        {
            foreach (var attribute in _attributes)
            {
                var textBox = new TextBox { ReadOnly = false };
                _controls.Add(attribute.Name, textBox);
                _container.Add(textBox, $"{attribute.Title} ({attribute.Name})");
            }
            _container.Add(btnSave = new Button { Text = "Зберегти"});
            btnSave.Click += (sender, e) => { Save(); };
        }
        public void SetUiValues()
        {
            foreach (var attribute in _attributes)
            {
                var textBox = (TextBox)_controls[attribute.Name];
                textBox.Text = _node?.GetSingleProperty(attribute.Name)?.Value;
            }
        }
        private void Save()
        {
            try
            {
                if (_node == null) Create();
                else Update();

                OnSave?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Create()
        {
            _node = _ontologyData.CreateNode(_nodeType.Id);
            foreach (var attribute in _attributes)
            {
                var textBox = (TextBox)_controls[attribute.Name];
                var newValue = textBox.Text.Trim();
                if (!string.IsNullOrEmpty(newValue))
                    _ontologyData.AddValueByDotName(_node.Id, newValue, attribute.Name);
            }
        }
        private void Update()
        {
            foreach (var attribute in _attributes)
            {
                var textBox = (TextBox)_controls[attribute.Name];
                var property = _node.GetSingleProperty(attribute.Name);

                var oldValue = property?.Value;
                var newValue = textBox.Text.Trim();

                if (oldValue == newValue || string.IsNullOrEmpty(oldValue) && string.IsNullOrEmpty(newValue))
                    continue;

                if (!string.IsNullOrEmpty(newValue))
                    _ontologyData.AddValueByDotName(_node.Id, newValue, attribute.Name);

                if (property != null)
                {
                    _ontologyData.RemoveNodeAndRelations(property.IncomingRelations.First().Id);
                }
            }
        }
    }
}
