using Iis.OntologyManager.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public abstract class UIBaseControl
    {
        protected UiContainerManager _container;
        protected IOntologyManagerStyle _style;
        protected UiControlsCreator _uiControlsCreator;
        public Panel MainPanel { get; private set; }
        public string Name { get; private set; }
        public int Width => _container.Right;
        public int Height => _container.Bottom;
        public bool Visible
        {
            get { return MainPanel.Visible; }
            set { MainPanel.Visible = value; }
        }
        public Panel Initialize(string name, Panel mainPanel)
        {
            Name = name;
            
            MainPanel = mainPanel ?? new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
            };
            _style = OntologyManagerStyle.GetDefaultStyle(MainPanel);
            _uiControlsCreator = new UiControlsCreator(_style);

            _container = new UiContainerManager(name, MainPanel);
            MainPanel.BackColor = _style.BackgroundColor;

            MainPanel.SuspendLayout();
            CreateControls();
            if (mainPanel == null)
            {
                MainPanel.Width = _container.Right;
                MainPanel.Height = _container.Bottom + _style.MarginVer;
            }
            MainPanel.ResumeLayout();

            return MainPanel;
        }

        protected abstract void CreateControls();

        public void Enable() => MainPanel.Enabled = true;
        public void Disable() => MainPanel.Enabled = false;
        public void SetEnabled(bool enabled) => MainPanel.Enabled = enabled;
    }
}
