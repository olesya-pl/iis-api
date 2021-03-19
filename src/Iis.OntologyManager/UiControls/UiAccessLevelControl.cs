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
        Button btnStartEditing;
        Button btnCancelEditing;
        Button btnCommitEditing;
        INodeTypeLinked _nodeType;
        IOntologyNodesData _ontologyData;
        IAccessLevels _accessLevels;
        bool _editMode = false;

        public UiAccessLevelControl(UiControlsCreator uiControlsCreator, IAccessLevels accessLevels)
        {
            _uiControlsCreator = uiControlsCreator;
        }
        protected override void CreateControls()
        {
            _container.Add(btnStartEditing = new Button { Text = "Почати редактування" });
            btnStartEditing.Click += (sender, e) => StartEditing();

            _container.Add(btnCancelEditing = new Button { Text = "Скасувати зміни" });
            btnCancelEditing.Click += (sender, e) => CancelEditing();

            _container.Add(btnCommitEditing = new Button { Text = "Зберегти зміни" });
            btnCommitEditing.Click += (sender, e) => CommitEditing();

            _container.Add(btnAdd = new Button { Text = "Додати" });

            grid = _uiControlsCreator.GetDataGridView("gridData", null, new List<string> { "Name", "NumericIndex"});
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.ColumnHeadersVisible = true;
            grid.AutoGenerateColumns = false;
            grid.CellFormatting += grid_CellFormatting;

            _container.Add(grid, null, true);
        }
        public void SetUiValues(INodeTypeLinked nodeType, IOntologyNodesData ontologyData)
        {
            _ontologyData = ontologyData;
            BindGrid();
        }
        private void BindGrid()
        {
            _accessLevels = _ontologyData.GetAccessLevels();
            if (_accessLevels != null)
            {
                grid.DataSource = _accessLevels.Items;
            }
            SetControlsState();
        }
        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            var row = grid.Rows[e.RowIndex];

            var color = _style.AttributeTypeBackColor;
            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }
        private void SetControlsState()
        {
            btnAdd.Enabled = _editMode;
            btnStartEditing.Enabled = !_editMode;
            btnCancelEditing.Enabled = _editMode;
            btnCommitEditing.Enabled = _editMode;
        }
        private void StartEditing()
        {
            if (MessageBox.Show(
                "Редактування ціх даних може привести до тяжких наслідків. Ви впевнені що хочете редактувати?", 
                "Попередження", 
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _editMode = true;
                SetControlsState();
            }
        }
        private void CancelEditing()
        {
            if (MessageBox.Show(
                "Ви дійсно хочете скасувати зміни?",
                "Попередження",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _editMode = false;
                BindGrid();
            }
        }
        private void CommitEditing()
        {
            if (MessageBox.Show(
                "Ви дійсно хочете зберегти зміни?",
                "Попередження",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _editMode = false;
            }
        }
        private void ShowEditForm(AccessLevel accessLevel)
        {
            var form = new Form()
            {
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Height = 300,
                StartPosition = FormStartPosition.CenterParent
            };
            var rootPanel = _uiControlsCreator.GetFillPanel(form);
            var editControl = new UiAccessLevelEditControl(accessLevel, _accessLevels.Items.Count);
            editControl.Initialize("AccessLevelEditControl", rootPanel);
            editControl.SetUiValues();
            editControl.OnSave += () =>
            {
                form.Close();
                form.DialogResult = DialogResult.OK;
                //PopulateGrid(nodes);
            };
            form.ShowDialog();
        }
    }
}
