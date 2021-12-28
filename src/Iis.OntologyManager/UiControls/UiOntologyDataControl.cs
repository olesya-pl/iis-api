using Iis.Desktop.Common.Controls;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiOntologyDataControl : UiBaseControl, IDataViewControl
    {
        DataGridView grid;
        Button btnAdd;
        INodeTypeLinked _nodeType;
        IOntologyNodesData _ontologyData;
        List<INodeTypeLinked> _attributes;
        IOntologyManagerStyle _appStyle;
        
        DataGridViewRow SelectedRow => grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;

        Guid? SelectedId => SelectedRow != null ? Guid.Parse(SelectedRow.Cells[grid.Columns["Id"].Index].Value.ToString()) : (Guid?)null;

        INode SelectedNode => SelectedId == null ? null : _ontologyData.GetNode((Guid)SelectedId);

        public UiOntologyDataControl(UiControlsCreator uiControlsCreator)
        {
            _uiControlsCreator = uiControlsCreator;
        }
        protected override void CreateControls()
        {
            _container.Add(btnAdd = new Button { Text = "Додати" });
            btnAdd.Click += (sender, e) => { AddNewItem(); };
            grid = _uiControlsCreator.GetDataGridView("gridData", null, new List<string>());
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.ColumnHeadersVisible = true;
            grid.AutoGenerateColumns = false;
            grid.DoubleClick += (sender, e) => { EditSelectedItem(); };
            grid.CellFormatting += grid_CellFormatting;

            _container.Add(grid, null, true);
        }
        public void SetUiValues(INodeTypeLinked nodeType, IOntologyNodesData ontologyData)
        {
            _nodeType = nodeType;
            _ontologyData = ontologyData;
            _attributes = GetAttributes();

            ConfigureGrid();
            
            var nodes = ontologyData.GetNodesByTypeId(nodeType.Id).ToList();
            PopulateGrid(nodes);
        }
        private void ConfigureGrid()
        {
            grid.Columns.Clear();

            foreach (var attribute in _attributes)
            {
                grid.AddTextColumn(attribute.Name, attribute.Name, 2);
            }
            grid.AddTextColumn("Id", "Id", 2);
        }
        private void PopulateGrid(List<INode> nodes)
        {
            grid.Rows.Clear();
            foreach (var node in nodes)
            {
                var row = new DataGridViewRow();
                row.CreateCells(grid);
                foreach (var attribute in _attributes)
                {
                    row.Cells[grid.Columns[attribute.Name].Index].Value = node.GetSingleProperty(attribute.Name)?.Value;
                }
                row.Cells[grid.Columns["Id"].Index].Value = node.Id.ToString();
                grid.Rows.Add(row);
            }
        }
        private int GetSortingValue(string name)
        {
            switch (name)
            {
                case "name": 
                    return 1;
                case "code":
                    return 2;
                case "sortOrder":
                    return 100;
                default:
                    return 10;
            }
        }
        private List<INodeTypeLinked> GetAttributes()
        {
            return _nodeType.GetAllProperties()
                .Where(nt => nt.RelationType.TargetType.Kind == Kind.Attribute
                   && nt.RelationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                .OrderBy(nt => GetSortingValue(nt.Name))
                .ToList();
        }
        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            var row = grid.Rows[e.RowIndex];

            var color = _appStyle.AttributeTypeBackColor;
            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }
        private void EditSelectedItem()
        {
            if (SelectedNode != null)
                ShowEditForm(SelectedNode);
        }
        private void AddNewItem()
        {
            ShowEditForm(null);
        }
        private void ShowEditForm(INode node)
        {
            var form = new Form()
            {
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Height = 600,
                StartPosition = FormStartPosition.CenterParent
            };
            var rootPanel = _uiControlsCreator.GetFillPanel(form);
            var editControl = new UiDataEditControl(node, _nodeType, _attributes, _ontologyData);
            editControl.Initialize("DataEditControl", rootPanel, _style);
            editControl.SetUiValues();
            editControl.OnSave += () =>
            {
                form.Close();
                form.DialogResult = DialogResult.OK;
                var nodes = _ontologyData.GetNodesByTypeId(_nodeType.Id).ToList();
                PopulateGrid(nodes);
            };
            form.ShowDialog();
        }
    }
}
