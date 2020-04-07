using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Iis.OntologySchema.ChangeParameters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiRelationEntityControl: UIBaseControl, IUiNodeTypeControl
    {
        private TextBox txtId;
        private TextBox txtName;
        private TextBox txtTitle;
        private RichTextBox txtMeta;
        private ComboBox cmbEmbedding;
        private Button btnSave;
        UiControlsCreator _uiControlsCreator;
        public event Action<INodeTypeUpdateParameter> OnSave;
        public UiRelationEntityControl(UiControlsCreator uiControlsCreator)
        {
            _uiControlsCreator = uiControlsCreator;
        }
        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "Id");
            _container.Add(txtName = new TextBox(), "Name");
            _container.Add(txtTitle = new TextBox(), "Title");
            _container.Add(cmbEmbedding = _uiControlsCreator.GetEnumComboBox(typeof(EmbeddingOptions)), "Embedding Options");
            btnSave = new Button { Text = "Save" };
            btnSave.Click += (sender, e) => { OnSave?.Invoke(GetUpdateParameter()); };
            _container.Add(btnSave);
            _container.GoToNewColumn();
            _container.Add(txtMeta = new RichTextBox(), "Meta", true);
        }
        public void SetUiValues(INodeTypeLinked nodeType)
        {
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            txtMeta.Text = nodeType.Meta;
            _uiControlsCreator.SetSelectedValue(cmbEmbedding, nodeType.RelationType.EmbeddingOptions.ToString());
        }
        private INodeTypeUpdateParameter GetUpdateParameter()
        {
            return new NodeTypeUpdateParameter
            {
                Id = new Guid(txtId.Text),
                Title = txtTitle.Text,
                Meta = txtMeta.Text,
                EmbeddingOptions = (EmbeddingOptions)cmbEmbedding.SelectedItem
            };
        }
    }
}
