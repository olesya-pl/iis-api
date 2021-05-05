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
    public enum RelationControlMode
    {
        ToAttribute,
        ToEntity
    };
    public class UiRelationControl: UIBaseControl, IUiNodeTypeControl
    {
        TextBox txtId;
        TextBox txtName;
        TextBox txtTitle;
        RichTextBox txtMeta;
        ComboBox cmbEmbedding;
        ComboBox cmbScalarType;
        ComboBox cmbTargetType;
        TextBox txtSortOrder;
        CheckBox cbHidden;
        Button btnSave;
        TextBox txtFormula;
        CheckBox cbIsAggregated;
        ComboBox cmbFormat;
        CheckBox cbIsImportantRelation;
        CheckBox cbEditing;
        CheckedListBox clbTargetTypes;

        TextBox txtFormFieldType;
        TextBox txtFormFieldLines;
        TextBox txtFormFieldHint;
        ComboBox cmbFormFieldIcon;

        TextBox txtContainerId;
        TextBox txtContainerTitle;
        TextBox txtContainerType;

        CheckBox cbIsInversed;
        TextBox txtInversedCode;
        TextBox txtInversedTitle;
        CheckBox cbInversedMultiple;

        Func<List<INodeTypeLinked>> _getAllEntities;
        public event Action<INodeTypeUpdateParameter> OnSave;

        private List<string> _valuesForFormat = new List<string> { "", "email", "phone" };

        private Guid? Id => string.IsNullOrEmpty(txtId.Text) ? (Guid?)null : new Guid(txtId.Text);
        private Guid? _parentTypeId;
        
        private RelationControlMode _mode;
        public UiRelationControl(
            UiControlsCreator uiControlsCreator, 
            Func<List<INodeTypeLinked>> getAllEntities,
            RelationControlMode mode)
        {
            _uiControlsCreator = uiControlsCreator;
            _getAllEntities = getAllEntities;
            _mode = mode;
        }
        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 350);
            var containerTop = new UiContainerManager("MainOptions", panels.panelTop);
            var containerBottom = new UiContainerManager("SecondaryOptions", panels.panelBottom);

            containerTop.Add(txtId = new TextBox { ReadOnly = true }, "ІД");
            containerTop.Add(txtName = new TextBox(), "Ім'я");
            containerTop.Add(txtTitle = new TextBox(), "Заголовок");
            containerTop.Add(cmbEmbedding = _uiControlsCreator.GetEnumComboBox(typeof(EmbeddingOptions)), "Обов'язковість");
            containerTop.Add(txtSortOrder = new TextBox(), "Індекс сортування");
            containerTop.Add(cbHidden = new CheckBox { Text = "Відключений" });
            btnSave = new Button { Text = "Зберегти" };
            btnSave.Click += (sender, e) => { OnSave?.Invoke(GetUpdateParameter()); };
            containerTop.Add(btnSave);
            
            containerTop.GoToNewColumn();

            if (_mode == RelationControlMode.ToEntity)
            {
                cmbTargetType = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    BackColor = _style.BackgroundColor
                };
                containerTop.Add(cmbTargetType, "Зв'язок до типу:");
                containerTop.Add(cbIsImportantRelation = new CheckBox { Text = "Важливе" });

                containerTop.GoToNewColumn();
                clbTargetTypes = new CheckedListBox();
                containerTop.Add(clbTargetTypes, "Типи які можна вибрати", true);
            }

            if (_mode == RelationControlMode.ToAttribute)
            {
                containerTop.Add(cmbScalarType = _uiControlsCreator.GetEnumComboBox(typeof(ScalarType)), "Тип даних");
                containerTop.Add(txtFormula = new TextBox(), "Формула");
                
                cmbFormat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                cmbFormat.DataSource = _valuesForFormat;
                containerTop.Add(cmbFormat, "Формат");
                containerTop.Add(txtFormFieldLines = new TextBox(), "Рядків у текстовому полі");

                containerTop.Add(cbIsAggregated = new CheckBox { Text = "Агрегація" });
            }

            containerTop.GoToNewColumn();
            containerTop.Add(txtMeta = new RichTextBox(), "Meta", true);
        }

        public void SetUiValues(INodeTypeLinked nodeType, List<string> aliases)
        {
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            txtMeta.Text = nodeType.Meta;
            cbHidden.Checked = nodeType.MetaObject?.Hidden ?? false;
            txtSortOrder.Text = nodeType.MetaObject?.SortOrder?.ToString();
            _uiControlsCreator.SetSelectedValue(cmbEmbedding, nodeType.RelationType.EmbeddingOptions.ToString());

            if (_mode == RelationControlMode.ToEntity)
            {
                cmbTargetType.DataSource = _getAllEntities();
                _uiControlsCreator.SetSelectedValue(cmbTargetType, nodeType.RelationType.TargetType.Name);
                cbIsImportantRelation.Checked = nodeType.MetaObject?.IsImportantRelation ?? false;
                FillTargetTypes(nodeType);
            }

            if (_mode == RelationControlMode.ToAttribute)
            {
                _uiControlsCreator.SetSelectedValue(cmbScalarType, nodeType.RelationType.TargetType.AttributeType.ScalarType.ToString());
                txtFormula.Text = nodeType.MetaObject?.Formula;
                cbIsAggregated.Checked = nodeType.MetaObject?.IsAggregated ?? false;
                cmbFormat.Text = nodeType.MetaObject.Format ?? string.Empty;
                txtFormFieldLines.Text = nodeType.MetaObject.FormField?.Lines?.ToString();
            }
        }

        private void FillTargetTypes(INodeTypeLinked nodeType)
        {
            var descendants = nodeType.TargetType.GetAllDescendants().Where(nt => !nt.IsAbstract);
            clbTargetTypes.Enabled = descendants.Any();

            var allowedTypes = descendants
                .Union(new[] { nodeType.TargetType })
                .OrderBy(nt => nt.Name);
            
            clbTargetTypes.Items.Clear();
            foreach (var allowedType in allowedTypes)
            {
                bool isChecked = nodeType.MetaObject.TargetTypes == null
                    || nodeType.MetaObject.TargetTypes.Contains(allowedType.Name);
                clbTargetTypes.Items.Add(allowedType.Name, isChecked);
            }
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

        public void SetParentTypeId(Guid? parentTypeId)
        {
            _parentTypeId = parentTypeId;
        }

        public void CreateNew()
        {
            throw new NotImplementedException();
        }
    }
}
