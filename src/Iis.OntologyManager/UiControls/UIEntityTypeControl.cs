using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Iis.OntologySchema.ChangeParameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiEntityTypeControl: UiTypeViewControl, IUiNodeTypeControl
    {
        private TextBox txtName;
        private TextBox txtTitle;
        private RichTextBox txtAliases;
        private ComboBox cmbUniqueValueFieldName;
        private DataGridView gridInheritedFrom;
        private DataGridView gridInheritedBy;
        private DataGridView gridEmbeddence;
        private ContextMenuStrip menuChildren;
        private DataGridView gridChildren;
        private Button btnSave;
        private UiControlsCreator _uiControlsCreator;

        public event Action<IChildNodeType> OnShowRelationType;
        public event Action<IChildNodeType> OnShowTargetType;
        public event Action<INodeTypeLinked> OnShowEntityType;
        public event Action<INodeTypeUpdateParameter> OnSave;
        public event Action<Guid> OnCreateAttribute;
        public event Action<Guid> OnCreateRelationEntity;
        public event Action<IChildNodeType> OnDeleteRelationEntity;
        public event Action OnSetInheritance;

        public IChildNodeType SelectedChild
        {
            get
            {
                var selectedRow = gridChildren.SelectedRows.Count > 0 ? gridChildren.SelectedRows[0] : null;
                return selectedRow == null ? null : (IChildNodeType)selectedRow.DataBoundItem;
            }
        }

        public UiEntityTypeControl(UiControlsCreator uiControlsCreator)
        {
            _uiControlsCreator = uiControlsCreator;
        }
        public void SetUiValues(INodeTypeLinked nodeType, List<string> aliases)
        {
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            txtAliases.Lines = aliases.ToArray();
            
            var children = nodeType.GetAllChildren()
                .OrderBy(ch => ch.InheritedFrom)
                .ThenBy(ch => ch.RelationName)
                .ToList();
            gridChildren.DataSource = children;
            var attributesList = children
                .Where(ch => ch.Kind == Kind.Attribute)
                .OrderBy(ch => ch.Name)
                .Select(ch => ch.Name)
                .ToList();
            attributesList.Insert(0, string.Empty);
            cmbUniqueValueFieldName.DataSource = attributesList;

            var ancestors = nodeType.GetAllAncestors();
            gridInheritedFrom.DataSource = ancestors;
            gridInheritedBy.DataSource = nodeType.GetDirectDescendants();
            gridEmbeddence.DataSource = nodeType.GetNodeTypesThatEmbedded();
        }
        public void CreateNew()
        {
            txtId.Clear();
            txtName.Clear();
            txtTitle.Clear();
            gridChildren.DataSource = null;
            gridInheritedFrom.DataSource = null;
            gridInheritedBy.DataSource = null;
            gridEmbeddence.DataSource = null;
        }
        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "Id");
            _container.Add(txtName = new TextBox(), "Name");
            _container.Add(txtTitle = new TextBox(), "Title");

            cmbUniqueValueFieldName = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "",
                ValueMember = "",
                BackColor = _style.BackgroundColor
            };
            _container.Add(cmbUniqueValueFieldName, "Unique Value Field Name");

            btnSave = new Button { Text = "Save" };
            btnSave.Click += (sender, e) => { OnSave?.Invoke(GetUpdateParameter()); };
            _container.Add(btnSave);
            _container.GoToNewColumn();

            _container.Add(txtAliases = new RichTextBox(), "Aliases", true);
            _container.GoToNewColumn();

            gridInheritedFrom = GetRelationsGrid(nameof(gridInheritedFrom));
            _container.Add(gridInheritedFrom, "Inherited From:", true);
            _container.GoToNewColumn();

            gridInheritedBy = GetRelationsGrid(nameof(gridInheritedBy));
            _container.Add(gridInheritedBy, "Ancestor To:", true);
            _container.GoToNewColumn();

            gridEmbeddence = GetRelationsGrid(nameof(gridEmbeddence));
            _container.Add(gridEmbeddence, "Embedded By:", true);
            _container.GoToNewColumn();

            _container.GoToBottom();
            _container.StepDown();
            _container.SetFullWidthColumn();

            var menuInheritance = new ContextMenuStrip();
            menuInheritance.Items.Add("Set Inheritance");
            menuInheritance.Items[0].Click += (sender, e) => { OnSetInheritance?.Invoke(); };
            gridInheritedFrom.ContextMenuStrip = menuInheritance;

            menuChildren = new ContextMenuStrip();
            menuChildren.Items.Add("Show Target Type");
            menuChildren.Items[0].Click += (sender, e) => { gridChildrenEvent(OnShowTargetType); };
            menuChildren.Items.Add("Create Attribute");
            menuChildren.Items[1].Click += (sender, e) => { if (Id != null) OnCreateAttribute((Guid)Id); };
            menuChildren.Items.Add("Create Relation to Entity");
            menuChildren.Items[2].Click += (sender, e) => { if (Id != null) OnCreateRelationEntity((Guid)Id); };
            menuChildren.Items.Add("Delete");
            menuChildren.Items[3].Click += (sender, e) => { if (Id != null) gridChildrenEvent(OnDeleteRelationEntity); };

            gridChildren = _uiControlsCreator.GetDataGridView("gridChildren", null,
                new List<string> { "RelationName", "RelationTitle", "Name", "InheritedFrom", "EmbeddingOptions", "ScalarType" });
            gridChildren.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridChildren.ColumnHeadersVisible = true;
            gridChildren.AutoGenerateColumns = false;
            gridChildren.DoubleClick += gridChildren_DoubleClick;
            gridChildren.CellFormatting += gridChildren_CellFormatting;
            gridChildren.ContextMenuStrip = menuChildren;

            _container.Add(gridChildren, null, true);
        }
        private INodeTypeUpdateParameter GetUpdateParameter()
        {
            var isNew = string.IsNullOrEmpty(txtId.Text);
            return new NodeTypeUpdateParameter
            {
                Id = isNew ? (Guid?)null : new Guid(txtId.Text),
                Name = txtName.Text,
                Title = txtTitle.Text,
                ParentTypeId = null,
                Aliases = txtAliases.Lines,
                UniqueValueFieldName = string.IsNullOrEmpty(cmbUniqueValueFieldName.Text) ? null : cmbUniqueValueFieldName.Text
            };
        }
        private DataGridView GetRelationsGrid(string name)
        {
            var grid = _uiControlsCreator.GetDataGridView(name, null,
                new List<string> { "Name" });
            grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.CellFormatting += _style.GridTypes_CellFormatting;
            grid.DoubleClick += gridInheritance_DoubleClick;
            return grid;
        }
        private void gridInheritance_DoubleClick(object sender, EventArgs e)
        {
            if (OnShowEntityType == null) return;
            var grid = (DataGridView)sender;
            var selectedRow = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;
            if (selectedRow == null) return;
            var nodeType = (INodeTypeLinked)selectedRow.DataBoundItem;
            OnShowEntityType(nodeType);
        }
        private void gridChildrenEvent(Action<IChildNodeType> action)
        {
            if (action == null) return;
            if (SelectedChild != null)
            {
                action(SelectedChild);
            }
        }
        private void gridChildren_DoubleClick(object sender, EventArgs e)
        {
            gridChildrenEvent(OnShowRelationType);
        }
        private void gridChildren_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            var nodeType = (IChildNodeType)grid.Rows[e.RowIndex].DataBoundItem;
            if (nodeType == null) return;
            var color = _style.GetColorByNodeType(nodeType.Kind);
            var row = (DataGridViewRow)grid.Rows[e.RowIndex];
            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }
    }
}
