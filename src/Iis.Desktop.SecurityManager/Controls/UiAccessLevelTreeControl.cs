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
            var menu = new ContextMenuStrip();
            menu.Items.Add("Створити рівень");
            menu.Items[0].Click += (sender, e) => { CreateLevel(); };
            menu.Items.Add("Видалити рівень");
            menu.Items[1].Click += (sender, e) => { RemoveLevel(); };
            _treeView.ContextMenuStrip = menu;
        }

        private void CreateLevel()
        {
            var selectedNode = _treeView.SelectedNode;
            if (selectedNode == null) return;
            var node = new TreeNode();
            selectedNode.Nodes.Add(node);
            OnNodeSelect?.Invoke(node);
        }

        private void RemoveLevel()
        {
            if (MessageBox.Show("Дійсно видалити?", "Підтвердження", MessageBoxButtons.YesNo) == DialogResult.No) return;

            var selectedNode = _treeView.SelectedNode;
            
            if (selectedNode == null) return;
            var parent = selectedNode.Parent;
            parent.Nodes.Remove(selectedNode);
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnNodeSelect?.Invoke(e.Node);
        }
    }
}