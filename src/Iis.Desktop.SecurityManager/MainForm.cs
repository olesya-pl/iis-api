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

namespace Iis.Desktop.SecurityManager
{
    public partial class MainForm : Form
    {
        #region Fields

        private IConfiguration _configuration;
        //private ISecurityManagerStyle _appStyle;
        private IDesktopStyle _style;
        private const string VERSION = "0.1";
        private ILogger _logger;
        private UiControlsCreator _uiControlsCreator;
        private Panel panelMain;
        private Panel panelLeft;
        private Panel panelRight;
        private Panel panelTop;
        private Panel panelBottom;

        #endregion

        #region Properties and Constructors

        public MainForm(
            IConfiguration configuration,
            ILogger logger,
            IDesktopStyleFactory desktopStyleFactory)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            Text = $"Володар Таємниць {VERSION}";

            _configuration = configuration;
            _style = desktopStyleFactory.GetDefaultStyle(this);
            _logger = logger;
            _uiControlsCreator = new UiControlsCreator(_style);

            panelMain = _uiControlsCreator.GetFillPanel(this);

            int panelLeftWidth = panelMain.Width / 3;
            (panelLeft, panelRight) = _uiControlsCreator.GetLeftRightPanels(panelMain, panelLeftWidth);

            int panelTopHeight = panelRight.Height / 5;
            (panelTop, panelBottom) = _uiControlsCreator.GetTopBottomPanels(panelRight, panelTopHeight);
            ShowLogin();
        }

        #endregion

        #region UI Control Creators

        private void ShowLogin()
        {
            var panelModal = _uiControlsCreator.GetFillPanel(this);
            var panelLogin = _uiControlsCreator.GetPanel(panelModal);
            var loginControl = new UiLoginControl();
            loginControl.Initialize("LoginControl", panelLogin, _style);
            loginControl.OnLogin += (credentials) => { panelMain.Visible = true; this.Controls.Remove(panelModal); };
            panelLogin.Width = loginControl.Width;
            panelLogin.Height = loginControl.Height;
            Center(panelLogin);
            panelLogin.BorderStyle = BorderStyle.FixedSingle;
            panelMain.Visible = false;
        }

        private void Center(Control control)
        {
            var parent = control.Parent;
            if (parent == null) return;
            if (control.Width < parent.Width)
            {
                control.Left = (parent.Width - control.Width) / 2;
            }
            if (control.Height < parent.Height)
            {
                control.Top = (parent.Height - control.Height) / 2;
            }
        }

        #endregion
    }
}
