using Iis.DbLayer.OntologyData;
using Iis.OntologyData;
using Iis.OntologyManager.DuplicateSearch;
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

        public DuplicateSearchResultItem SelectedItem
        {
            get
            {
                var selectedRow = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;
                return selectedRow == null ? null : (DuplicateSearchResultItem)selectedRow.DataBoundItem;
            }
        }
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

            grid = _uiControlsCreator.GetDataGridView("gridDuplicateResult", null,
                new List<string> { "Value", "Url", "LinksCount" });
            grid.Columns[0].Width *= 2;
            grid.Columns[1].Width *= 5;
            grid.Columns[0].HeaderText = "Значення";
            grid.Columns[1].HeaderText = "Урл";
            grid.Columns[2].HeaderText = "Вхідні зв'язки";
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
            //_grid.ContextMenuStrip = menuChildren;
            
        }
        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            var resultItem = (DuplicateSearchResultItem)grid.Rows[e.RowIndex].DataBoundItem;
            if (resultItem == null) return;
            
            var color = resultItem.OrderNumber % 2 == 1 ? _style.RelationTypeBackColor : _style.AttributeTypeBackColor;
            var row = (DataGridViewRow)grid.Rows[e.RowIndex];
            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }
        private void OpenUrl()
        {
            if (SelectedItem == null) return;

            var url = SelectedItem.Url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        private void Delete()
        {
            if (SelectedItem == null) return;
            if (MessageBox.Show("Ви дійсно хочете видалити цю сутність?", "Видалення", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            _data.DeleteEntity(SelectedItem.Node.Id, true, true);
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
            grid.DataSource = searchResult.Items;
        }
    }
}
