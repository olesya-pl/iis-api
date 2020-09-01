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
        public Panel MainPanel { get; private set; }
        public string Name { get; private set; }
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
    }
}
