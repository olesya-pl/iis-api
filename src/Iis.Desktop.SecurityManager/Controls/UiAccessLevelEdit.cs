using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Iis.Desktop.Common.Controls;

namespace Iis.Desktop.SecurityManager.Controls
{
    public class UiAccessLevelEdit : UiBaseControl
    {
        private TextBox txtId;
        private TextBox txtName;
        private Button btnSave;

        public void SetUiValues(TreeNode node)
        {
            txtId.Text = Guid.NewGuid().ToString("N");
            txtName.Text = node.Text;
        }

        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "ИД");
            _container.Add(txtName = new TextBox(), "Назва");
            _container.Add(new TextBox(), "Ще якійсь контрол");
            _container.Add(btnSave = _uiControlsCreator.GetButton("Зберегти"));
        }
    }
}
