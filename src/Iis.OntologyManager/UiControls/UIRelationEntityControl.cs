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
        UiNodeTypeMetaControl _metaControl;
        public event Action<INodeTypeUpdateParameter> OnSave;
        public UiRelationEntityControl(UiControlsCreator uiControlsCreator, Func<List<INodeTypeLinked>> getAllEntities)
        {
            _uiControlsCreator = uiControlsCreator;
            _getAllEntities = getAllEntities;
        }
        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetLeftRightPanels(MainPanel, _style.ControlWidthDefault + 2 * _style.MarginHor);
            var containerLeft = new UiContainerManager("MainOptions", panels.panelLeft);
            
            containerLeft.Add(txtId = new TextBox { ReadOnly = true }, "Id");
            containerLeft.Add(txtName = new TextBox(), "Name");
            containerLeft.Add(txtTitle = new TextBox(), "Title");
            containerLeft.Add(cmbEmbedding = _uiControlsCreator.GetEnumComboBox(typeof(EmbeddingOptions)), "Embedding Options");

            cmbTargetType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Id",
                BackColor = _style.BackgroundColor
            };
            containerLeft.Add(cmbTargetType);

            btnSave = new Button { Text = "Save" };
            btnSave.Click += (sender, e) => { OnSave?.Invoke(GetUpdateParameter()); };
            containerLeft.Add(btnSave);
            containerLeft.Add(txtMeta = new RichTextBox(), "Meta", true);

            _metaControl = new UiNodeTypeMetaControl();
            _metaControl.Initialize("NodeTypeMetaControl", panels.panelRight);
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
            _metaControl.SetUiValues(nodeType.MetaObject);
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
                Name = txtName.Text,
                Title = txtTitle.Text,
                Meta = txtMeta.Text,
                EmbeddingOptions = (EmbeddingOptions)cmbEmbedding.SelectedItem,
                TargetTypeId = (Guid)cmbTargetType.SelectedValue,
                ParentTypeId = _parentTypeId
            };
        }
    }
}
