﻿using Iis.DbLayer.OntologyData;
using Iis.OntologyData;
using Iis.OntologyManager.DuplicateSearch;
using Iis.OntologySchema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        OntologyNodesData _data;
        public OntologyPatchSaver PatchSaver { get; set; }

        public event Func<OntologyNodesData> OnGetData;

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
            txtSearch.Text = "MilitaryOrganization: title, commonInfo.OpenName, commonInfo.RealNameShort";
            container.Add(txtUrl = new TextBox(), "Базовий Урл");
            txtUrl.Text = "http://qa.contour.net";

            container.Add(btnSearch = new Button { Text = "Шукати" });
            btnSearch.Width = _style.ButtonWidthDefault;
            btnSearch.Click += (sender, e) => { Search(true); };

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
            _data.DeleteEntity(id, true, true);
            PatchSaver.SavePatch(_data.Patch);
            Search(false);
        }
        private void Search(bool reloadData)
        {
            if (reloadData)
            {
                _data = OnGetData();
            }
            if (_data == null) return;

            var param = new DuplicateSearchParameter(txtSearch.Text, txtUrl.Text);
            var duplicateSearcher = new DuplicateSearcher(_data);
            var searchResult = duplicateSearcher.Search(param);
            ConfigureGrid(param);
            PopulateGrid(param, searchResult);
        }
        private DataGridViewColumn AddTextColumn(string name, string headerText)
        {
            var column = new DataGridViewColumn
            {
                Name = name,
                HeaderText = headerText,
                CellTemplate = new DataGridViewTextBoxCell()
            };
            grid.Columns.Add(column);
            return column;
        }
        private void ConfigureGrid(DuplicateSearchParameter param)
        {
            grid.Columns.Clear();
            AddTextColumn("Value", "Значення");

            foreach (var dotName in param.DotNames)
            {
                AddTextColumn("Data_" + dotName, dotName);
            }

            AddTextColumn("Url", "Урл");
            AddTextColumn("LinksCount", "Вхідні зв'язки");
            AddTextColumn("OrderNumber", "OrderNumber").Visible = true;
            AddTextColumn("Id", "Id").Visible = true;
        }
        private void PopulateGrid(DuplicateSearchParameter param, DuplicateSearchResult result)
        {
            grid.Rows.Clear();
            foreach (var item in result.Items)
            {
                var row = new DataGridViewRow();
                row.CreateCells(grid);
                row.Cells[grid.Columns["Value"].Index].Value = item.Value;
                foreach (var dotNameStr in param.DotNames)
                {
                    var dotName = new DotName(dotNameStr);
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
