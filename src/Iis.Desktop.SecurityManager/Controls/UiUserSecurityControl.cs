using Iis.Desktop.Common.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Iis.Desktop.Common.Requests;
using System.Threading.Tasks;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Desktop.SecurityManager.Controls
{
    public class UiUserSecurityControl : UiBaseControl
    {
        public Action OnSave;

        private RequestWraper _requestWrapper;
        private DataGridView _grid;
        private TreeView _treeView;
        private Button _btnSave;
        private ISecurityLevelChecker _securityLevelChecker;
        private IReadOnlyList<UserSecurityDto> _allUsers;

        public UiUserSecurityControl(RequestWraper requestWrapper)
        {
            _requestWrapper = requestWrapper;
        }

        private UserSecurityDto SelectedUser
        {
            get
            {
                var selectedRow = _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0] : null;
                return (UserSecurityDto)selectedRow?.DataBoundItem;
            }
        }

        public void SetSecurityLevelChecker(ISecurityLevelChecker securityLevelChecker)
        {
            _securityLevelChecker = securityLevelChecker;
        }
        public void SetUiValues(IReadOnlyList<UserSecurityDto> users)
        {
            _allUsers = users.OrderBy(_ => _.Username).ToList();
            _grid.DataSource = _allUsers;
        }
        protected override void CreateControls()
        {
            int panelLeftWidth = MainPanel.Width / 3;
            var (panelLeft, panelRight) = _uiControlsCreator.GetLeftRightPanels(MainPanel, panelLeftWidth);
            _grid = _uiControlsCreator.GetDataGridView("Users", null, new List<string> { "Username" });
            _grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            panelLeft.Controls.Add(_grid);
            _uiControlsCreator.SetFullSize(_grid);
            _grid.CellFormatting += grid_CellFormatting;
            _grid.SelectionChanged += (sender, e) => { SetSecurityLevels(); };

            var container = new UiContainerManager("Right", panelRight, _style);
            container.SetFullWidthColumn();
            container.Add(_btnSave = _uiControlsCreator.GetButton("Зберегти"));
            _btnSave.Click += async (sender, e) => { await SaveAsync(); };

            _treeView = _uiControlsCreator.GetTreeView();
            _treeView.CheckBoxes = true;
            _uiControlsCreator.SetFullSize(_treeView);
            container.Add(_treeView, null, true);
        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            var userSecurityDto = (UserSecurityDto)grid.Rows[e.RowIndex].DataBoundItem;
            if (userSecurityDto == null) return;
            var color = _style.BackgroundColor;
            var row = (DataGridViewRow)grid.Rows[e.RowIndex];
            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }

        private void SetSecurityLevels()
        {
            _uiControlsCreator.FillTreeView(
                _treeView,
                new[] { _securityLevelChecker.RootLevel },
                _ => _.Name,
                _ => _.Children,
                _ => SelectedUser?.SecurityIndexes.Contains(_.UniqueIndex) ?? false);
        }

        private IReadOnlyList<int> GetCheckedIndexes() =>
            _uiControlsCreator.GetAllNodes(_treeView.Nodes)
            .Where(_ => _.Checked)
            .Select(_ => ((ISecurityLevel)_.Tag).UniqueIndex)
            .ToList();

        private async Task SaveAsync()
        {
            if (SelectedUser == null) return;
            var userSecurityDto = new UserSecurityDto
            {
                Id = SelectedUser.Id,
                Username = SelectedUser.Username,
                SecurityIndexes = GetCheckedIndexes()
            };
            await _requestWrapper.SaveUserSecurityDtoAsync(userSecurityDto).ConfigureAwait(false);
            OnSave?.Invoke();
        }
    }
}
