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
        private UiAccessLevelTree _uiAccessLevelTree;
        private UiAccessLevelEdit _uiAccessLevelEdit;
        private TabControl tabControl;

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

            tabControl.TabPages.Add("Користувачи");

            CreateSecurityAttributesTab(tabControl);

            ShowLogin();
        }

        #endregion

        #region UI Control Creators

        private void ShowLogin()
        {
            var panelModal = _uiControlsCreator.GetFillPanel(this);
            var panelLogin = _uiControlsCreator.GetPanel(panelModal);
            var appUriString = _environmentProperties["qa"].ApiUri;
            var _loginRequestWrapper = new LoginRequestWrapper(new Uri(appUriString));
            var loginControl = new UiLoginControl(_loginRequestWrapper);
            loginControl.Initialize("LoginControl", panelLogin, _style);
            loginControl.OnLogin += (credentials) => { panelMain.Visible = true; this.Controls.Remove(panelModal); };
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

            int panelLeftWidth = tabControl.Width / 3;
            (panelLeft, panelRight) = _uiControlsCreator.GetLeftRightPanels(pageAttributes, panelLeftWidth);

            int panelTopHeight = panelRight.Height / 3;

            _uiAccessLevelTree = new UiAccessLevelTree();
            _uiAccessLevelTree.Initialize("AccessLevelTree", panelLeft, _style);

            _uiAccessLevelEdit = new UiAccessLevelEdit();
            _uiAccessLevelEdit.Initialize("AccessLevelEdit", panelRight, _style);

            _uiAccessLevelTree.OnNodeSelect += node => { _uiAccessLevelEdit.SetUiValues(node); };
        }


        #endregion
    }
}
