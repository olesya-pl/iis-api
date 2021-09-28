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
        CheckedListBox clbTargetTypes;
        ToolTip toolTip;

        TextBox txtFormFieldLines;
        Label lblFormFieldLines;
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

        private Dictionary<string, EmbeddingOptions> embeddingOptionsUkr = new Dictionary<string, EmbeddingOptions>
        {
            { "Не обов'язкове", EmbeddingOptions.Optional },
            { "Обов'язкове", EmbeddingOptions.Required },
            { "Множинне не обов'язкове", EmbeddingOptions.Multiple }
        };

        private RelationControlMode _mode;
        public UiRelationControl(
            UiControlsCreator uiControlsCreator, 
            Func<List<INodeTypeLinked>> getAllEntities,
            RelationControlMode mode)
        {
            _uiControlsCreator = uiControlsCreator;
            _getAllEntities = getAllEntities;
            _mode = mode;
            toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 0,
                ReshowDelay = 10,
                ShowAlways = true
            };
        }
        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 350);
            var containerTop = new UiContainerManager("MainOptions", panels.panelTop);
            containerTop.SetColWidth((int)(_style.ControlWidthDefault * 1.5));
            var containerBottom = new UiContainerManager("SecondaryOptions", panels.panelBottom);

            containerTop.Add(txtId = new TextBox { ReadOnly = true }, "ІД");
            containerTop.Add(txtName = new TextBox(), "Ім'я *");
            containerTop.Add(txtTitle = new TextBox(), "Заголовок *");
            SetToolTip(txtTitle, "Відображається в меню \"Створити об'єкт\" та агрегації");
            containerTop.Add(cmbEmbedding = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, "Обов'язковість");
            cmbEmbedding.DataSource = embeddingOptionsUkr.Keys.ToList();
            
            containerTop.Add(cmbContainer = new ComboBox(), "Розділ");
            SetToolTip(cmbContainer, "Розділ, під яким це поле буде відображатися у веб-додаткі. Можна вибрати існуючий або ввести новий. ");
            cmbContainer.DisplayMember = "Title";
            containerTop.Add(txtSortOrder = new TextBox(), "Індекс сортування");
            SetToolTip(txtSortOrder, "Ціле число. Визначає порядок сортировки всередині розділу");
            containerTop.Add(cbHidden = new CheckBox { Text = "Не відображати в веб додатку" });
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
                containerTop.Add(cmbTargetType, "Зв'язок до типу *");
                cmbTargetType.SelectedIndexChanged += (sender, e) => TargetTypeChanged();
                containerTop.Add(cbIsImportantRelation = new CheckBox { Text = "Важливе" });
                SetToolTip(cbIsImportantRelation, "Зробити це поле пріоритетним при пошуку");
                containerTop.Add(cbIsInversed = new CheckBox { Text = "Інвертоване" });
                SetToolTip(cbIsInversed, "Створити зеркальне поле у цільовому типу");
                cbIsInversed.Click += (o, e) => SetControlsEnabled();
                lblInversedCode = containerTop.Add(txtInversedCode = new TextBox(), "Інвертоване ім'я");
                SetToolTip(txtInversedCode, "Ім'я зеркального поля *");
                lblInversedTitle = containerTop.Add(txtInversedTitle = new TextBox(), "Інвертований заголовок");
                SetToolTip(txtInversedTitle, "Заголовок зеркального поля *");
                containerTop.Add(cbInversedMultiple = new CheckBox { Text = "Інвертоване множинне" });
                SetToolTip(cbInversedMultiple, "Зробити зеркальне поле множинним");

                containerTop.GoToNewColumn();
                clbTargetTypes = new CheckedListBox();
                containerTop.Add(clbTargetTypes, "Типи які можна вибрати", true);
                SetToolTip(clbTargetTypes, "Типи, які з'явяться для вибору у веб додатку");
            }

            if (_mode == RelationControlMode.ToAttribute)
            {
                containerTop.Add(cmbScalarType = _uiControlsCreator.GetEnumComboBox(typeof(ScalarType)), "Тип даних *");
                cmbScalarType.SelectedIndexChanged += (sender, e) => SetControlsEnabled();
                containerTop.Add(txtFormula = new TextBox(), "Формула");
                SetToolTip(txtFormula, "Правила до створення формул дивись у документації");
                
                cmbFormat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                cmbFormat.DataSource = _valuesForFormat;
                containerTop.Add(cmbFormat, "Формат");
                SetToolTip(cmbFormat, "Правило, з яким поле буде валидуватися у веб додатку");

                lblFormFieldLines = containerTop.Add(txtFormFieldLines = new TextBox(), "Рядків у текстовому полі");
                SetToolTip(txtFormFieldLines, "Числове поле. Кількість рядків у веб додатку");

                containerTop.Add(cbIsAggregated = new CheckBox { Text = "Агрегація" });
                SetToolTip(cbIsAggregated, "Додати це поле у аггрегацію при пошуку");
            }
        }

        public void SetUiValues(INodeTypeLinked nodeType, List<string> aliases)
        {
            _nodeType = nodeType;
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            cbHidden.Checked = nodeType.MetaObject?.Hidden ?? false;
            txtSortOrder.Text = nodeType.MetaObject?.SortOrder?.ToString();

            var embeddingOptions = nodeType.RelationType.EmbeddingOptions == EmbeddingOptions.None ? EmbeddingOptions.Optional
                : nodeType.RelationType.EmbeddingOptions;
            var embeddingUkr = embeddingOptionsUkr.FirstOrDefault(_ => _.Value == embeddingOptions).Key;
            _uiControlsCreator.SetSelectedValue(cmbEmbedding, embeddingUkr);
            FillContainers(nodeType);

            if (_mode == RelationControlMode.ToEntity)
            {
                cmbTargetType.DataSource = _getAllEntities();
                _uiControlsCreator.SetSelectedValue(cmbTargetType, nodeType.RelationType.TargetType.Name);
                cbIsImportantRelation.Checked = nodeType.MetaObject?.IsImportantRelation ?? false;
                FillTargetTypes(nodeType.TargetType, nodeType.MetaObject.TargetTypes);
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
            if (cmbTargetType != null)
            {
                cmbTargetType.DataSource = _getAllEntities();
                _uiControlsCreator.SetSelectedValue(cmbTargetType, null);
            }

            txtId.Text = string.Empty;
            txtName.Text = string.Empty;
            txtTitle.Text = string.Empty;
            cbHidden.Checked = false;
            txtSortOrder.Text = string.Empty;

            if (_mode == RelationControlMode.ToEntity)
            {
                cmbTargetType.DataSource = _getAllEntities();
                cmbTargetType.SelectedIndex = -1;
                cbIsImportantRelation.Checked = false;
                clbTargetTypes.Items.Clear();
            }

            if (_mode == RelationControlMode.ToAttribute)
            {
                cmbScalarType.SelectedIndex = -1;
                txtFormula.Text = string.Empty;
                cbIsAggregated.Checked = false;
                cmbFormat.Text = string.Empty;
                txtFormFieldLines.Text = string.Empty;
            }
            SetControlsEnabled();
        }

        private void FillTargetTypes(INodeTypeLinked nodeType, string[] savedTargetTypes)
        {
            var descendants = nodeType == null ? new List<INodeTypeLinked>()
                : nodeType.GetAllDescendants().Where(nt => !nt.IsAbstract);
            clbTargetTypes.Enabled = descendants.Any();
            clbTargetTypes.Items.Clear();

            if (nodeType != null)
            {
                var allowedTypes = descendants
                    .Union(new[] { nodeType })
                    .OrderBy(nt => nt.Name);


                foreach (var allowedType in allowedTypes)
                {
                    bool isChecked = savedTargetTypes == null
                        || savedTargetTypes.Contains(allowedType.Name);
                    clbTargetTypes.Items.Add(allowedType.Name, isChecked);
                }
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

        private EntityOperation[] GetCurrentAcceptsEntityOperations()
        {
            var targetType = (INodeTypeLinked)cmbTargetType?.SelectedItem;
            if (targetType == null) return null;

            return targetType.IsObject || targetType.IsEnum || targetType.IsObjectSign ? null
                : new[] { EntityOperation.Create, EntityOperation.Update, EntityOperation.Delete };
        }

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
                EmbeddingOptions = embeddingOptionsUkr.GetValueOrDefault(cmbEmbedding.SelectedItem.ToString()),
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

            if (string.IsNullOrEmpty(txtName.Text))
            {
                sb.AppendLine("Ім'я не може бути порожнім");
            }
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                sb.AppendLine("Заголовок не може бути порожнім");
            }
            if (!string.IsNullOrEmpty(txtSortOrder.Text) && !int.TryParse(txtSortOrder.Text, out n))
            {
                sb.AppendLine("Індекс сортування повинен бути цілим числом");
            }
            if (!string.IsNullOrEmpty(txtFormFieldLines?.Text) && !int.TryParse(txtFormFieldLines?.Text, out n))
            {
                sb.AppendLine("Рядків у текстовому полі повинне бути цілим числом");
            }
            if (cmbScalarType != null && cmbScalarType.SelectedItem == null)
            {
                sb.AppendLine("Тип даних не може бути порожнім");
            }
            if (cmbTargetType != null && cmbTargetType.SelectedItem == null)
            {
                sb.AppendLine("Зв'язок до типу не може бути порожнім");
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
            if (cmbScalarType != null)
            {
                txtFormFieldLines.Enabled = cmbScalarType.SelectedItem != null && (ScalarType)cmbScalarType.SelectedItem == ScalarType.String;
                lblFormFieldLines.Enabled = txtFormFieldLines.Enabled;
            }
        }

        private void SetToolTip(Control control, string text)
        {
            toolTip.SetToolTip(control, text);
        }

        private void TargetTypeChanged()
        {
            var targetType = (INodeTypeLinked)cmbTargetType.SelectedItem;
            FillTargetTypes(targetType, null);
        }
    }
}
