using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Iis.Desktop.Common.Controls;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Desktop.SecurityManager.Controls
{
    public class UiAccessLevelTree: UiBaseControl
    {
        private TreeView _treeView;

        public event Action<TreeNode> OnNodeSelect;
        protected override void CreateControls()
        {
            _container.Add(_treeView = _uiControlsCreator.GetTreeView());
            _treeView.Dock = DockStyle.Fill;
            _treeView.AfterSelect += TreeView_AfterSelect;
        }

        public void SetUiValues(ISecurityLevel rootLevel)
        {
            _uiControlsCreator.FillTreeView(
                _treeView,
                new[] { rootLevel },
                _ => _.Name,
                _ => _.Children);

            _treeView.ExpandAll();
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnNodeSelect?.Invoke(e.Node);
        }
    }
}