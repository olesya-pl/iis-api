using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Net.Http;
using System.Text;
using AutoMapper;
using Iis.Desktop.SecurityManager.Style;
using Microsoft.Extensions.Configuration;
using Serilog;
using Iis.Desktop.Common.Controls;
using Iis.Desktop.Common.Styles;
using Iis.Desktop.Common.Configurations;
using Iis.Desktop.SecurityManager.Controls;
using Iis.Desktop.Common.Login;
using Iis.Desktop.Common.Requests;
using System.Threading.Tasks;
using Iis.Interfaces.SecurityLevels;
using Iis.Security.SecurityLevels;

namespace Iis.Desktop.SecurityManager
{
    public partial class MainForm : Form
    {
        #region Fields

        private IConfiguration _configuration;
        private IReadOnlyDictionary<string, EnvConfig> _environmentProperties;
        private IDesktopStyle _style;
        private const string VERSION = "0.1";
        private ILogger _logger;
        private UiControlsCreator _uiControlsCreator;
        private Panel panelMain;
        private Panel panelLeft;
        private Panel panelRight;
        private UiAccessLevelTreeControl _uiAccessLevelTreeControl;
        private UiAccessLevelEditControl _uiAccessLevelEditControl;
        private UiUserSecurityControl _uiUserSecurityControl;
        private TabControl tabControl;
        private EnvConfig _currentConfig;
        private UserCredentials _userCredentials;
        private RequestSettings _requestSettings;
        private SecurityLevelChecker _securityLevelChecker;
        private IReadOnlyList<UserSecurityDto> _users;

        #endregion

        #region Properties and Constructors

        public MainForm(
            IConfiguration configuration,
            ILogger logger,
            IDesktopStyleFactory desktopStyleFactory)
        {
            InitializeComponent();
            Text = $"Володар Таємниць {VERSION}";

            _configuration = configuration;
            _style = desktopStyleFactory.GetDefaultStyle(this);
            _logger = logger;
            _uiControlsCreator = new UiControlsCreator(_style);

            _environmentProperties = _configuration.GetSection(EnvConfig.SectionName)
                                        .Get<IReadOnlyDictionary<string, EnvConfig>>();

            panelMain = _uiControlsCreator.GetFillPanel(this);

            tabControl = new TabControl();
            panelMain.Controls.Add(tabControl);
            tabControl.Dock = DockStyle.Fill;

            _currentConfig = _environmentProperties["local"];
            _requestSettings = new RequestSettings { ReIndexTimeOutInMins = 25 };

            CreateSecurityAttributesTab(tabControl);
            CreateUsersTab(tabControl);

            ShowLogin();
        }

        #endregion

        #region UI Control Creators

        private void ShowLogin()
        {
            var panelModal = _uiControlsCreator.GetFillPanel(this);
            var panelLogin = _uiControlsCreator.GetPanel(panelModal);
            var appUriString = _environmentProperties["local"].ApiUri;
            var _loginRequestWrapper = new LoginRequestWrapper(new Uri(appUriString));
            var loginControl = new UiLoginControl(_loginRequestWrapper);
            loginControl.Initialize("LoginControl", panelLogin, _style);
            loginControl.OnLogin += async (credentials) => { await OnLogin(credentials, panelModal).ConfigureAwait(false); };
            panelLogin.Width = loginControl.Width;
            panelLogin.Height = loginControl.Height;
            _uiControlsCreator.PutToCenterOfParent(panelLogin);
            panelLogin.BorderStyle = BorderStyle.FixedSingle;
            panelMain.Visible = false;
        }

        private void CreateSecurityAttributesTab(TabControl tabControl)
        {
            var pageAttributes = new TabPage("Атрибути доступу");
            tabControl.TabPages.Add(pageAttributes);
            var pnlAttributes = _uiControlsCreator.GetFillPanel(pageAttributes);

            int panelLeftWidth = pnlAttributes.Width / 3;
            (panelLeft, panelRight) = _uiControlsCreator.GetLeftRightPanels(pnlAttributes, panelLeftWidth);

            _uiAccessLevelTreeControl = new UiAccessLevelTreeControl();
            _uiAccessLevelTreeControl.Initialize("AccessLevelTree", panelLeft, _style);

            _uiAccessLevelEditControl = new UiAccessLevelEditControl();
            _uiAccessLevelEditControl.Initialize("AccessLevelEdit", panelRight, _style);

            _uiAccessLevelTreeControl.OnNodeSelect += node => { _uiAccessLevelEditControl.SetUiValues(node); };
        }

        private void CreateUsersTab(TabControl tabControl)
        {
            var pageUser = new TabPage("Користувачи");
            tabControl.TabPages.Add(pageUser);
            var pnlUser = _uiControlsCreator.GetFillPanel(pageUser);

            _uiUserSecurityControl = new UiUserSecurityControl(GetRequestWrapper());
            _uiUserSecurityControl.Initialize("UserSecurityControl", pnlUser, _style);
            _uiUserSecurityControl.OnSave += async () => { await RefreshUsers(); };
        }

        #endregion

        #region Event Handlers

        private async Task OnLogin(UserCredentials userCredentials, Panel panelToHide)
        {
            panelMain.Visible = true;
            Controls.Remove(panelToHide);
            _userCredentials = userCredentials;
            var requestWrapper = GetRequestWrapper();
            var plainLevels = await requestWrapper.GetSecurityLevels().ConfigureAwait(false);
            _securityLevelChecker = new SecurityLevelChecker(plainLevels);
            _uiUserSecurityControl.SetSecurityLevelChecker(_securityLevelChecker);
            await RefreshUsers();

            Invoke((Action)(() =>
            {
                _uiAccessLevelTreeControl.SetUiValues(_securityLevelChecker.RootLevel);
            }));
        }

        private async Task RefreshUsers()
        {
            _users = await GetRequestWrapper().GetUserSecurityDtos().ConfigureAwait(false);

            Invoke((Action)(() =>
            {
                _uiUserSecurityControl.SetUiValues(_users);
            }));
        }

        #endregion

        private RequestWraper GetRequestWrapper() =>
            new RequestWraper(_currentConfig.ApiUri, _userCredentials, _requestSettings, _logger);

    }
}
