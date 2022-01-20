using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Iis.Desktop.Common.Controls;

namespace Iis.Desktop.SecurityManager.Controls
{
    public class UiAccessLevelEditControl : UiBaseControl
    {
        private TextBox txtId;
        private TextBox txtName;
        private Button btnSave;
        private TreeNode treeNode;

        //public 

        public void SetUiValues(TreeNode node)
        {
            treeNode = node;
            txtId.Text = Guid.NewGuid().ToString("N");
            txtName.Text = node.Text;
        }

        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "ИД");
            _container.Add(txtName = new TextBox(), "Назва");
            //_container.Add(new TextBox(), "Ще якійсь контрол");
            _container.Add(btnSave = _uiControlsCreator.GetButton("Зберегти"));
            btnSave.Click += (sender, e) => { Save(); };
        }

        private void Save()
        {
            treeNode.Text = txtName.Text;
        }
    }
}
