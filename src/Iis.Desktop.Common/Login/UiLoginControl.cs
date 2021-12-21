using System;
using System.Collections.Generic;
using System.Text;
using Iis.Desktop.Common.Login;
using System.Windows.Forms;

namespace Iis.Desktop.Common.Controls
{
    public class UiLoginControl: UIBaseControl
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;

        public event Action<UserCredentials> OnLogin;

        protected override void CreateControls()
        {
            _container.Add(txtUsername = new TextBox(), "Ім'я користувача");
            _container.Add(txtPassword = new TextBox(), "Пароль");
            _container.Add(btnLogin = _uiControlsCreator.GetButton("Увійти"));
            btnLogin.Click += (sender, e) => { OnLogin?.Invoke(GetCredentials()); };
        }

        public UserCredentials GetCredentials()
        {
            return new UserCredentials
            {
                UserName = txtUsername.Text,
                Password = txtPassword.Text
            };
        }
    }
}
