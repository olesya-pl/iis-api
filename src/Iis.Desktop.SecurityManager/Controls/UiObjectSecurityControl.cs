using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Iis.Desktop.Common.Controls;
using Iis.Desktop.Common.Requests;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Desktop.SecurityManager.Controls
{
    public class UiObjectSecurityControl : UiBaseControl
    {
        private TextBox _txtId;
        private TextBox _txtTitle;
        private Button _btnSave;
        private TreeView _treeView;
        private RequestWraper _requestWrapper;
        private ISecurityLevelChecker _securityLevelChecker;
        private ObjectSecurityDto _currentObjectSecurityDto;

        public UiObjectSecurityControl(RequestWraper requestWrapper)
        {
            _requestWrapper = requestWrapper;
        }

        public void SetSecurityLevelChecker(ISecurityLevelChecker securityLevelChecker)
        {
            _securityLevelChecker = securityLevelChecker;
        }

        protected override void CreateControls()
        {
            _container.SetColWidth(500);
            _container.Add(_btnSave = _uiControlsCreator.GetButton("Зберегти"));
            _btnSave.Click += async (sender, e) => { await SaveAsync(); };
            _container.Add(_txtId = new TextBox(), "Id");
            _txtId.TextChanged += async (sender, e) => { await LoadAsync(); };
            _container.Add(_txtTitle = new TextBox { ReadOnly = true }, "Title");

            _treeView = _uiControlsCreator.GetTreeView();
            _treeView.CheckBoxes = true;
            _container.Add(_treeView, null, true);
        }

        private async Task LoadAsync()
        {
            Guid id;
            if (!Guid.TryParse(_txtId.Text, out id)) return;
            _currentObjectSecurityDto = await _requestWrapper.GetObjectSecurityDtosAsync(id).ConfigureAwait(false);
            MainPanel.Invoke((Action)(() =>
            {
                _txtTitle.Text = _currentObjectSecurityDto.Title;
                SetSecurityLevels();
            }));
        }

        private void SetSecurityLevels()
        {
            _uiControlsCreator.FillTreeView(
                _treeView,
                new[] { _securityLevelChecker.RootLevel },
                _ => _.Name,
                _ => _.Children,
                _ => _currentObjectSecurityDto?.SecurityIndexes.Contains(_.UniqueIndex) ?? false);
        }

        private IReadOnlyList<int> GetCheckedIndexes() =>
            _uiControlsCreator.GetAllNodes(_treeView.Nodes)
            .Where(_ => _.Checked)
            .Select(_ => ((ISecurityLevel)_.Tag).UniqueIndex)
            .ToList();

        private async Task SaveAsync()
        {
            if (_currentObjectSecurityDto == null) return;
            var objectSecurityDto = new ObjectSecurityDto
            {
                Id = _currentObjectSecurityDto.Id,
                SecurityIndexes = GetCheckedIndexes()
            };
            await _requestWrapper.SaveObjectSecurityDtoAsync(objectSecurityDto).ConfigureAwait(false);
            await LoadAsync();
        }
    }
}
