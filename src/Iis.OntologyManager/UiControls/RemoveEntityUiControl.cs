using System;
using System.Linq;
using System.Drawing;
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
        private Label _invalidBaseAppAdressLabel;
        private DataGridView _resultGrid;
        private readonly SchemaDataSource _schemaDataSource;
        private INode _selectedNode;
        private IOntologyNodesData _data;
        private bool _baseAppAdressIsValid;

        public event Action<Guid> OnRemove;
        public event Func<IOntologyNodesData> OnGetOntologyData;

        DataGridViewRow SelectedRow => _resultGrid?.SelectedRows.Count > 0 ? _resultGrid.SelectedRows[0] : null;

        public RemoveEntityUiControl(SchemaDataSource schemaDataSource)
        {
            _schemaDataSource = schemaDataSource;

            _baseAppAdressIsValid = Uri.IsWellFormedUriString(_schemaDataSource.AppAddress, UriKind.Absolute);
        }

        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 200);
            var container = new UiContainerManager("RemoveEntityOptions", panels.panelTop, _style.Common);
            var bottomContainer = new UiContainerManager("RemoveEntityResult", panels.panelBottom, _style.Common);

            container.SetFullWidthColumn();

            container.Add(_entityIdSearch = new TextBox(), "Введіть ідентифікатор сутності");
            container.Add(_searchButton = new Button());
            container.Add(_removeButton = new Button());
            container.Add(_foundNodeLabel = new Label(), FoundNodeText);

            if (!_baseAppAdressIsValid)
            {
                container.Add(_invalidBaseAppAdressLabel = new Label(), $"Invalid web application url '{_schemaDataSource.AppAddress}'. Please check configuration.");
                SetupInvalidBaseAppAdressLabel(_invalidBaseAppAdressLabel);
            }

            SetupActionButtonAsSearch(_searchButton);
            SetupActionButtonAsRemove(_removeButton);
            SetupTextBox(_entityIdSearch);
            SetupFoundNodeLabel(_foundNodeLabel);

            _resultGrid = _uiControlsCreator.GetDataGridView("gridRelationsResult", null, new List<string>());
            _resultGrid.Width = panels.panelBottom.Width;

            SetupGridView(_resultGrid);

            bottomContainer.SetFullWidthColumn();
            bottomContainer.Add(_resultGrid, "Вхідні зв'язкі:", true);
        }

        private void TextChanged(object sender, EventArgs e)
        {
            _searchButton.Enabled = !string.IsNullOrWhiteSpace((sender as TextBox).Text);

            _removeButton.Enabled = false;
            
            _selectedNode = null;

            _foundNodeLabel.Text = string.Empty;

            if (_resultGrid.Rows.Count > 0)
            {
                _resultGrid.Rows.Clear();
            }
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

            if (!CouldBeDeleted(_selectedNode))
            {
                MessageBox.Show("Сутність з таким ідентифікатором не може бути зницена!");
                return;
            }

            var title = GetNodeTitle(_selectedNode);

            _foundNodeLabel.Text = $"тип {_selectedNode.NodeType.Title} з назвою '{title}' (ідентифікатор {_selectedNode.Id})";

            var incomingRelations = _selectedNode.IncomingRelations.Select(e => MapToDTO(e, _schemaDataSource.AppAddress)).ToArray();

            PopulateData(_resultGrid, incomingRelations);

            _removeButton.Enabled = true;
        }

        private void FoundNodeDoubleClick(object sender, EventArgs e)
        {
            if (_selectedNode is null) return;

            if (!_baseAppAdressIsValid) return;

            var url = GenerateUrlForNode(_selectedNode, _schemaDataSource.AppAddress);

            OpenUrl(url);
        }

        private void RemoveClick(object sender, EventArgs e)
        {
            if (OnRemove is null) return;

            if (_selectedNode is null) return;

            var title = _selectedNode.GetComputedValue("__title");

            var messageResult = MessageBox.Show(
                    $"Ви впевнені що бажаєте видалити сутність типа {_selectedNode.NodeType.Title} з назвою '{title}' (ідентифікатор {_selectedNode.Id})",
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

        private bool CouldBeDeleted(INode node)
        {
            return !node.IsArchived && (node.NodeType.IsEvent || node.NodeType.IsObject);
        }

        private string GetNodeTitle(INode node)
        {
            if (node.NodeType.IsEvent)
            {
                return node.OutgoingRelations.FirstOrDefault(node => node.TypeName == "name")?.TargetNode.Value;
            }
            return node.GetComputedValue("__title");
        }

        private Button SetupActionButtonAsRemove(Button actionButton)
        {
            if (actionButton is null) return actionButton;

            actionButton.Click += RemoveClick;
            actionButton.Text = RemoveEntityBtnCaption;
            actionButton.Enabled = false;
            actionButton.Width = _style.Common.ButtonWidthDefault;

            return actionButton;
        }

        private Button SetupActionButtonAsSearch(Button actionButton)
        {
            if (actionButton is null) return actionButton;

            actionButton.Click += SearchClick;
            actionButton.Text = FindEntityBtnCaption;
            actionButton.Enabled = false;
            actionButton.Width = _style.Common.ButtonWidthDefault;

            return actionButton;
        }

        private TextBox SetupTextBox(TextBox textBox)
        {
            textBox.TextChanged += TextChanged;
            textBox.Width = _style.Common.ButtonWidthDefault;

            return textBox;
        }

        private Label SetupFoundNodeLabel(Label label)
        {
            if (_baseAppAdressIsValid)
            {
                label.Cursor = Cursors.Hand;
            }

            label.Font = new Font(label.Font, FontStyle.Underline);

            label.DoubleClick += FoundNodeDoubleClick;

            return label;
        }

        private Label SetupInvalidBaseAppAdressLabel(Label label)
        {
            label.Font = new Font(label.Font, FontStyle.Bold);

            return label;
        }

        private DataGridView SetupGridView(DataGridView grid)
        {
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.ColumnHeadersVisible = true;
            grid.AutoGenerateColumns = false;
            grid.AllowUserToAddRows = false;
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

            if (!_baseAppAdressIsValid) return;

            OpenUrl(url);
        }

        private RelationDTO MapToDTO(IRelation relation, string appAddress)
        {
            return new RelationDTO
            {
                TypeName = $"{relation.RelationTypeName}/{relation.Node.NodeType.Title}",
                NodeId = relation.SourceNode.Id,
                NodeTitle = relation.SourceNode.GetComputedValue("__title"),
                NodeTypeName = $"{relation.SourceNode.NodeType.Name}/{relation.SourceNode.NodeType.Title}",
                NodeUrl = GenerateUrlForNode(relation.SourceNode, appAddress)
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

        private string GenerateUrlForNode(INode node, string baseUrl)
        {
            if (node is null) return null;

            var url = node switch
            {
                INode {NodeType: {IsEvent : true } } => $"{baseUrl}/events/?page=1&id={node.Id:N}",
                _ => $"{baseUrl}/objects/Entity{node.NodeType.Name}.{node.Id:N}/view"
            };

            return url.Replace("&", "^&");
        }

        private void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
    }
}
