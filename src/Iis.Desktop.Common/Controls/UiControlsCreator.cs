using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Iis.OntologyManager.Style;

namespace Iis.OntologyManager.UiControls
{
    public class UiControlsCreator
    {
        IDesktopStyle _style;
        public UiControlsCreator(IDesktopStyle style)
        {
            _style = style;
        }

        public void SetGridTypesStyle(DataGridView grid)
        {
            grid.ColumnCount = 1;
            grid.Columns[0].DataPropertyName = "Name";
            grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.AllowUserToResizeRows = false;
            grid.AutoGenerateColumns = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.ForeColor = Color.Black;
        }

        public DataGridView GetDataGridView(string name, Point? location, List<string> dataNames)
        {
            var grid = new DataGridView
            {
                AutoGenerateColumns = false,
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                Dock = DockStyle.None,
                Name = name,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                ReadOnly = true,
                ColumnCount = dataNames.Count,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = _style.BackgroundColor
            };
            if (location != null)
            {
                grid.Location = (Point)location;
            }
            grid.DefaultCellStyle.SelectionBackColor = grid.DefaultCellStyle.BackColor;
            grid.DefaultCellStyle.SelectionForeColor = grid.DefaultCellStyle.ForeColor;

            for (int i = 0; i < dataNames.Count; i++)
            {
                grid.Columns[i].DataPropertyName = dataNames[i];
                grid.Columns[i].HeaderText = dataNames[i];
                grid.Columns[i].Name = dataNames[i];
            }
            return grid;
        }

        public (Panel panelTop, Panel panelBottom) GetTopBottomPanels(Panel rootPanel, int topPanelHeight, int margin = 0)
        {
            var panelTop = new Panel
            {
                Name = $"{rootPanel.Name}_Top",
                Location = new Point(margin, margin),
                Size = new Size(rootPanel.Width - margin * 2, topPanelHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = rootPanel.BackColor
            };

            var panelBottom = new Panel
            {
                Name = $"{rootPanel.Name}_Bottom",
                Location = new Point(margin, panelTop.Bottom + margin),
                Size = new Size(rootPanel.Width - margin * 2, rootPanel.Height - panelTop.Bottom - margin * 4),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = rootPanel.BackColor
            };
            rootPanel.Controls.Add(panelTop);
            rootPanel.Controls.Add(panelBottom);

            return (panelTop, panelBottom);
        }

        public (Panel panelLeft, Panel panelRight) GetLeftRightPanels(Panel rootPanel, int leftPanelWidth, int margin = 0)
        {
            var panelLeft = new Panel
            {
                Name = $"{rootPanel.Name}_Left",
                Location = new Point(margin, margin),
                Size = new Size(leftPanelWidth, rootPanel.Height - margin * 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = rootPanel.BackColor
            };

            var panelRight = new Panel
            {
                Name = $"{rootPanel.Name}_Right",
                Location = new Point(panelLeft.Width + margin, margin),
                Size = new Size(rootPanel.Width - panelLeft.Right - margin * 4, rootPanel.Height - margin * 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = rootPanel.BackColor
            };
            rootPanel.Controls.Add(panelLeft);
            rootPanel.Controls.Add(panelRight);

            return (panelLeft, panelRight);
        }

        public void UpdateComboSource(ComboBox comboBox, IEnumerable<object> source)
        {
            var selectedText = comboBox.Text;
            comboBox.DataSource = source;
            var index = comboBox.FindStringExact(selectedText);
            if (!string.IsNullOrEmpty(selectedText) && index > -1)
            {
                comboBox.SelectedIndex = index;
            }
            else
            {
                comboBox.SelectedIndex = 0;
            }
        }

        public T ChooseFromModalComboBox<T>(IEnumerable<T> items, string dataPropertyName) where T : class
        {
            var form = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                Width = _style.ControlWidthDefault + _style.MarginHor * 2,
                StartPosition = FormStartPosition.CenterParent
            };
            var rootPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _style.BackgroundColor
            };
            form.Controls.Add(rootPanel);
            var container = new UiContainerManager("ModalComboBox", rootPanel, _style);
            var comboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = dataPropertyName,
                BackColor = rootPanel.BackColor
            };

            var src = new List<T>(items);
            comboBox.DataSource = src;
            container.Add(comboBox);
            var btnOk = new Button { Text = "Ok", DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            container.AddInRow(new List<Control> { btnOk, btnCancel });
            form.Height = container.Bottom;
            return form.ShowDialog() == DialogResult.OK ? (T)comboBox.SelectedItem : null;
        }

        public Panel GetFillPanel(Control parent, bool visible = true)
        {
            var marginVer = 20;
            var panel = new Panel
            {
                Parent = parent,
                Top = marginVer,
                Left = 0,
                Width = parent.Width,
                Height = parent.Height - marginVer,
                Dock = DockStyle.None,
                BackColor = Color.OrangeRed,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                Visible = visible
            };
            parent.Controls.Add(panel);
            return panel;
        }
        public ComboBox GetEnumComboBox(Type enumType)
        {
            var comboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Value",
                BackColor = _style.BackgroundColor,
                DataSource = Enum.GetValues(enumType)
            };

            return comboBox;
        }

        public void SetSelectedValue(ComboBox comboBox, string value)
        {
            comboBox.SelectedIndex = -1;

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i].ToString() == value)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }
        }
        public Form GetModalForm(Form parent)
        {
            return new Form()
            {
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Width = parent.Width - 20,
                Height = parent.Height - 20,
                StartPosition = FormStartPosition.CenterParent
            };
        }

        public CheckBox GetCheckBox(string text, bool isChecked) =>
            new CheckBox { Text = text, Checked = isChecked, MinimumSize = new Size { Height = _style.CheckboxHeightDefault } };

        public Button GetButton(string text) =>
            new Button
            {
                Text = "Порівняти", 
                Width = _style.ButtonWidthDefault,
                MinimumSize = new Size { Height = _style.ButtonHeightDefault } 
            };
    }
}
