using Iis.DataModel;
using Iis.DbLayer.OntologySchema;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Comparison;
using Iis.OntologySchema.Saver;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiComparisonControl: UIBaseControl
    {
        IReadOnlyCollection<IOntologySchemaSource> _schemaSources;
        IOntologySchemaService _schemaService;
        IOntologySchema _schema;
        ISchemaCompareResult _compareResult;
        CompareResultForGrid _compareResultForGrid;

        ComboBox cmbSchemaSourcesCompare;
        CheckBox cbNodeTypeCreate;
        CheckBox cbNodeTypeUpdate;
        CheckBox cbNodeTypeDelete;
        CheckBox cbAliasCreate;
        CheckBox cbAliasUpdate;
        CheckBox cbAliasDelete;
        DataGridView grid;
        public List<string> UpdatedDatabases = new List<string>();
        Color _createColor = Color.FromArgb(192, 255, 192);
        Color _updateColor = Color.Moccasin;
        Color _deleteColor = Color.FromArgb(255, 192, 192);
        Color _aliasColor = Color.DarkBlue;

        public UiComparisonControl(IReadOnlyCollection<IOntologySchemaSource> schemaSources,
            IOntologySchemaService schemaService,
            IOntologySchema schema)
        {
            _schemaSources = schemaSources;
            _schemaService = schemaService;
            _schema = schema;
        }

        protected override void CreateControls()
        {
            MainPanel.SuspendLayout();
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 120, 20);
            var container = new UiContainerManager("Comparison", panels.panelTop, _style.Common);

            cmbSchemaSourcesCompare = new ComboBox
            {
                Name = "cmbSchemaSourcesCompare",
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Title",
                BackColor = panels.panelTop.BackColor
            };
            var src = new List<IOntologySchemaSource>(_schemaSources);
            cmbSchemaSourcesCompare.Items.Add(string.Empty);
            cmbSchemaSourcesCompare.Items.AddRange(src.ToArray());
            cmbSchemaSourcesCompare.SelectedIndexChanged += (sender, e) => { CompareSchemas(); };
            container.Add(cmbSchemaSourcesCompare);

            var btnComparisonUpdate = new Button { Text = "Update database", MinimumSize = new Size { Height = _style.Common.ButtonHeightDefault } };
            btnComparisonUpdate.Click += (sender, e) => { UpdateComparedDatabase(); };
            container.Add(btnComparisonUpdate);

            container.GoToNewColumn();
            CreateCheckBoxes(container);
            CreateGrid(container, panels.panelBottom);

            panels.panelBottom.Controls.Add(grid);

            MainPanel.ResumeLayout();
            if (_schema.SchemaSource.SourceKind == SchemaSourceKind.Database)
            {
                _uiControlsCreator.SetSelectedValue(cmbSchemaSourcesCompare, _schema.SchemaSource.Title);
                CompareSchemas(_schema.SchemaSource);
            }
            else
            {
                grid.DataSource = new List<CompareResultForGridItem>();
            }
        }
        private void CreateCheckBoxes(UiContainerManager container)
        {
            container.Add(cbNodeTypeCreate = _uiControlsCreator.GetCheckBox("NodeType Create", true));
            container.Add(cbNodeTypeUpdate = _uiControlsCreator.GetCheckBox("NodeType Update", true));
            container.Add(cbNodeTypeDelete = _uiControlsCreator.GetCheckBox("NodeType Delete", false));
            container.GoToNewColumn();
            container.Add(cbAliasCreate = _uiControlsCreator.GetCheckBox("Aliases Create", false));
            container.Add(cbAliasUpdate = _uiControlsCreator.GetCheckBox("Aliases Update", false));
            container.Add(cbAliasDelete = _uiControlsCreator.GetCheckBox("Aliases Delete", false));

            cbNodeTypeCreate.BackColor = _createColor;
            cbNodeTypeUpdate.BackColor = _updateColor;
            cbNodeTypeDelete.BackColor = _deleteColor;
            cbAliasCreate.BackColor = _createColor;
            cbAliasUpdate.BackColor = _updateColor;
            cbAliasDelete.BackColor = _deleteColor;
            cbAliasCreate.ForeColor = _aliasColor;
            cbAliasUpdate.ForeColor = _aliasColor;
            cbAliasDelete.ForeColor = _aliasColor;

            cbNodeTypeCreate.Click += (sender, e) => { ChangeChecks(cbNodeTypeCreate.Checked, CompareResultItemType.NodeType, CompareResultOperation.Insert); };
            cbNodeTypeUpdate.Click += (sender, e) => { ChangeChecks(cbNodeTypeUpdate.Checked, CompareResultItemType.NodeType, CompareResultOperation.Update); };
            cbNodeTypeDelete.Click += (sender, e) => { ChangeChecks(cbNodeTypeDelete.Checked, CompareResultItemType.NodeType, CompareResultOperation.Delete); };
            cbAliasCreate.Click += (sender, e) => { ChangeChecks(cbAliasCreate.Checked, CompareResultItemType.Alias, CompareResultOperation.Insert); };
            cbAliasUpdate.Click += (sender, e) => { ChangeChecks(cbAliasUpdate.Checked, CompareResultItemType.Alias, CompareResultOperation.Update); };
            cbAliasDelete.Click += (sender, e) => { ChangeChecks(cbAliasDelete.Checked, CompareResultItemType.Alias, CompareResultOperation.Delete); };
        }
        private void CreateGrid(UiContainerManager container, Panel panel)
        {
            grid = _uiControlsCreator.GetDataGridView("gridCompareResult", null, new List<string>());
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.Width = panel.ClientRectangle.Width;
            grid.Height = panel.ClientRectangle.Height;
            grid.ReadOnly = false;
            grid.CellFormatting += grid_CellFormatting;

            grid.AddCheckBoxColumn("Checked", "");
            grid.AddTextColumn("ItemType", "ItemType");
            grid.AddTextColumn("Title", "Title", 2);
            grid.AddTextColumn("NewValue", "NewValue", 2);
            grid.AddTextColumn("OldValue", "OldValue", 2);
            grid.Columns["NewValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns["OldValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.ColumnHeadersVisible = true;
            grid.AutoGenerateColumns = false;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.ReadOnly = column.Name != "Checked";
            }
        }
        public void CompareSchemas(IOntologySchemaSource source = null, bool setCheckBoxes = true)
        {
            var selectedSource = source ?? (IOntologySchemaSource)cmbSchemaSourcesCompare.SelectedItem;
            if (selectedSource == null) return;

            var schema = _schemaService.GetOntologySchema(selectedSource);
            _compareResult = _schema.CompareTo(schema);
            _compareResultForGrid = new CompareResultForGrid(_compareResult);
            grid.DataSource = _compareResultForGrid.Items;
            
            if (setCheckBoxes) SetStartCheckBoxes();
            SetAllChecks();
        }
        private void SetStartCheckBoxes()
        {
            cbNodeTypeCreate.Checked = true;
            cbNodeTypeUpdate.Checked = true;
            cbNodeTypeDelete.Checked = false;
            cbAliasCreate.Checked = false;
            cbAliasUpdate.Checked = false;
            cbAliasDelete.Checked = false;
        }
        private void SetAllChecks()
        {
            ChangeChecks(cbNodeTypeCreate.Checked, CompareResultItemType.NodeType, CompareResultOperation.Insert);
            ChangeChecks(cbNodeTypeUpdate.Checked, CompareResultItemType.NodeType, CompareResultOperation.Update);
            ChangeChecks(cbNodeTypeDelete.Checked, CompareResultItemType.NodeType, CompareResultOperation.Delete);
            ChangeChecks(cbAliasCreate.Checked, CompareResultItemType.Alias, CompareResultOperation.Insert);
            ChangeChecks(cbAliasUpdate.Checked, CompareResultItemType.Alias, CompareResultOperation.Update);
            ChangeChecks(cbAliasDelete.Checked, CompareResultItemType.Alias, CompareResultOperation.Delete);
        }
        private void UpdateComparedDatabase()
        {
            if (_compareResult == null) return;
            if (MessageBox.Show($"Are you sure you want to update database {_compareResult.SchemaTo.SchemaSource.Title}?", "Update Database", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
            using var context = OntologyContext.GetContext(_compareResult.SchemaTo.SchemaSource.Data);
            var schema = _schemaService.GetOntologySchema(_compareResult.SchemaTo.SchemaSource);
            var schemaSaver = new OntologySchemaSaver(context, _compareResult.SchemaTo);

            schemaSaver.SaveToDatabase(_compareResult, _compareResultForGrid);
            UpdatedDatabases.Add(_compareResult.SchemaTo.SchemaSource.Title);
            CompareSchemas(null, false);
        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = grid.Rows[e.RowIndex];
            var item = (CompareResultForGridItem)row.DataBoundItem;

            if (item == null) return;

            var color = item.Operation switch
            {
                CompareResultOperation.Insert => _createColor,
                CompareResultOperation.Update => _updateColor,
                CompareResultOperation.Delete => _deleteColor
            };

            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.ForeColor = item.ItemType == CompareResultItemType.Alias ? _aliasColor : grid.DefaultCellStyle.ForeColor;
            style.SelectionBackColor = color;
            style.SelectionForeColor = style.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }

        private void ChangeChecks(bool isChecked, CompareResultItemType? itemType, CompareResultOperation? operation)
        {
            foreach (var item in _compareResultForGrid.Items)
            {
                if ((itemType == null || item.ItemType == itemType) &&
                    (operation == null || item.Operation == operation))
                {
                    item.Checked = isChecked;
                }
            }
            grid.Refresh();
        }
    }
}
