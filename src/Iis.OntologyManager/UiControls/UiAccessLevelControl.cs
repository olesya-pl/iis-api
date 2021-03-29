using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.IisAccessLevels;
using Iis.OntologyManager.DTO;
using Iis.OntologyManager.Helpers;
using Iis.Services.Contracts.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        ContextMenuStrip _menu;
        IOntologyNodesData _ontologyData;
        AccessLevels _accessLevels;
        bool _editMode = false;
        Dictionary<Guid, Guid> _deletedMappings = new Dictionary<Guid, Guid>();
        public event Func<ChangeAccessLevelsParams, Task<RequestResult>> OnSave;

        DataGridViewRow SelectedRow => grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;
        AccessLevel SelectedItem => SelectedRow == null ? null : (AccessLevel)SelectedRow.DataBoundItem;

        public UiAccessLevelControl(UiControlsCreator uiControlsCreator)
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
            btnAdd.Click += (sender, e) => { AddNewItem(); };

            grid = _uiControlsCreator.GetDataGridView("gridData", null, new List<string> { });
            grid.AddTextColumn("Name", "Назва", 4);
            grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns[0].Width *= 4;
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.ColumnHeadersVisible = true;
            grid.AutoGenerateColumns = false;
            grid.CellFormatting += grid_CellFormatting;
            grid.DoubleClick += (sender, e) => { EditSelectedItem(); };

            _menu = new ContextMenuStrip();
            _menu.Items.Add("Підняти вгору");
            _menu.Items[0].Click += (sender, e) => { MoveUp(); };
            _menu.Items.Add("Опустити вниз");
            _menu.Items[1].Click += (sender, e) => { MoveDown(); };
            _menu.Items.Add("Знищити");
            _menu.Items[2].Click += (sender, e) => { RemoveItem(); };
            grid.ContextMenuStrip = _menu;

            _container.Add(grid, null, true);
        }
        public void SetUiValues(INodeTypeLinked nodeType, IOntologyNodesData ontologyData)
        {
            _ontologyData = ontologyData;
            _accessLevels = (AccessLevels)_ontologyData.GetAccessLevels();
            BindGrid();
        }
        private void BindGrid()
        {
            grid.DataSource = null;
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
            _menu.Enabled = _editMode;
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
                SetControlsState();
                _accessLevels = (AccessLevels)_ontologyData.GetAccessLevels();
                BindGrid();
            }
        }
        private void CommitEditing()
        {
            if (MessageBox.Show(
                "Ця операція може тривати багато часу. Ви дійсно хочете зберегти зміни?",
                "Попередження",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var param = new ChangeAccessLevelsParams
                {
                    AccessLevelList = _accessLevels.OriginalItems.ToList(),
                    DeletedMappings = _deletedMappings
                };
                OnSave.Invoke(param);
                //_ontologyData.SaveAccessLevels(_accessLevels);
                _editMode = false;
                SetControlsState();
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
                if (accessLevel == null)
                {
                    _accessLevels.Add(new AccessLevel(Guid.NewGuid(), editControl.AccessLevelName, _accessLevels.Items.Count));
                }
                else
                {
                    _accessLevels.ChangeName(SelectedItem.Id, editControl.AccessLevelName);
                }
                
                form.Close();
                form.DialogResult = DialogResult.OK;
                BindGrid();
            };
            form.ShowDialog();
        }
        private void EditSelectedItem()
        {
            if (SelectedItem != null)
                ShowEditForm(SelectedItem);
        }
        private int GetIndex(AccessLevel accessLevel)
        {
            for (int i = 0; i < _accessLevels.Items.Count; i++)
            {
                if (_accessLevels.Items[i] == accessLevel)
                    return i;
            }
            return -1;
        }
        private void MoveUp()
        {
            if (SelectedItem == null) return;
            var item = SelectedItem;
            var index = GetIndex(item);
            if (index > 0)
            {
                _accessLevels.SwapItems(index, index - 1);
            }
            BindGrid();
        }
        private void MoveDown()
        {
            if (SelectedItem == null) return;
            var item = SelectedItem;
            var index = GetIndex(item);
            if (index < _accessLevels.Items.Count - 1)
            {
                _accessLevels.SwapItems(index, index + 1);
            }
            BindGrid();
        }
        private void AddNewItem()
        {
            ShowEditForm(null);
        }
        private void RemoveItem()
        {
            var deletedItem = SelectedItem;
            if (deletedItem == null) return;
            if (MessageBox.Show($"Ви дійсно хочете знищити { deletedItem.Name }? Треба буде вибрати чим його заменити", "Попередження", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var items = new List<IAccessLevel>(_accessLevels.Items);
                items.Remove(deletedItem);
                var item = _uiControlsCreator.ChooseFromModalComboBox(items, "Name");
                if (item == null) return;

                foreach (var key in _deletedMappings.Keys)
                {
                    if (_deletedMappings[key] == deletedItem.Id)
                    {
                        _deletedMappings[key] = item.Id;
                    }
                }
                _deletedMappings[deletedItem.Id] = item.Id;

                _accessLevels.Remove(SelectedItem);
                BindGrid();
            }
        }
    }
}
