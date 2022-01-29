using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Iis.Desktop.Common.Controls;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Desktop.SecurityManager.Controls
{
    public class UiAccessLevelTreeControl: UiBaseControl
    {
        private TreeView _treeView;
        private ISecurityLevelChecker _securityLevelChecker;

        public event Action<TreeNode> OnNodeSelect;
        public event Action<TreeNode> OnNodeRemove;
        public void SetUiValues(ISecurityLevelChecker securityLevelChecker)
        {
            _securityLevelChecker = securityLevelChecker;
            _uiControlsCreator.FillTreeView(
                _treeView,
                new[] { _securityLevelChecker.RootLevel },
                _ => _.Name,
                _ => _.Children,
                null);

            _treeView.ExpandAll();
        }
        public void SelectNode(Guid id)
        {
            var treeNode = GetAllNodes()
                .SingleOrDefault(_ => ((ISecurityLevel)_?.Tag)?.Id == id);

            MainPanel.Invoke((Action)(() =>
            {
                _treeView.SelectedNode = treeNode;
            }));
        }

        private ISecurityLevel SelectedLevel => (ISecurityLevel)_treeView.SelectedNode?.Tag;

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
            if (SelectedLevel == null) return;
            var newLevel = _securityLevelChecker.CreateChildLevel(SelectedLevel.UniqueIndex);
            var node = new TreeNode { Text = "< новий рівень >", Tag = newLevel };
            _treeView.SelectedNode.Nodes.Add(node);
            _treeView.SelectedNode = node;
            OnNodeSelect?.Invoke(node);
        }
        private void RemoveLevel()
        {
            if (MessageBox.Show("Дійсно видалити?", "Підтвердження", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                OnNodeRemove?.Invoke(_treeView.SelectedNode);
            }
        }
        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnNodeSelect?.Invoke(e.Node);
        }
        private IEnumerable<TreeNode> GetAllNodes() =>
            _treeView.Nodes
                .Cast<TreeNode>()
                .SelectMany(GetNodeBranch);
        private IEnumerable<TreeNode> GetNodeBranch(TreeNode node)
        {
            yield return node;

            foreach (TreeNode child in node.Nodes)
            {
                foreach (var childChild in GetNodeBranch(child))
                {
                    yield return childChild;
                }
            }
        }
    }
}