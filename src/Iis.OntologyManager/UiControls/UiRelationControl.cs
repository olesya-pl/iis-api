using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Iis.OntologySchema.ChangeParameters;
using Iis.OntologySchema.DataTypes;
using Newtonsoft.Json;
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

        TextBox txtFormFieldLines;
        ComboBox cmbContainer;

        CheckBox cbIsInversed;
        Label lblInversedCode;
        TextBox txtInversedCode;
        Label lblInversedTitle;
        TextBox txtInversedTitle;
        CheckBox cbInversedMultiple;

        Func<List<INodeTypeLinked>> _getAllEntities;
        public event Action<INodeTypeUpdateParameter> OnSave;

        private List<string> _valuesForFormat = new List<string> { "", "email", "phone" };

        private INodeTypeLinked _nodeType;
        private Guid? Id => string.IsNullOrEmpty(txtId.Text) ? (Guid?)null : new Guid(txtId.Text);
        private Guid? _parentTypeId;
        private List<IContainerMeta> _containers;

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
            containerTop.SetColWidth((int)(_style.ControlWidthDefault * 1.5));
            var containerBottom = new UiContainerManager("SecondaryOptions", panels.panelBottom);

            containerTop.Add(txtId = new TextBox { ReadOnly = true }, "ІД");
            containerTop.Add(txtName = new TextBox(), "Ім'я");
            containerTop.Add(txtTitle = new TextBox(), "Заголовок");
            containerTop.Add(cmbEmbedding = _uiControlsCreator.GetEnumComboBox(typeof(EmbeddingOptions)), "Обов'язковість");
            containerTop.Add(cmbContainer = new ComboBox(), "Контейнер");
            cmbContainer.DisplayMember = "Title";
            containerTop.Add(txtSortOrder = new TextBox(), "Індекс сортування");
            containerTop.Add(cbEditing = new CheckBox { Text = "Можливе редактування" });
            containerTop.Add(cbHidden = new CheckBox { Text = "Відключений" });
            btnSave = new Button { Text = "Зберегти" };
            btnSave.Click += (sender, e) => { ValidateAndSave(); };
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
                containerTop.Add(cbIsInversed = new CheckBox { Text = "Інвертоване" });
                cbIsInversed.Click += (o, e) => SetControlsEnabled();
                lblInversedCode = containerTop.Add(txtInversedCode = new TextBox(), "Інвертоване ім'я");
                lblInversedTitle = containerTop.Add(txtInversedTitle = new TextBox(), "Інвертований заголовок");
                containerTop.Add(cbInversedMultiple = new CheckBox { Text = "Інвертоване множинне" });

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
            _nodeType = nodeType;
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            txtMeta.Text = nodeType.Meta;
            cbHidden.Checked = nodeType.MetaObject?.Hidden ?? false;
            txtSortOrder.Text = nodeType.MetaObject?.SortOrder?.ToString();
            cbEditing.Checked = nodeType.CanBeEditedOnUi;

            _uiControlsCreator.SetSelectedValue(cmbEmbedding, nodeType.RelationType.EmbeddingOptions.ToString());
            FillContainers(nodeType);

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
            
            SetControlsEnabled();
        }

        public void SetParentTypeId(Guid? parentTypeId)
        {
            _parentTypeId = parentTypeId;
        }

        public void CreateNew()
        {
            cmbTargetType.DataSource = _getAllEntities();
            _uiControlsCreator.SetSelectedValue(cmbTargetType, null);
            SetControlsEnabled();
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

        private List<IContainerMeta> GetAllContainers(INodeTypeLinked entityType)
        {
            var containers = entityType.GetAllProperties()
               .Where(nt => 
                    nt.MetaObject.Container != null && 
                    !string.IsNullOrEmpty(nt.MetaObject.Container.Title))
               .Select(nt => nt.MetaObject.Container)
               .ToList();

            var result = new List<IContainerMeta>();

            foreach (var container in containers)
            {
                if (!result.Any(c => c.Id == container.Id))
                {
                    result.Add(container);
                }
            }

            return result.OrderBy(c => c.Title).ToList();
        }

        private void FillContainers(INodeTypeLinked nodeType)
        {
            var sourceType = nodeType.RelationType.SourceType;
            var _containers = GetAllContainers(sourceType);
            cmbContainer.DataSource = _containers;

            var container = _containers.FirstOrDefault(_ => _.Id == nodeType.MetaObject.Container?.Id);
            _uiControlsCreator.SetSelectedValue(cmbContainer, nodeType.MetaObject.Container?.Title);
        }

        private int? GetCurrentSortOrder() =>
            string.IsNullOrEmpty(txtSortOrder?.Text) ? (int?) null : int.Parse(txtSortOrder.Text);

        private string GetCurrentFormat() =>
            string.IsNullOrEmpty(cmbFormat?.Text) ? null : cmbFormat.Text;

        private EntityOperation[] GetCurrentAcceptsEntityOperations() =>
            cbEditing?.Checked == true ? new[] { EntityOperation.Create, EntityOperation.Update, EntityOperation.Delete } :
            (EntityOperation[])null;

        private string[] GetCurrentTargetTypes()
        {
            if (clbTargetTypes == null) return new string[] { };
            if (clbTargetTypes.Items.Count == clbTargetTypes.CheckedItems.Count) return null;

            var result = new List<string>();
            foreach (var item in clbTargetTypes.CheckedItems)
            {
                result.Add(item.ToString());
            }
            return result.ToArray();
        }

        private int? GetCurrentFormFieldLines() =>
            string.IsNullOrEmpty(txtFormFieldLines?.Text) ? (int?)null : int.Parse(txtFormFieldLines.Text);

        private IFormField GetCurrentFormField()
        {
            if (_nodeType == null) return null;
            var type = _nodeType.MetaObject?.FormField?.Type;
            var hint = _nodeType.MetaObject?.FormField?.Hint;
            var lines = GetCurrentFormFieldLines();
            var icon = _nodeType.MetaObject?.FormField?.Icon;

            return type == null && hint == null && lines == null && icon == null ? null :
                new FormField
                {
                    Type = type,
                    Hint = hint,
                    Lines = lines,
                    Icon = icon
                };
        }

        private IContainerMeta GetCurrentContainer()
        {
            if (string.IsNullOrWhiteSpace(cmbContainer?.Text)) return null;
            
            var selected = (IContainerMeta)cmbContainer.SelectedItem;
            return selected == null ?
                new SchemaContainer { Id = Guid.NewGuid(), Title = cmbContainer.Text } :
                new SchemaContainer { Id = selected.Id, Title = selected.Title, Type = selected.Type };
        }

        private IInversedMeta GetCurrentInversedMeta() =>
            !(cbIsInversed?.Checked == true) ? null :
                new SchemaInversedMeta
                {
                    Code = txtInversedCode.Text,
                    Title = txtInversedTitle.Text,
                    Multiple = cbInversedMultiple.Checked
                };

        private ISchemaMeta GetMeta()
        {
            var meta = new SchemaMeta();
            meta.Hidden = cbHidden.Checked;
            meta.Formula = txtFormula?.Text;
            meta.IsAggregated = cbIsAggregated?.Checked;
            meta.SortOrder = GetCurrentSortOrder();
            meta.Format = GetCurrentFormat();
            meta.IsImportantRelation = cbIsImportantRelation?.Checked;
            meta.AcceptsEntityOperations = GetCurrentAcceptsEntityOperations();
            meta.TargetTypes = GetCurrentTargetTypes();
            meta.FormField = GetCurrentFormField();
            meta.Container = GetCurrentContainer();
            meta.Inversed = GetCurrentInversedMeta();

            return meta;
        }

        private string GetMetaJson() => JsonConvert.SerializeObject(GetMeta());

        private INodeTypeUpdateParameter GetUpdateParameter()
        {
            var isNew = string.IsNullOrEmpty(txtId.Text);
            return new NodeTypeUpdateParameter
            {
                Id = isNew ? (Guid?)null : new Guid(txtId.Text),
                Name = txtName.Text,
                Title = txtTitle.Text,
                Meta = GetMetaJson(),
                EmbeddingOptions = (EmbeddingOptions)cmbEmbedding.SelectedItem,
                ScalarType = (ScalarType?)cmbScalarType?.SelectedItem,
                TargetTypeId = (Guid?)(cmbTargetType?.SelectedValue),
                ParentTypeId = _parentTypeId
            };
        }

        private void ValidateAndSave()
        {
            var errors = GetValidationErrors();
            if (string.IsNullOrEmpty(errors))
            {
                OnSave?.Invoke(GetUpdateParameter()); 
            }
            else
            {
                MessageBox.Show(errors);
            }
        }

        private string GetValidationErrors()
        {
            var sb = new StringBuilder();
            int n;

            if (!string.IsNullOrEmpty(txtSortOrder.Text) && !int.TryParse(txtSortOrder.Text, out n))
            {
                sb.AppendLine("Індекс сортування повинен бути цілим числом");
            }
            if (cmbTargetType != null && cmbTargetType.SelectedItem == null)
            {
                sb.AppendLine("Тип повинен бути вибраним");
            }
            return sb.ToString();
        }

        private void SetControlsEnabled()
        {
            if (cbIsInversed != null)
            {
                bool isInversed = cbIsInversed.Checked;
                lblInversedCode.Enabled = isInversed;
                txtInversedCode.Enabled = isInversed;
                lblInversedTitle.Enabled = isInversed;
                txtInversedTitle.Enabled = isInversed;
                cbInversedMultiple.Enabled = isInversed;
            }
        }
    }
}
