using Iis.DbLayer.OntologyData;
using Iis.Interfaces.Ontology.Data;
using Iis.OntologyData;
using Iis.OntologyManager.DuplicateSearch;
using Iis.OntologySchema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiDuplicatesControl: UIBaseControl
    {
        TextBox txtSearch;
        TextBox txtUrl;
        Button btnSearch;
        DataGridView grid;
        ContextMenuStrip menuGrid;
        Label lblValuesCount;
        Label lblRecordsCount;

        IOntologyNodesData _data;
        public OntologyPatchSaver PatchSaver { get; set; }
        const string RECORDS_COUNT_TEXT = "Знайдено записей: ";
        const string VALUES_COUNT_TEXT = "Різних значень: ";

        public event Func<IOntologyNodesData> OnGetData;

        DataGridViewRow SelectedRow => grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;
        object SelectedValue(string columnName) =>
            SelectedRow == null ? null :
            SelectedRow.Cells[grid.Columns[columnName].Index].Value;
        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 200);
            var container = new UiContainerManager("DuplicateSearchOptions", panels.panelTop);
            container.SetColWidth(500);
            container.Add(txtSearch = new TextBox(), "Параметри пошуку");
            txtSearch.Text = "MilitaryOrganization: title, commonInfo.OpenName, commonInfo.RealNameShort (parent.title, parent.parent.title, parent.parent.parent.title)";
            container.Add(txtUrl = new TextBox(), "Базовий Урл");
            txtUrl.Text = "http://qa.contour.net";

            container.Add(btnSearch = new Button { Text = "Шукати" });
            btnSearch.Width = _style.ButtonWidthDefault;
            btnSearch.Click += (sender, e) => { Search(true); };
            container.Add(lblRecordsCount = new Label { Text = RECORDS_COUNT_TEXT });
            container.Add(lblValuesCount = new Label { Text = VALUES_COUNT_TEXT });

            grid = _uiControlsCreator.GetDataGridView("gridDuplicateResult", null, new List<string>());
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.Width = panels.panelBottom.Width;
            grid.ColumnHeadersVisible = true;
            grid.AutoGenerateColumns = false;

            menuGrid = new ContextMenuStrip();
            menuGrid.Items.Add("Видалити");
            menuGrid.Items[0].Click += (sender, e) => { Delete(); };
            grid.ContextMenuStrip = menuGrid;

            var bottomContainer = new UiContainerManager("DuplicateSearchResult", panels.panelBottom);
            bottomContainer.SetFullWidthColumn();
            bottomContainer.Add(grid, null, true);
            grid.DoubleClick += (sender, e) => { OpenUrl(); };
            grid.CellFormatting += grid_CellFormatting;
        }
        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            if (grid.Columns["OrderNumber"] == null) return;
            var row = grid.Rows[e.RowIndex];
            
            int orderNumber = Convert.ToInt32(row.Cells[grid.Columns["OrderNumber"].Index].Value);
            
            var color = orderNumber % 2 == 1 ? _style.RelationTypeBackColor : _style.AttributeTypeBackColor;
            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }
        private void OpenUrl()
        {
            var baseUrl = SelectedValue("Url")?.ToString();
            if (baseUrl == null) return;

            var url = baseUrl.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        private void Delete()
        {
            if (SelectedRow == null) return;
            if (MessageBox.Show("Ви дійсно хочете видалити цю сутність?", "Видалення", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            var id = (Guid)SelectedValue("Id");
            _data.RemoveNode(id);
            PatchSaver.SavePatch(_data.Patch);
            Search(false);
        }
        private void SearchNoTitles()
        {
            var nodes = _data.GetEntitiesByTypeName(null);
            var node = _data.GetNode(new Guid("d295b801-90a4-49b4-b822-08f3c3d360aa"));
            var title = node.GetSingleProperty("__title");
            var notitleNodes = nodes
                .Where(n => n.NodeType.IsObjectOfStudy &&
                    string.IsNullOrWhiteSpace(n.GetComputedValue("__title")))
                .ToList();
        }
        private void Search(bool reloadData)
        {
            if (reloadData)
            {
                _data = OnGetData();
            }
            if (_data == null) return;
            SearchNoTitles();

            var param = new DuplicateSearchParameter(txtSearch.Text, txtUrl.Text);
            var duplicateSearcher = new DuplicateSearcher(_data);
            var searchResult = duplicateSearcher.Search(param);
            ConfigureGrid(param);
            PopulateGrid(param, searchResult);
            lblRecordsCount.Text = RECORDS_COUNT_TEXT + searchResult.Items.Count.ToString();
            lblValuesCount.Text = VALUES_COUNT_TEXT + (searchResult.Items.Count == 0 ? "" : searchResult.Items.Max(i => i.OrderNumber).ToString());
        }
        private DataGridViewColumn AddTextColumn(string name, string headerText, int koef = 1)
        {
            var column = new DataGridViewColumn
            {
                Name = name,
                HeaderText = headerText,
                CellTemplate = new DataGridViewTextBoxCell()
            };
            column.Width *= koef;
            grid.Columns.Add(column);
            return column;
        }
        private void ConfigureGrid(DuplicateSearchParameter param)
        {
            grid.Columns.Clear();
            AddTextColumn("Value", "Значення", 2);

            foreach (var dotName in param.DotNames.Union(param.DistinctNames))
            {
                AddTextColumn("Data_" + dotName, dotName, 2);
            }

            AddTextColumn("Url", "Урл", 2);
            AddTextColumn("LinksCount", "Вхідні зв'язки");
            AddTextColumn("OrderNumber", "OrderNumber").Visible = false;
            AddTextColumn("Id", "Id").Visible = false;
        }
        private void PopulateGrid(DuplicateSearchParameter param, DuplicateSearchResult result)
        {
            grid.Rows.Clear();
            foreach (var item in result.Items)
            {
                var row = new DataGridViewRow();
                row.CreateCells(grid);
                row.Cells[grid.Columns["Value"].Index].Value = item.Value;
                foreach (var dotName in param.DotNames.Union(param.DistinctNames))
                {
                    row.Cells[grid.Columns["Data_" + dotName].Index].Value =
                        item.Node.GetSingleProperty(dotName)?.Value;
                }
                row.Cells[grid.Columns["Url"].Index].Value = item.Url;
                row.Cells[grid.Columns["LinksCount"].Index].Value = item.LinksCount;
                row.Cells[grid.Columns["OrderNumber"].Index].Value = item.OrderNumber;
                row.Cells[grid.Columns["Id"].Index].Value = item.Node.Id;
                grid.Rows.Add(row);
            }
        }
    }
}
