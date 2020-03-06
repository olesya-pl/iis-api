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
        private int _bottom;
        private int _right;
        public int Width => _right - _rect.Left + _style.MarginHor;
        public int Height => _bottom - _rect.Top + _style.MarginVer;
        private int Top { get; set; }

        private int _left;
        private int _colWidth;
        private int TopOfFirst => _rect.Top + _style.MarginVer * 2;
        public bool AutoWidth { get; set; } = true;

        public UiContainerManager(Control rootControl, IOntologyManagerStyle style, Rectangle? rect = null)
        {
            _rootControl = rootControl;
            _style = style;
            _rect = rect ?? rootControl.ClientRectangle;
            Top = TopOfFirst;
            _left = _rect.Left + _style.MarginHor;
            _colWidth = _style.ControlWidthDefault;
        }

        public void AddToRoot(Control control)
        {
            _rootControl.Controls.Add(control);
            if (control.Right > _right) _right = control.Right;
            if (control.Bottom > _bottom) _bottom = control.Bottom;
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
                AddToRoot(label);
                Top += label.Height;
            }

            control.Left = _left;
            control.Top = Top;

            if (stretchToDown)
            {
                control.Height = _bottom > control.Top ? 
                    _bottom - control.Top  :
                    _rect.Height - control.Top;
            }

            if (AutoWidth)
            {
                control.Width = _colWidth;
            }
            Top = control.Bottom + _style.MarginVer;
            AddToRoot(control);
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

        public void SetLeft(int left)
        {
            _left = left;
        }

        public void GoToBottom()
        {
            _left = _style.MarginHor;
            Top = _bottom;
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
            var maxbottom = Top;
            foreach (var control in controls)
            {
                control.Left = left;
                control.Top = Top;
                control.Width = width;
                AddToRoot(control);
                left += width + _style.MarginHor;

                if (maxbottom < control.Bottom)
                {
                    maxbottom = control.Bottom;
                }
            }
            Top = maxbottom + _style.MarginVer;
        }
    }
}
