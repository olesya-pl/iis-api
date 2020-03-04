using Iis.OntologyManager.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiContainerManager
    {
        private Control _rootControl;
        private IOntologyManagerStyle _style;
        private Rectangle _rect;
        private int _top;
        public int MaxTop { get; private set; }
        private int Top {
            get 
            {
                return _top;
            }
            set 
            {
                _top = value;
                if (_top > MaxTop)
                {
                    MaxTop = _top;
                }
            }
        }

        private int _left;
        private int _colWidth;
        private int TopOfFirst => _style.MarginVer * 2;

        public bool AutoWidth { get; set; } = true;
        

        public UiContainerManager(Control rootControl, IOntologyManagerStyle style, Rectangle? rect = null)
        {
            _rootControl = rootControl;
            _style = style;
            _rect = rect ?? rootControl.ClientRectangle;
            Top = TopOfFirst;
            _left = _style.MarginHor;
            _colWidth = _style.ControlWidthDefault;
        }

        public Label Add(Control control, string labelText = null, bool stretchToDown = false)
        {
            Label label = null;
            if (!string.IsNullOrEmpty(labelText))
            {
                label = new Label
                {
                    Left = _left,
                    Top = Top,
                    Width = _colWidth,
                    Text = labelText
                };
                _rootControl.Controls.Add(label);
                Top += label.Height;
            }

            control.Left = _left;
            control.Top = Top;

            if (stretchToDown)
            {
                control.Height = MaxTop > control.Top ? 
                    MaxTop - control.Top  :
                    _rect.Height - control.Top;
            }

            if (AutoWidth)
            {
                control.Width = _colWidth;
            }
            Top = control.Bottom + _style.MarginVer;
            _rootControl.Controls.Add(control);
            return label;
        }

        public void GoToNewColumn(int colWidth = default)
        {
            Top = TopOfFirst;
            _left += _colWidth + _style.MarginHor;

            if (colWidth != default)
            {
                _colWidth = colWidth;
            }
        }

        public void GoToBottom()
        {
            _left = _style.MarginHor;
            Top = MaxTop;
        }

        public void StepDown(int cnt = 1)
        {
            Top += _style.MarginVer * cnt;
        }

        public void SetFullWidthColumn()
        {
            _colWidth = _rect.Width - _style.MarginHor * 2;
        }

        public void SetColWidth(int width)
        {
            _colWidth = width;
        }

        public void AddInRow(IReadOnlyList<Control> controls)
        {
            if (controls.Count == 0) return;
            var width = (_colWidth - (controls.Count - 1) * _style.MarginHor) / controls.Count;
            var left = _left;
            foreach (var control in controls)
            {
                control.Left = left;
                control.Top = Top;
                control.Width = width;
                _rootControl.Controls.Add(control);
                left += width + _style.MarginHor;
            }
        }
    }
}
