using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using Iis.Interfaces.Ontology.Data;
using Iis.OntologyManager.DTO;
using Iis.OntologyManager.Configurations;
using System.Diagnostics;

namespace Iis.OntologyManager.UiControls
{
    public class RemoveEntityUiControl : UIBaseControl
    {
        private const string FindEntityBtnCaption = "Шукати сутність";
        private const string RemoveEntityBtnCaption = "Видалити сутність";
        private const string FoundNodeText = "Знайдена сутність: ";
        private TextBox _entityIdSearch;
        private Button _searchButton;
        private Button _removeButton;
        private Label _foundNodeLabel;
        private DataGridView _resultGrid;
        private SchemaDataSource _schemaDataSource;
        private INode _selectedNode;
        private IOntologyNodesData _data;

        public event Action<Guid> OnRemove;
        public event Func<IOntologyNodesData> OnGetOntologyData;

        DataGridViewRow SelectedRow => _resultGrid?.SelectedRows.Count > 0 ? _resultGrid.SelectedRows[0] : null;

        public RemoveEntityUiControl(SchemaDataSource schemaDataSource)
        {
            _schemaDataSource = schemaDataSource;
        }
        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 200);
            var container = new UiContainerManager("RemoveEntityOptions", panels.panelTop);
            var bottomContainer = new UiContainerManager("RemoveEntityResult", panels.panelBottom);

            container.SetColWidth(750);

            container.Add(_entityIdSearch = new TextBox(), "Введіть ідентифікатор сутності");
            container.Add(_searchButton = new Button());
            container.Add(_removeButton = new Button());
            container.Add(_foundNodeLabel = new Label { Text = FoundNodeText });

            SetupActionButtonAsSearch(_searchButton);
            SetupActionButtonAsRemove(_removeButton);
            SetupTextBox(_entityIdSearch);

            _resultGrid = _uiControlsCreator.GetDataGridView("gridRelationsResult", null, new List<string>());
            _resultGrid.Width = panels.panelBottom.Width;

            SetupGridView(_resultGrid);

            bottomContainer.SetFullWidthColumn();
            bottomContainer.Add(_resultGrid, null, true);


        }

        private void TextChanged(object sender, EventArgs e)
        {
            _searchButton.Enabled = !string.IsNullOrWhiteSpace((sender as TextBox).Text);

            _removeButton.Enabled = false;
            
            _selectedNode = null;
        }

        private void SearchClick(object sender, EventArgs e)
        {
            var result = TryGetNodeIdentity(_entityIdSearch.Text);

            if (result is null)
            {
                MessageBox.Show("Введення значення не відповідає формату ідентифікатора!");

                return;
            }

            _selectedNode = GetNodeByIdentity(result.Value, true);

            if (_selectedNode is null)
            {
                MessageBox.Show("Не має сутності з таким ідентифікатором!");
                return;
            }

            var title = _selectedNode.GetComputedValue("__title");

            _foundNodeLabel.Text = $"{FoundNodeText}'{title}' з ідентифікатором {_selectedNode.Id}";

            var incomingRelations = _selectedNode.IncomingRelations.Select(e => MapToDTO(e, _schemaDataSource.AppAddress)).ToArray();

            PopulateData(_resultGrid, incomingRelations);

            _removeButton.Enabled = true;
        }

        private void RemoveClick(object sender, EventArgs e)
        {
            if (OnRemove is null) return;

            if (_selectedNode is null) return;

            var title = _selectedNode.GetComputedValue("__title");

            var messageResult = MessageBox.Show(
                    $"Ви впевнені що бажаєте видалити сутність {title} з ідентифікатором {_selectedNode.Id}",
                    "Підтвердження видалення",
                    MessageBoxButtons.YesNo);

            if (messageResult == DialogResult.Yes)
            {
                OnRemove(_selectedNode.Id);
            }
        }

        private Guid? TryGetNodeIdentity(string identityText)
        {
            if (Guid.TryParse(identityText, out Guid nodeIdentity))
            {
                return nodeIdentity;
            }
            else
            {
                return null;
            }
        }

        private INode GetNodeByIdentity(Guid identity, bool reloadData = false)
        {
            if (reloadData)
            {
                _data = OnGetOntologyData();
            }

            return _data.GetNode(identity);
        }

        private Button SetupActionButtonAsRemove(Button actionButton)
        {
            if (actionButton is null) return actionButton;

            actionButton.Click += RemoveClick;
            actionButton.Text = RemoveEntityBtnCaption;
            actionButton.Enabled = false;
            actionButton.Width = _style.ButtonWidthDefault;

            return actionButton;
        }

        private Button SetupActionButtonAsSearch(Button actionButton)
        {
            if (actionButton is null) return actionButton;

            actionButton.Click += SearchClick;
            actionButton.Text = FindEntityBtnCaption;
            actionButton.Enabled = false;
            actionButton.Width = _style.ButtonWidthDefault;

            return actionButton;
        }

        private TextBox SetupTextBox(TextBox textBox)
        {
            textBox.TextChanged += TextChanged;
            textBox.Width = _style.ButtonWidthDefault;

            return textBox;
        }

        private DataGridView SetupGridView(DataGridView grid)
        {
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.ColumnHeadersVisible = true;
            grid.AutoGenerateColumns = false;

            ConfigureGridColumns(grid);

            grid.DoubleClick += GridDoubleClick;
            grid.CellFormatting += GridCellFormatting;
            return grid;
        }

        private void GridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;

            var row = grid.Rows[e.RowIndex];

            var color = row.Index % 2 == 1 ? _style.RelationTypeBackColor : _style.AttributeTypeBackColor;
            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }

        private void GridDoubleClick(object sender, EventArgs e)
        {
            var url = SelectedRow.Cells[_resultGrid.Columns["NodeUrl"].Index].Value?.ToString();

            if (url == null) return;

            url = url.Replace("&", "^&");
            
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private RelationDTO MapToDTO(IRelation relation, string appAddress)
        {
            return new RelationDTO
            {
                TypeName = $"{relation.RelationTypeName}/{relation.Node.NodeType.Title}",
                NodeId = relation.SourceNode.Id,
                NodeTitle = relation.SourceNode.GetComputedValue("__title"),
                NodeTypeName = $"{relation.SourceNode.NodeType.Name}/{relation.SourceNode.NodeType.Title}",
                NodeUrl = $"{appAddress}/objects/Entity{relation.SourceNode.NodeType.Name}.{relation.SourceNode.Id:N}/view"
            };
        }

        private void ConfigureGridColumns(DataGridView grid)
        {
            grid.Columns.Clear();

            grid.AddTextColumn("TypeName", "назва відносини", 2);
            grid.AddTextColumn("NodeTypeName", "тип сутності", 2);
            grid.AddTextColumn("NodeTitle", "назва сутності", 2);
            grid.AddTextColumn("NodeUrl", "посилання на сутность", 2);
        }

        private void PopulateData(DataGridView grid, IReadOnlyCollection<RelationDTO> relationList)
        {
            grid.Rows.Clear();

            foreach (var relation in relationList)
            {
                var row = new DataGridViewRow();

                row.CreateCells(grid);

                row.Cells[grid.Columns["TypeName"].Index].Value = relation.TypeName;
                row.Cells[grid.Columns["NodeTypeName"].Index].Value = relation.NodeTypeName;
                row.Cells[grid.Columns["NodeTitle"].Index].Value = relation.NodeTitle;
                row.Cells[grid.Columns["NodeUrl"].Index].Value = relation.NodeUrl;

                grid.Rows.Add(row);
            }
        }
    }
}
