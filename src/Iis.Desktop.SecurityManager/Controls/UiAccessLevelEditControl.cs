using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Iis.Desktop.Common.Controls;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Desktop.SecurityManager.Controls
{
    public class UiAccessLevelEditControl : UiBaseControl
    {
        public Action<SecurityLevelPlain> OnSave; 

        private TextBox txtId;
        private TextBox txtName;
        private TextBox txtUniqueIndex;
        private TextBox txtParent;
        private Button btnSave;
        private TreeNode treeNode;
        private ISecurityLevelChecker _securityLevelChecker;

        private ISecurityLevel CurrentLevel => (ISecurityLevel)treeNode.Tag;
        private string CurrentName => txtName.Text.Trim();

        public void SetSecurityLevelChecker(ISecurityLevelChecker securityLevelChecker)
        {
            _securityLevelChecker = securityLevelChecker;
        }
        public void SetUiValues(TreeNode node)
        {
            treeNode = node;
            txtId.Text = CurrentLevel.Id.ToString("N");
            txtName.Text = CurrentLevel.Name;
            txtUniqueIndex.Text = CurrentLevel.IsNew ? string.Empty : CurrentLevel.UniqueIndex.ToString();
            txtParent.Text = CurrentLevel.Parent?.Name;
        }

        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "ИД");
            _container.Add(txtName = new TextBox(), "Назва");
            _container.Add(txtUniqueIndex = new TextBox { Enabled = false }, "Унікальний індекс");
            _container.Add(txtParent = new TextBox { Enabled = false }, "Належить до");
            _container.Add(btnSave = _uiControlsCreator.GetButton("Зберегти"));
            btnSave.Click += BtnSaveClick;
        }

        private SecurityLevelPlain GetSecurityLevelPlain()
            => new SecurityLevelPlain(CurrentLevel) { Name = CurrentName };

        private void BtnSaveClick(object sender, EventArgs e)
        {
            try
            {
                btnSave.Enabled = false;
                var id = Guid.Parse(txtId.Text);
                var name = txtName.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Назва не може бути пустою");
                }
                else if (!_securityLevelChecker.IsNameUnique(id, name))
                {
                    MessageBox.Show("Назва має бути унікальною");
                }
                else
                {
                    OnSave?.Invoke(GetSecurityLevelPlain());
                }
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private bool NameIsUnique()
        {
            var levelWithTheSameName = _securityLevelChecker.GetSecurityLevelByName(CurrentName);
            return levelWithTheSameName == null || levelWithTheSameName.Id == CurrentLevel.Id;
        }
    }
}
