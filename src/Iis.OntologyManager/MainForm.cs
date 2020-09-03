using AutoMapper;
using Iis.DataModel;
using Iis.DbLayer.OntologyData;
using Iis.DbLayer.OntologySchema;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData;
using Iis.OntologyManager.Parameters;
using Iis.OntologyManager.Style;
using Iis.OntologyManager.UiControls;
using Iis.OntologySchema.Saver;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Iis.OntologyManager
{
    public partial class MainForm : Form
    {
        #region Properties and Constructors

        IConfiguration _configuration;
        IOntologySchema _schema;
        IOntologyManagerStyle _style;
        IMapper _mapper;
        UiControlsCreator _uiControlsCreator;
        INodeTypeLinked _currentNodeType;
        OntologySchemaService _schemaService;
        List<IOntologySchemaSource> _schemaSources;
        IList<INodeTypeLinked> _history = new List<INodeTypeLinked>();
        ISchemaCompareResult _compareResult;
        UiFilterControl _filterControl;
        UiMigrationControl _migrationControl;
        UiEntityTypeControl _uiEntityTypeControl;
        UiRelationAttributeControl _uiRelationAttributeControl;
        UiRelationEntityControl _uiRelationEntityControl;
        Dictionary<NodeViewType, IUiNodeTypeControl> _nodeTypeControls = new Dictionary<NodeViewType, IUiNodeTypeControl>();
        const string VERSION = "1.13";
        CheckBox cbComparisonCreate;
        CheckBox cbComparisonUpdate;
        CheckBox cbComparisonDelete;
        CheckBox cbComparisonAliases;
        Button btnMigrate;
        ILogger _logger;

        private enum NodeViewType : byte
        {
            Entity,
            RelationEntity,
            RelationAttribute
        }

        string DefaultSchemaStorage => _configuration.GetValue<string>("DefaultSchemaStorage");
        INodeTypeLinked SelectedNodeType
        {
            get
            {
                return gridTypes.SelectedRows.Count == 0 ?
                    null :
                    (INodeTypeLinked)gridTypes.SelectedRows[0].DataBoundItem;
            }
        }
        public MainForm(IConfiguration configuration,
            OntologySchemaService schemaService,
            ILogger logger, 
            IMapper mapper)
        {
            InitializeComponent();
            this.Text = $"Володар Онтології {VERSION}";
            _configuration = configuration;
            _style = OntologyManagerStyle.GetDefaultStyle(this);
            _uiControlsCreator = new UiControlsCreator(_style);
            _logger = logger;
            _schemaService = schemaService;
            _mapper = mapper;
            _schemaSources = GetSchemaSources();

            SuspendLayout();
            SetBackColor();
            _uiControlsCreator.SetGridTypesStyle(gridTypes);
            gridTypes.CellFormatting += _style.GridTypes_CellFormatting;
            AddGridTypesMenu();
            SetControlsTabMain(panelRight);
            
            SetControlsTopPanel();
            CreateComparisonPanel();
            LoadCurrentSchema();
            ResumeLayout();
            ReloadTypes(_filterControl.GetModel());
        }

        #endregion

        #region UI Control Creators
        private void SetBackColor()
        {
            panelTop.BackColor = _style.BackgroundColor;
            panelMain.BackColor = _style.BackgroundColor;
            panelLeft.BackColor = _style.BackgroundColor;
            panelRight.BackColor = _style.BackgroundColor;
        }

        private void SetControlsTabMain(Panel rootPanel)
        {
            var pnlTop = new Panel();
            pnlTop.Location = new Point(0, 0);
            pnlTop.Size = new Size(rootPanel.Width, _style.ButtonHeightDefault*2);
            pnlTop.BorderStyle = BorderStyle.FixedSingle;
            pnlTop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var pnlBottom = new Panel();
            pnlBottom.Location = new Point(0, 51);
            pnlBottom.Size = new Size(rootPanel.Width, rootPanel.Height - 51);
            pnlBottom.BorderStyle = BorderStyle.FixedSingle;
            pnlBottom.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SetTypeViewHeader(pnlTop);
            var pnlEntityType = _uiControlsCreator.GetFillPanel(pnlBottom, false);
            _uiEntityTypeControl = new UiEntityTypeControl(_uiControlsCreator);
            _uiEntityTypeControl.Initialize("EntityTypeControl", pnlEntityType);
            _uiEntityTypeControl.OnShowRelationType += ChildrenShowRelation;
            _uiEntityTypeControl.OnShowTargetType += (childNodeType) => SetNodeTypeView(childNodeType.TargetType, true);
            _uiEntityTypeControl.OnShowEntityType += (nodeType) => SetNodeTypeView(nodeType, true);
            _uiEntityTypeControl.OnCreateAttribute += (parentTypeId) => CreateNewNodeType(NodeViewType.RelationAttribute, parentTypeId);
            _uiEntityTypeControl.OnCreateRelationEntity += (parentTypeId) => CreateNewNodeType(NodeViewType.RelationEntity, parentTypeId);
            _uiEntityTypeControl.OnDeleteRelationEntity += DeleteChildNode;
            _uiEntityTypeControl.OnSetInheritance += SetInheritance;
            _uiEntityTypeControl.OnRemoveInheritance += RemoveInheritance;
            _uiEntityTypeControl.OnSave += OnNodeTypeSaveClick;

            var pnlRelationAttribute = _uiControlsCreator.GetFillPanel(pnlBottom, true);
            _uiRelationAttributeControl = new UiRelationAttributeControl(_uiControlsCreator);
            _uiRelationAttributeControl.Initialize("RelationAttributeControl", pnlRelationAttribute);
            _uiRelationAttributeControl.OnSave += OnNodeTypeSaveClick;

            var pnlRelationEntity = _uiControlsCreator.GetFillPanel(pnlBottom, true);
            _uiRelationEntityControl = new UiRelationEntityControl(_uiControlsCreator, GetAllEntities);
            _uiRelationEntityControl.Initialize("RelationEntityControl", pnlRelationEntity);
            _uiRelationEntityControl.OnSave += OnNodeTypeSaveClick;

            _nodeTypeControls[NodeViewType.Entity] = _uiEntityTypeControl;
            _nodeTypeControls[NodeViewType.RelationEntity] = _uiRelationEntityControl;
            _nodeTypeControls[NodeViewType.RelationAttribute] = _uiRelationAttributeControl;

            rootPanel.SuspendLayout();
            rootPanel.Controls.Add(pnlTop);
            rootPanel.Controls.Add(pnlBottom);
            rootPanel.ResumeLayout();
        }

        private void AddGridTypesMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Створити нову сутність");
            menu.Items[0].Click += (sender, e) => { CreateNewNodeType(NodeViewType.Entity, null); };
            menu.Items.Add("Знищити сутність");
            menu.Items[1].Click += (sender, e) => { RemoveNodeType(SelectedNodeType); };
            gridTypes.ContextMenuStrip = menu;
        }

        private void SetTypeViewHeader(Panel rootPanel)
        {
            rootPanel.SuspendLayout();

            btnTypeBack = new Button
            {
                Location = new Point(_style.MarginHor, _style.MarginVer),
                MinimumSize = new Size
                {
                    Height = _style.ButtonHeightDefault,
                    Width = _style.ButtonWidthDefault
                },
                Text = "Back"
            };
            btnTypeBack.Click += (sender, e) => { GoBack(); };

            lblTypeHeaderName = new Label
            {
                Location = new Point(btnTypeBack.Right + _style.MarginHor, _style.MarginVer),
                ForeColor = Color.DarkBlue,
                Font = _style.TypeHeaderNameFont,
                AutoSize = true,
                Text = ""
            };

            rootPanel.Controls.Add(btnTypeBack);
            rootPanel.Controls.Add(lblTypeHeaderName);
            rootPanel.ResumeLayout();
        }
        public void CreateComparisonPanel()
        {
            const int ComparisonMargin = 30;
            const int BtnCloseSize = 20;
            const int BtnCloseMargin = 5;
            panelComparison = new Panel {
                Name = "panelComparison",
                Location = new Point(ComparisonMargin, ComparisonMargin),
                Size = new Size(this.ClientSize.Width - ComparisonMargin * 2, this.ClientSize.Height - ComparisonMargin * 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = _style.ComparisonBackColor,
                Visible = false
            };
            panelComparison.SuspendLayout();
            var panels = _uiControlsCreator.GetTopBottomPanels(panelComparison, 100, 10);
            var container = new UiContainerManager("Comparison", panels.panelTop);

            var btnComparisonClose = new Button
            {
                Location = new Point(panels.panelTop.Width - BtnCloseSize - BtnCloseMargin*2, BtnCloseMargin),
                Anchor = Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(BtnCloseSize, BtnCloseSize),
                Text = "X"
            };
            btnComparisonClose.Click += (sender, e) => { panelComparison.Visible = false; };
            panels.panelTop.Controls.Add(btnComparisonClose);

            cmbSchemaSourcesCompare = new ComboBox
            {
                Name = "cmbSchemaSourcesCompare",
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Title",
                BackColor = panelTop.BackColor
            };
            var src = new List<IOntologySchemaSource>(_schemaSources);
            cmbSchemaSourcesCompare.DataSource = src;
            cmbSchemaSourcesCompare.SelectedIndexChanged += (sender, e) => { CompareSchemas(); };
            container.Add(cmbSchemaSourcesCompare);

            var btnComparisonUpdate = new Button { Text = "Update database", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnComparisonUpdate.Click += (sender, e) => { UpdateComparedDatabase(); };
            container.Add(btnComparisonUpdate);

            container.GoToNewColumn();
            container.Add(cbComparisonCreate = new CheckBox { Text = "Create", Checked = true });
            container.Add(cbComparisonUpdate = new CheckBox { Text = "Update", Checked = true });
            container.Add(cbComparisonDelete = new CheckBox { Text = "Delete" });
            container.Add(cbComparisonAliases = new CheckBox { Text = "Aliases" });

            txtComparison = new RichTextBox 
            {
                ReadOnly = true,
                Dock = DockStyle.Fill, 
                BackColor = panelComparison.BackColor 
            };
            panels.panelBottom.Controls.Add(txtComparison);

            panelComparison.ResumeLayout();
            this.Controls.Add(panelComparison);
            panelComparison.BringToFront();
        }
        private void SetControlsTopPanel()
        {
            panelTop.SuspendLayout();
            var container = new UiContainerManager("PanelTop", panelTop);
            _filterControl = new UiFilterControl();
            _filterControl.Initialize("FilterControl", null);
            _filterControl.OnChange += ReloadTypes;
            container.AddPanel(_filterControl.MainPanel);
            
            cmbSchemaSources = new ComboBox
            {
                Name = "cmbSchemaSources",
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Title",
                BackColor = panelTop.BackColor
            };
            cmbSchemaSources.DataSource = _schemaSources;
            cmbSchemaSources.SelectedIndexChanged += (sender, e) => { LoadCurrentSchema(); };
            container.Add(cmbSchemaSources);

            btnSaveSchema = new Button { Text = "Save To File", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnSaveSchema.Click += btnSave_Click;
            btnCompare = new Button { Text = "Compare", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnCompare.Click += btnCompare_Click;
            container.AddInRow(new List<Control> { btnSaveSchema, btnCompare });
            btnMigrate = new Button { Text = "Migrate", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnMigrate.Click += btnMigrate_Click;
            container.Add(btnMigrate);

            panelTop.ResumeLayout();
        }
        #endregion

        #region UI Control Events
        private void GoBack()
        {
            if (_history.Count == 0) return;
            var nodeType = _history[_history.Count - 1];
            _history.RemoveAt(_history.Count - 1);
            SetNodeTypeView(nodeType, false);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                InitialDirectory = DefaultSchemaStorage,
                Filter = "Ontology files (*.ont)|*.ont|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _schemaService.SaveToFile(_schema, dialog.FileName);
            }
            UpdateSchemaSources();
        }
        private void btnCompare_Click(object sender, EventArgs e)
        {
            CompareSchemas();
            panelComparison.Visible = true;
        }
        private void btnMigrate_Click(object sender, EventArgs e)
        {
            var form = new Form() 
            { 
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Width = this.Width - 20,
                Height = this.Height - 20,
                StartPosition = FormStartPosition.CenterParent
            };
            var rootPanel = _uiControlsCreator.GetFillPanel(form);
            _migrationControl = new UiMigrationControl();
            _migrationControl.Initialize("MigrationControl", rootPanel);
            _migrationControl.OnRun += Migrate;
            //_filterControl.OnChange += ReloadTypes;

            form.ShowDialog();
            form.Close();
        }
        private Dictionary<Guid,Guid> Migrate(IMigrationEntity migration)
        {
            var currentSchemaSource = (OntologySchemaSource)cmbSchemaSources.SelectedItem ??
                (_schemaSources?.Count > 0 ? _schemaSources[0] : (OntologySchemaSource)null);
            if (currentSchemaSource == null || currentSchemaSource.SourceKind != SchemaSourceKind.Database)
            {
                return null;
            }

            using var context = OntologyContext.GetContext(currentSchemaSource.Data);
            var schema = _schemaService.GetOntologySchema(currentSchemaSource);

            var rawData = new NodesRawData(context.Nodes, context.Relations, context.Attributes);
            var ontologyData = new OntologyNodesData(rawData, schema);
            var result = ontologyData.Migrate(migration);

            var ontologyPatchSaver = new OntologyPatchSaver(context, _mapper);
            ontologyPatchSaver.SavePatch(ontologyData.Patch); 
            return result;
        }

        private void gridTypes_SelectionChanged(object sender, EventArgs e)
        {
            if (SelectedNodeType == null) return;
            _history.Clear();
            SetNodeTypeView(SelectedNodeType, false);
        }

        #endregion

        #region Schema Logic
        private void LoadCurrentSchema()
        {
            var currentSchemaSource = (OntologySchemaSource)cmbSchemaSources.SelectedItem ??
                (_schemaSources?.Count > 0 ? _schemaSources[0] : (OntologySchemaSource)null);
            if (currentSchemaSource == null) return;
            _schema = _schemaService.GetOntologySchema(currentSchemaSource);
            ReloadTypes(_filterControl.GetModel());
        }
        private void UpdateSchemaSources()
        {
            _schemaSources = GetSchemaSources();
            _uiControlsCreator.UpdateComboSource(cmbSchemaSources, _schemaSources);
            var src = new List<IOntologySchemaSource>(_schemaSources);
            _uiControlsCreator.UpdateComboSource(cmbSchemaSourcesCompare, src);
        }
        private List<IOntologySchemaSource> GetSchemaSources()
        {
            var result = new List<IOntologySchemaSource>
            {
            };
            var connectionStrings = _configuration.GetSection("ConnectionStrings").GetChildren();
            result.AddRange(connectionStrings.Select(section => new OntologySchemaSource
            {
                Title = $"(DB): {section.Key}",
                SourceKind = SchemaSourceKind.Database,
                Data = section.Value
            }));

            var filesNames = Directory.GetFiles(DefaultSchemaStorage, "*.ont");
            result.AddRange(filesNames.Select(fileName => new OntologySchemaSource
            {
                Title = $"(FILE): {Path.GetFileNameWithoutExtension(fileName)}",
                SourceKind = SchemaSourceKind.File,
                Data = fileName
            }));

            return result;
        }
        private List<INodeTypeLinked> GetAllEntities()
        {
            var filter = new GetTypesFilter { Kinds = new[] { Kind.Entity } };
            return _schema.GetTypes(filter).OrderBy(t => t.Name).ToList();
        }
        private void OnNodeTypeSaveClick(INodeTypeUpdateParameter updateParameter)
        {
            try
            {
                _schema.UpdateNodeType(updateParameter);
                if (updateParameter.Id == null && updateParameter.ParentTypeId == null)
                {
                    ReloadTypes(_filterControl.GetModel());
                }
                else
                {
                    GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Compare Logic
        private void CompareSchemas()
        {
            var selectedSource = cmbSchemaSourcesCompare.SelectedItem;
            if (selectedSource == null) return;
            var schema = _schemaService.GetOntologySchema((IOntologySchemaSource)selectedSource);
            _compareResult = _schema.CompareTo(schema);
            txtComparison.Text = GetCompareText(_compareResult);
        }
        private string GetCompareText(string title, IEnumerable<string> lines)
        {
            var sb = new StringBuilder();
            sb.AppendLine("===============");
            sb.AppendLine(title);
            sb.AppendLine("===============");
            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }

            return sb.ToString();
        }
        private string GetCompareText(ISchemaCompareResult compareResult)
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetCompareText("NODES TO ADD", compareResult.ItemsToAdd.Select(item => item.GetStringCode())));
            sb.AppendLine(GetCompareText("NODES TO DELETE", compareResult.ItemsToDelete.Select(item => item.GetStringCode())));
            sb.AppendLine("===============");
            sb.AppendLine("NODES TO UPDATE");
            sb.AppendLine("===============");
            foreach (var item in compareResult.ItemsToUpdate)
            {
                sb.AppendLine(item.NodeTypeFrom.GetStringCode());
                var differences = item.NodeTypeFrom.GetDifference(item.NodeTypeTo);
                foreach (var diff in differences)
                {
                    sb.AppendLine($"{diff.PropertyName}:\n{diff.OldValue}\n{diff.NewValue}");
                }
                sb.AppendLine();
            }
            sb.AppendLine(GetCompareText("ALIASES TO ADD", compareResult.AliasesToAdd.Select(item => item.ToString())));
            sb.AppendLine(GetCompareText("ALIASES TO DELETE", compareResult.AliasesToDelete.Select(item => item.ToString())));
            sb.AppendLine(GetCompareText("ALIASES TO UPDATE", compareResult.AliasesToUpdate.Select(item => item.ToString())));
            return sb.ToString();
        }
        private void UpdateComparedDatabase()
        {
            if (_compareResult == null) return;
            if (MessageBox.Show($"Are you sure you want to update database {_compareResult.SchemaSource.Title}?", "Update Database", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
            using var context = OntologyContext.GetContext(_compareResult.SchemaSource.Data);
            var schema = _schemaService.GetOntologySchema(_compareResult.SchemaSource);
            var schemaSaver = new OntologySchemaSaver(context);
            var parameters = new SchemaSaveParameters
            {
                Create = cbComparisonCreate.Checked,
                Update = cbComparisonUpdate.Checked,
                Delete = cbComparisonDelete.Checked,
                Aliases = cbComparisonAliases.Checked
            };

            schemaSaver.SaveToDatabase(_compareResult, schema, parameters);
            CompareSchemas();
        }
        #endregion

        #region Node Type Logic
        private NodeViewType GetNodeViewType(INodeTypeLinked nodeType)
        {
            if (nodeType.Kind == Kind.Entity) return NodeViewType.Entity;

            if (nodeType.Kind == Kind.Relation)
            {
                return nodeType.RelationType.TargetType.Kind == Kind.Entity ?
                    NodeViewType.RelationEntity :
                    NodeViewType.RelationAttribute;
            }

            throw new ArgumentException($"Cannot get NodeViewType for {nodeType.Id}");
        }
        private void SetInheritance()
        {
            var newTargetType = ChooseEntityTypeFromCombo();
            if (newTargetType == null) return;
            _schema.SetInheritance(_currentNodeType.Id, newTargetType.Id);
            SetNodeTypeView(_currentNodeType, false);
        }
        private void RemoveInheritance(Guid ancestorId)
        {
            _schema.RemoveInheritance(_currentNodeType.Id, ancestorId);
            SetNodeTypeView(_currentNodeType, false);
        }
        private void SetNodeTypeViewVisibility(NodeViewType nodeViewType)
        {
            foreach (var key in _nodeTypeControls.Keys)
            {
                _nodeTypeControls[key].Visible = nodeViewType == key;
            }
        }
        private void CreateNewNodeType(NodeViewType nodeViewType, Guid? parentTypeId)
        {
            SetNodeTypeViewVisibility(nodeViewType);
            _nodeTypeControls[nodeViewType].SetParentTypeId(parentTypeId);
            _nodeTypeControls[nodeViewType].CreateNew();
            _history.Add(_currentNodeType);
        }
        private void RemoveNodeType(INodeTypeLinked nodeType)
        {
            if (nodeType == null) return;
            var msg = _schema.ValidateRemoveEntity(nodeType.Id);
            const string header = "Знищення сутності";
            if (!string.IsNullOrEmpty(msg))
            {
                MessageBox.Show(msg, header);
            }
            if (MessageBox.Show($"Ви правда хочете знищити сутність {nodeType.Name}?", header, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _schema.RemoveEntity(nodeType.Id);
                ReloadTypes(_filterControl.GetModel());
            }
        }
        private void DeleteChildNode(IChildNodeType childNodeType)
        {
            _schema.RemoveRelation(childNodeType.RelationId);
            SetNodeTypeView(_currentNodeType, false);
        }
        private void SetNodeTypeView(INodeTypeLinked nodeType, bool addToHistory)
        {
            if (addToHistory && _currentNodeType != null)
            {
                _history.Add(_currentNodeType);
            }
            var nodeViewType = GetNodeViewType(nodeType);
            SetNodeTypeViewVisibility(nodeViewType);
            var aliases = nodeType.Kind == Kind.Entity ? _schema.Aliases.GetStrings(nodeType.Name) : new List<string>();
            _nodeTypeControls[nodeViewType].SetUiValues(nodeType, aliases);

            lblTypeHeaderName.Text = nodeType.Name;
            _currentNodeType = nodeType;
        }
        private INodeTypeLinked ChooseEntityTypeFromCombo()
        {
            return _uiControlsCreator.ChooseFromModalComboBox(GetAllEntities(), "Name");
        }
        public void ReloadTypes(IGetTypesFilter filter)
        {
            if (_schema == null) return;
            var ds = _schema.GetTypes(filter)
                .OrderBy(t => t.Name)
                .ToList();
            this.gridTypes.DataSource = ds;
        }
        private void ChildrenShowRelation(IChildNodeType childNodeType)
        {
            var relationType = _schema.GetNodeTypeById(childNodeType.RelationId);
            if (relationType == null) return;
            SetNodeTypeView(relationType, true);
        }
        #endregion
    }
}
