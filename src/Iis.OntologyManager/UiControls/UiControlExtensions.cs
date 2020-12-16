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
                HeaderText = headerText,
                CellTemplate = new DataGridViewTextBoxCell()
            };
            column.Width *= koef;
            grid.Columns.Add(column);
            return column;
        }

    }
}
