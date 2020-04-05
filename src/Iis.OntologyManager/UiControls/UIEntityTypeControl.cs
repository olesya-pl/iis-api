using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiEntityTypeControl: UIBaseControl
    {
        private TextBox txtId;
        private TextBox txtName;
        private TextBox txtTitle;
        private DataGridView gridInheritedFrom;
        private DataGridView gridInheritedBy;
        private DataGridView gridEmbeddence;
        private RichTextBox txtMeta;
        private ContextMenuStrip menuChildren;
        private DataGridView gridChildren;
        private Button btnTypeSave;
        private UiControlsCreator _uiControlsCreator;

        public UiEntityTypeControl(UiControlsCreator uiControlsCreator)
        {
            _uiControlsCreator = uiControlsCreator;
        }
        public void SetUiValues(INodeTypeLinked nodeType)
        {
            
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            var children = nodeType.GetAllChildren();
            gridChildren.DataSource = children;
            var ancestors = nodeType.GetAllAncestors();
            gridInheritedFrom.DataSource = ancestors;
            gridInheritedBy.DataSource = nodeType.GetDirectDescendants();
            gridEmbeddence.DataSource = nodeType.GetNodeTypesThatEmbedded();
            txtMeta.Text = nodeType.Meta;
        }
        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "Id");
            _container.Add(txtName = new TextBox(), "Name");
            _container.Add(txtTitle = new TextBox(), "Title");
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

            _container.GoToNewColumn();
            _container.Add(txtMeta = new RichTextBox(), "Meta", true);

            _container.GoToNewColumn(_style.ButtonWidthDefault);
            btnTypeSave = new Button { Text = "Save" };
            //btnTypeSave.Click += (sender, e) => { SaveTypeProperties(); };
            _container.Add(btnTypeSave);


            _container.GoToBottom();
            _container.StepDown();
            _container.SetFullWidthColumn();

            menuChildren = new ContextMenuStrip();
            menuChildren.Items.Add("Show Relation");
            //menuChildren.Items[0].Click += (sender, e) => { gridChildrenEvent(ChildrenShowRelation); };
            menuChildren.Items.Add("Change Target Type");
            //menuChildren.Items[1].Click += (sender, e) => { gridChildrenEvent(ChildrenChangeTargetType); };

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
        private DataGridView GetRelationsGrid(string name)
        {
            var grid = _uiControlsCreator.GetDataGridView(name, null,
                new List<string> { "Name" });
            grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //grid.CellFormatting += gridTypes_CellFormatting;
            //grid.DoubleClick += gridInheritance_DoubleClick;
            return grid;
        }
        private void gridChildrenEvent(Action<IChildNodeType> action)
        {
            var grid = gridChildren;
            var selectedRow = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;
            if (selectedRow == null) return;
            var childNodeType = (IChildNodeType)selectedRow.DataBoundItem;
            action(childNodeType);
        }
        private void menuChildren_showRelationClick(object sender, EventArgs e)
        {
            //gridChildrenEvent(ChildrenShowRelation);
        }
        private void gridChildren_DoubleClick(object sender, EventArgs e)
        {
            //gridChildrenEvent(cnt => SetNodeTypeView(cnt.TargetType, true));
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
            //style.Font = row.Selected ? SelectedFont : DefaultFont;
        }
        


    }
}
