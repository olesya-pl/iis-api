using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.Desktop.Common.Controls
{
    public class UiLoginControl: UIBaseControl
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;

        protected override void CreateControls()
        {
            _container.Add(txtUsername = new TextBox(), "Ім'я користувача");
            _container.Add(txtPassword = new TextBox(), "Пароль");
            _container.Add(btnLogin = _uiControlsCreator.GetButton("Увійти"));
        }
    }
}
