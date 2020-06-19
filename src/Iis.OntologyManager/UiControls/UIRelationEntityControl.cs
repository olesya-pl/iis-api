using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Iis.OntologySchema.ChangeParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiRelationEntityControl: UiTypeViewControl, IUiNodeTypeControl
    {
        private TextBox txtName;
        private TextBox txtTitle;
        private RichTextBox txtMeta;
        private ComboBox cmbEmbedding;
        private ComboBox cmbTargetType;
        private Button btnSave;
        Func<List<INodeTypeLinked>> _getAllEntities;
        UiControlsCreator _uiControlsCreator;
        public event Action<INodeTypeUpdateParameter> OnSave;
        public UiRelationEntityControl(UiControlsCreator uiControlsCreator, Func<List<INodeTypeLinked>> getAllEntities)
        {
            _uiControlsCreator = uiControlsCreator;
            _getAllEntities = getAllEntities;
        }
        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "Id");
            _container.Add(txtName = new TextBox(), "Name");
            _container.Add(txtTitle = new TextBox(), "Title");
            _container.Add(cmbEmbedding = _uiControlsCreator.GetEnumComboBox(typeof(EmbeddingOptions)), "Embedding Options");
            cmbTargetType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Id",
                BackColor = _style.BackgroundColor
            };
            _container.Add(cmbTargetType);

            btnSave = new Button { Text = "Save" };
            btnSave.Click += (sender, e) => { OnSave?.Invoke(GetUpdateParameter()); };
            _container.Add(btnSave);
            _container.GoToNewColumn();
            _container.Add(txtMeta = new RichTextBox(), "Meta", true);
        }
        public void SetUiValues(INodeTypeLinked nodeType, List<string> aliases)
        {
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            txtMeta.Text = nodeType.Meta;
            _uiControlsCreator.SetSelectedValue(cmbEmbedding, nodeType.RelationType.EmbeddingOptions.ToString());
            cmbTargetType.DataSource = _getAllEntities();
            _uiControlsCreator.SetSelectedValue(cmbTargetType, nodeType.RelationType.TargetType.Name);
        }
        public void CreateNew()
        {
            txtId.Clear();
            txtName.Clear();
            txtTitle.Clear();
            txtMeta.Clear();
            cmbEmbedding.SelectedIndex = 0;
            cmbTargetType.DataSource = _getAllEntities();
            cmbTargetType.SelectedIndex = -1;
        }
        private INodeTypeUpdateParameter GetUpdateParameter()
        {
            var isNew = string.IsNullOrEmpty(txtId.Text);
            return new NodeTypeUpdateParameter
            {
                Id = isNew ? (Guid?)null : new Guid(txtId.Text),
                Name = isNew ? txtName.Text : null,
                Title = txtTitle.Text,
                Meta = txtMeta.Text,
                EmbeddingOptions = (EmbeddingOptions)cmbEmbedding.SelectedItem,
                TargetTypeId = (Guid)cmbTargetType.SelectedValue,
                ParentTypeId = _parentTypeId
            };
        }
    }
}
