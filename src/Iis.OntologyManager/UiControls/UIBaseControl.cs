using Iis.OntologyManager.Style;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public abstract class UIBaseControl
    {
        protected UiContainerManager _container;
        protected IOntologyManagerStyle _style;
        public Panel MainPanel { get; private set; }
        public bool Visible
        {
            get { return MainPanel.Visible; }
            set { MainPanel.Visible = value; }
        }
        public Panel Initialize(IOntologyManagerStyle style, Panel mainPanel)
        {
            _style = style;
            MainPanel = mainPanel ?? new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = _style.BackgroundColor
            };

            _container = new UiContainerManager(MainPanel, style);

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
