using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public static class UiControlExtensions
    {
        public static DataGridViewColumn AddTextColumn(this DataGridView grid, string name, string headerText, int koef = 1)
        {
            var column = new DataGridViewColumn
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = headerText,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                CellTemplate = new DataGridViewTextBoxCell()
            };
            column.Width *= koef;
            grid.Columns.Add(column);
            return column;
        }

        public static DataGridViewColumn AddCheckBoxColumn(this DataGridView grid, string name, string headerText)
        {
            var column = new DataGridViewColumn
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = headerText,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                CellTemplate = new DataGridViewCheckBoxCell()
            };
            grid.Columns.Add(column);
            return column;
        }

    }
}
