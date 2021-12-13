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

using Iis.Services.Contracts.Interfaces;

namespace Iis.Desktop.SecurityManager
{
    public partial class MainForm : Form
    {
        #region Fields

        IConfiguration _configuration;
        ISecurityManagerStyle _style;
        Panel pnlTop;
        const string VERSION = "0.01";
        ILogger _logger;

        #endregion

        #region Properties and Constructors

        public MainForm(
            IConfiguration configuration,
            ILogger logger)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            Text = $"Володар Онтології {VERSION}";

            _configuration = configuration;
            _style = SecurityManagerStyle.GetDefaultStyle(this);
            _logger = logger;

            SuspendLayout();
            SetBackColor();
            SetControlsTabMain(panelRight);
            ResumeLayout();
        }

        #endregion

        #region UI Control Creators
        private void SetBackColor()
        {
            panelTop.BackColor = _style.BackgroundColor;
            panelMain.BackColor = _style.BackgroundColor;
            panelLeft.BackColor = _style.BackgroundColor;
            panelRight.BackColor = _style.BackgroundColor;
        }

        private void SetControlsTabMain(Panel rootPanel)
        {
            pnlTop = new Panel();
            pnlTop.Location = new Point(0, 0);
            pnlTop.Size = new Size(rootPanel.Width, _style.ButtonHeightDefault * 2);
            pnlTop.BorderStyle = BorderStyle.FixedSingle;
            pnlTop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var pnlBottom = new Panel();
            pnlBottom.Location = new Point(0, 51);
            pnlBottom.Size = new Size(rootPanel.Width, rootPanel.Height - 51);
            pnlBottom.BorderStyle = BorderStyle.FixedSingle;
            pnlBottom.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rootPanel.SuspendLayout();
            rootPanel.Controls.Add(pnlTop);
            rootPanel.Controls.Add(pnlBottom);
            rootPanel.ResumeLayout();
        }

        private void SetEnabled(bool enabled)
        {
        }

        #endregion
    }
}
