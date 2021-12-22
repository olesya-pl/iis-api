using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Iis.Desktop.Common.Controls;

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

            TreeNode node1, node2, node3;
            _treeView.Nodes.Add(node1 = new TreeNode(
                "Таємність",
                new[] { new TreeNode("Ніфіга не таємно"), new TreeNode("Чутка таємно"), new TreeNode("Дофіга таємно") }));
            _treeView.Nodes.Add(node2 = new TreeNode(
                "Планета",
                new[] { new TreeNode("Ретроградний Меркурій"), new TreeNode("Сатурн"), new TreeNode("Єбучий Плутон") }));
            _treeView.Nodes.Add(node3 = new TreeNode(
                "Краіна",
                new[] { new TreeNode("Гондурас"), new TreeNode("Мадагаскар"), new TreeNode("Сомалі") }));

            _treeView.AfterSelect += TreeView_AfterSelect;
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnNodeSelect?.Invoke(e.Node);
        }
    }
}