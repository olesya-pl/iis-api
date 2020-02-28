using Iis.OntologyManager.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiControlsCreator
    {
        IOntologyManagerStyle _style;
        public UiControlsCreator(IOntologyManagerStyle style)
        {
            _style = style;
        }

        public void SetGridTypesStyle(DataGridView grid)
        {
            grid.ColumnCount = 1;
            grid.Columns[0].DataPropertyName = "Name";
            grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.ForeColor = Color.Black;
        }

        public DataGridView GetDataGridView(string name, Point location, List<string> dataNames)
        {
            var grid = new DataGridView
            {
                AutoGenerateColumns = false,
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                Dock = DockStyle.None,
                Location = location,
                Name = name,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                ReadOnly = true,
                ColumnCount = dataNames.Count,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = _style.BackgroundColor
            };
            grid.DefaultCellStyle.SelectionBackColor = grid.DefaultCellStyle.BackColor;
            grid.DefaultCellStyle.SelectionForeColor = grid.DefaultCellStyle.ForeColor;

            for (int i = 0; i < dataNames.Count; i++)
            {
                grid.Columns[i].DataPropertyName = dataNames[i];
                grid.Columns[i].HeaderText = dataNames[i];
            }
            return grid;
        }

        public (Panel panelTop, Panel panelBottom) GetTopBottomPanels(Panel rootPanel, int topPanelWidth, int margin = 0)
        {
            var panelTop = new Panel
            {
                Location = new Point(margin, margin),
                Size = new Size(rootPanel.Width - margin * 2, topPanelWidth),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = rootPanel.BackColor
            };

            var panelBottom = new Panel
            {
                Location = new Point(margin, panelTop.Bottom + margin),
                Size = new Size(rootPanel.Width - margin * 2, rootPanel.Height - panelTop.Bottom - margin*2),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = rootPanel.BackColor
            };
            rootPanel.Controls.Add(panelTop);
            rootPanel.Controls.Add(panelBottom);

            return (panelTop, panelBottom);
        }
    }
}
