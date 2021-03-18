using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiAccessLevelControl: UIBaseControl, IDataViewControl
    {
        DataGridView grid;
        Button btnAdd;
        INodeTypeLinked _nodeType;
        IOntologyNodesData _ontologyData;
        IAccessLevels _accessLevels;
        public UiAccessLevelControl(UiControlsCreator uiControlsCreator, IAccessLevels accessLevels)
        {
            _uiControlsCreator = uiControlsCreator;
        }
        protected override void CreateControls()
        {
            _container.Add(btnAdd = new Button { Text = "Додати" });
            grid = _uiControlsCreator.GetDataGridView("gridData", null, new List<string>());
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.ColumnHeadersVisible = true;
            grid.AutoGenerateColumns = false;

            _container.Add(grid, null, true);
        }
        public void SetUiValues(INodeTypeLinked nodeType, IOntologyNodesData ontologyData)
        {
            grid.Rows.Clear();
            grid.DataSource = _accessLevels.Items;
        }
    }
}
