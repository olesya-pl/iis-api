using Iis.Desktop.Common.Controls;
using Iis.Interfaces.AccessLevels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiAccessLevelEditControl: UiBaseControl
    {
        TextBox txtName;
        Button btnSave;
        AccessLevel _accessLevel;
        int _numericIndex;

        public event Action OnSave;
        public string AccessLevelName => txtName.Text;

        public UiAccessLevelEditControl(AccessLevel accessLevel, int numericIndex)
        {
            _accessLevel = accessLevel;
            _numericIndex = numericIndex;
        }
        protected override void CreateControls()
        {
            txtName = new TextBox { ReadOnly = false };
            _container.Add(txtName, $"Назва");
            _container.Add(btnSave = new Button { Text = "Зберегти" });
            btnSave.Click += (sender, e) => { Save(); };
        }
        public void SetUiValues()
        {
            txtName.Text = _accessLevel?.Name;
        }
        private void Save()
        {
            try
            {
                if (_accessLevel == null) Create();
                else Update();

                OnSave?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Create()
        {
            _accessLevel = new AccessLevel(Guid.NewGuid(), txtName.Text, _numericIndex);
        }
        private void Update()
        {
            _accessLevel.Name = txtName.Text;
        }
    }
}
