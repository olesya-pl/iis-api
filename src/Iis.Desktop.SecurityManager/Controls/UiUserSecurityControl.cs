using Iis.Desktop.Common.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Iis.Desktop.Common.Requests;

namespace Iis.Desktop.SecurityManager.Controls
{
    public class UiUserSecurityControl : UiBaseControl
    {
        private RequestWraper _requestWrapper;
        private ListView _listView;
        private TreeView _treeView;
        private Button _btnSave;
        public UiUserSecurityControl(RequestWraper requestWrapper)
        {
            _requestWrapper = requestWrapper;
        }
        protected override void CreateControls()
        {
            int panelLeftWidth = MainPanel.Width / 3;
            var (panelLeft, panelRight) = _uiControlsCreator.GetLeftRightPanels(MainPanel, panelLeftWidth);
            _listView = _uiControlsCreator.GetListView();
            _listView.Dock = DockStyle.Fill;
            panelLeft.Controls.Add(_listView);

            var container = new UiContainerManager("Right", panelRight, _style);
            container.SetColWidth(500);
            container.Add(_btnSave = _uiControlsCreator.GetButton("Зберегти"));

            _treeView = _uiControlsCreator.GetTreeView();
            container.Add(_treeView, null, true);
        }
    }
}
