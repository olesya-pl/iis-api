using Iis.OntologyManager.Style;
using Serilog;
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
        private int _bottom;
        private int _right;
        public int Right => _right - _rect.Left + _style.MarginHor;
        public int Bottom => _bottom - _rect.Top + _style.MarginVer;
        private int Top { get; set; }

        private int _left;
        private int _colWidth;
        private int TopOfFirst => _rect.Top + _style.MarginVer;
        public bool AutoWidth { get; set; } = true;
        public string Name { get; private set; }
        public Control RootControl => _rootControl;

        public UiContainerManager(string name, Control rootControl, Rectangle? rect = null)
        {
            Name = name;
            _rootControl = rootControl;
            _style = OntologyManagerStyle.GetDefaultStyle(rootControl);
            _rect = rect ?? rootControl.ClientRectangle;
            Top = TopOfFirst;
            _left = _rect.Left + _style.MarginHor;
            _colWidth = _style.ControlWidthDefault;
            Log.Logger.Verbose($"UiContainerManager {name} created on panel {rootControl.Name}");
        }

        public void AddToRoot(Control control)
        {
            _rootControl.Controls.Add(control);
            if (control.Right > _right) _right = control.Right;
            if (control.Bottom > _bottom) _bottom = control.Bottom;
        }

        public Label Add(Control control, string labelText = null, bool stretchToDown = false)
        {
            Log.Logger.Verbose($"{Name} add...");
            Log.Logger.Verbose($"... control type = {control.GetType()}");
            Log.Logger.Verbose($"... labelText = {labelText}");
            Log.Logger.Verbose($"... stretchToDown = {stretchToDown}");

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

            if (control.Left < _left) _left = control.Left;
            if (control.Bottom > _bottom) _bottom = control.Bottom;
            if (control.Right > _right) _right = control.Right;
            
            Top = control.Bottom + _style.MarginVer;
            AddToRoot(control);
            Log.Logger.Verbose($"<= Top = {Top}");
            Log.Logger.Verbose($"<= Bottom = {_bottom}");
            Log.Logger.Verbose($"<= Left = {_left}");
            return label;
        }

        public void AddPanel(Panel panel, AddPanelOptions options = null)
        {
            panel.Top = Top;
            panel.Left = _left;
            panel.Height = _rootControl.Height - panel.Top - _style.MarginVer * 2;
            _left = panel.Right + _style.MarginHor;
            Top = TopOfFirst;
            //panel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            //if (options.Width != null) panel.Width = (int)options.Width;
            //if (options.Height != null) panel.Height = (int)options.Height;
            //if (options.FitWidth)
            //{
            //    if (options.Width == null) panel.Width = _rootControl.Width - _style.MarginHor * 2;
            //    panel.Anchor |= AnchorStyles.Right;
            //}
            //if (options.FitHeight)
            //{
            //    if (options.Height == null) panel.Height = _rootControl.Height - _style.MarginVer * 2;
            //    panel.Anchor |= AnchorStyles.Bottom;
            //}

            //if (options.FitWidth)
            //{
            //    _left = _style.MarginHor;
            //    Top = panel.Bottom + _style.MarginVer;
            //}
            //else
            //{
            //    _left = 
            //}
            AddToRoot(panel);
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
