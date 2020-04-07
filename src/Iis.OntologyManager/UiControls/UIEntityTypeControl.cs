﻿using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Iis.OntologySchema.ChangeParameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiEntityTypeControl: UIBaseControl, IUiNodeTypeControl
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
        private Button btnSave;
        private UiControlsCreator _uiControlsCreator;

        public event Action<IChildNodeType> OnShowRelationType;
        public event Action<IChildNodeType> OnShowTargetType;
        public event Action<INodeTypeLinked> OnShowEntityType;
        public event Action<IChildNodeType> OnChangeTargetType;
        public event Action<INodeTypeUpdateParameter> OnSave;

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
            btnSave = new Button { Text = "Save" };
            btnSave.Click += (sender, e) => { OnSave?.Invoke(GetUpdateParameter()); };
            _container.Add(btnSave);

            _container.GoToBottom();
            _container.StepDown();
            _container.SetFullWidthColumn();

            menuChildren = new ContextMenuStrip();
            menuChildren.Items.Add("Show Relation");
            menuChildren.Items[0].Click += (sender, e) => { gridChildrenEvent(OnShowRelationType); };
            menuChildren.Items.Add("Change Target Type");
            menuChildren.Items[1].Click += (sender, e) => { gridChildrenEvent(OnChangeTargetType); };

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
            return new NodeTypeUpdateParameter
            {
                Id = new Guid(txtId.Text),
                Title = txtTitle.Text,
                Meta = txtMeta.Text
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
            var grid = gridChildren;
            var selectedRow = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;
            if (selectedRow == null) return;
            var childNodeType = (IChildNodeType)selectedRow.DataBoundItem;
            action(childNodeType);
        }
        private void gridChildren_DoubleClick(object sender, EventArgs e)
        {
            gridChildrenEvent(OnShowTargetType);
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
