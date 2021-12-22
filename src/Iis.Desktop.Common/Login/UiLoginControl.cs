using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Iis.Desktop.Common.Login;
using System.Threading.Tasks;

namespace Iis.Desktop.Common.Controls
{
    public class UiLoginControl: UiBaseControl
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Label lblError;
        private Button btnLogin;
        private ILoginRequestWrapper _loginRequestWrapper;

        public event Action<UserCredentials> OnLogin;

        public UiLoginControl(ILoginRequestWrapper loginRequestWrapper)
        {
            _loginRequestWrapper = loginRequestWrapper;
        }

        protected override void CreateControls()
        {
            _container.Add(txtUsername = new TextBox(), "Ім'я користувача");
            _container.Add(txtPassword = new TextBox(), "Пароль");
            txtUsername.Text = "olya";
            txtPassword.Text = "123";
            txtPassword.PasswordChar = '*';
            _container.Add(btnLogin = _uiControlsCreator.GetButton("Увійти"));
            _container.Add(lblError = new Label() { ForeColor = Color.Red, Text = " " });
            btnLogin.Click += async (sender, e) => { await Login(); };
        }

        private UserCredentials GetCredentials()
        {
            return new UserCredentials
            {
                UserName = txtUsername.Text,
                Password = txtPassword.Text
            };
        }

        private async Task Login()
        {
            var userCredentials = GetCredentials();
            var result = await _loginRequestWrapper.Login(userCredentials);

            if (result.IsSuccess)
            {
                lblError.Text = " ";
                OnLogin?.Invoke(userCredentials);
            }
            else
            {
                lblError.Text = result.Error.Message ?? "error";
            }
        }
    }
}
