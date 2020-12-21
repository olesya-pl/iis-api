using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Iis.Interfaces.Ontology.Data;

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

        private INode _selectedNode;
        private IOntologyNodesData _data;

        public event Action<Guid> OnRemove;
        public event Func<IOntologyNodesData> OnGetOntologyData;

        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 200);
            var container = new UiContainerManager("RemoveEntityOptions", panels.panelTop);
            var bottomContainer = new UiContainerManager("RemoveEntityResult", panels.panelBottom);

            container.SetColWidth(750);

            var resultContainer = new UiContainerManager("RemoveEntityResults", panels.panelTop);

            container.Add(_entityIdSearch = new TextBox(), "Введіть ідентифікатор сутності");
            container.Add(_searchButton = new Button());
            container.Add(_removeButton = new Button());
            container.Add(_foundNodeLabel = new Label { Text = FoundNodeText });

            SetupActionButtonAsSearch(_searchButton);
            SetupActionButtonAsRemove(_removeButton);
            SetupTextBox(_entityIdSearch);

            _resultGrid = _uiControlsCreator.GetDataGridView("gridRelationsResult", null, new List<string>());

            bottomContainer.SetFullWidthColumn();
            bottomContainer.Add(_resultGrid, null, true);

            SetupGridView(_resultGrid);
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

            return grid;
        }
    }
}
