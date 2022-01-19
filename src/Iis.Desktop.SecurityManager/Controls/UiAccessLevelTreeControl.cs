using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Iis.Desktop.Common.Controls;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Desktop.SecurityManager.Controls
{
    public class UiAccessLevelTreeControl: UiBaseControl
    {
        private TreeView _treeView;

        public event Action<TreeNode> OnNodeSelect;
        public void SetUiValues(ISecurityLevel rootLevel)
        {
            _uiControlsCreator.FillTreeView(
                _treeView,
                new[] { rootLevel },
                _ => _.Name,
                _ => _.Children,
                null);

            _treeView.ExpandAll();
        }

        protected override void CreateControls()
        {
            _container.Add(_treeView = _uiControlsCreator.GetTreeView());
            _treeView.Dock = DockStyle.Fill;
            _treeView.AfterSelect += TreeView_AfterSelect;
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnNodeSelect?.Invoke(e.Node);
        }
    }
}