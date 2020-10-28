using AutoMapper;
using Iis.DataModel;
using Iis.DbLayer.OntologyData;
using Iis.DbLayer.OntologySchema;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData;
using Iis.OntologyData.Migration;
using Iis.OntologyManager.Style;
using Iis.OntologyManager.UiControls;
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
        
        UiFilterControl _filterControl;
        UiMigrationControl _migrationControl;
        UiDuplicatesControl _duplicatesControl;
        UiEntityTypeControl _uiEntityTypeControl;
        UiRelationAttributeControl _uiRelationAttributeControl;
        UiRelationEntityControl _uiRelationEntityControl;
        Dictionary<NodeViewType, IUiNodeTypeControl> _nodeTypeControls = new Dictionary<NodeViewType, IUiNodeTypeControl>();
        const string VERSION = "1.19";
        Button btnMigrate;
        Button btnDuplicates;
        ILogger _logger;

        private enum NodeViewType : byte
        {
            Entity,
            RelationEntity,
            RelationAttribute
        }

        string DefaultSchemaStorage => _configuration.GetValue<string>("DefaultSchemaStorage");
        INodeTypeLinked SelectedNodeType =>
            gridTypes.SelectedRows.Count == 0 ?
                null : (INodeTypeLinked)gridTypes.SelectedRows[0].DataBoundItem;
        IOntologySchemaSource SelectedSchemaSource =>
             (OntologySchemaSource) cmbSchemaSources.SelectedItem ??
                (_schemaSources?.Count > 0 ? _schemaSources[0] : null);
        string SelectedConnectionString =>
            SelectedSchemaSource?.SourceKind == SchemaSourceKind.Database ?
                SelectedSchemaSource.Data : null;

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

            btnSaveSchema = new Button { Text = "Зберегти", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnSaveSchema.Click += btnSave_Click;
            btnCompare = new Button { Text = "Порівняти", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnCompare.Click += btnCompare_Click;
            container.AddInRow(new List<Control> { btnSaveSchema, btnCompare });
            
            btnMigrate = new Button { Text = "Міграція", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnMigrate.Click += btnMigrate_Click;
            container.Add(btnMigrate);

            btnDuplicates = new Button { Text = "Дублікати", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnDuplicates.Click += btnDuplicates_Click;
            container.Add(btnDuplicates);

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
            var form = _uiControlsCreator.GetModalForm(this);
            var rootPanel = _uiControlsCreator.GetFillPanel(form);
            var control = new UiComparisonControl(_schemaSources, _schemaService, _schema);
            control.Initialize("ComparisonControl", rootPanel);

            form.ShowDialog();
            form.Close();
        }
        private void btnDuplicates_Click(object sender, EventArgs e)
        {
            var form = _uiControlsCreator.GetModalForm(this);
            var rootPanel = _uiControlsCreator.GetFillPanel(form);
            _duplicatesControl = new UiDuplicatesControl();
            _duplicatesControl.Initialize("DuplicatesControl", rootPanel);
            _duplicatesControl.OnGetData += GetOntologyData;

            var context = OntologyContext.GetContext(SelectedConnectionString);
            _duplicatesControl.PatchSaver = new OntologyPatchSaver(context);

            form.ShowDialog();
            form.Close();
        }
        private void btnMigrate_Click(object sender, EventArgs e)
        {
            var form = _uiControlsCreator.GetModalForm(this);
            var rootPanel = _uiControlsCreator.GetFillPanel(form);
            _migrationControl = new UiMigrationControl();
            _migrationControl.Initialize("MigrationControl", rootPanel);
            _migrationControl.OnRun += Migrate;

            form.ShowDialog();
            form.Close();
        }
        private IMigrationResult Migrate(IMigration migration, IMigrationOptions migrationOptions)
        {
            if (SelectedConnectionString == null) return null;

            var ontologyData = GetOntologyData(SelectedConnectionString);
            var migrator = new OntologyMigrator(ontologyData, migration);
            var result = migrator.Migrate(migrationOptions);

            var context = OntologyContext.GetContext(SelectedConnectionString);
            var ontologyPatchSaver = new OntologyPatchSaver(context);
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

        #region Ontology Data

        public OntologyNodesData GetOntologyData(string connectionString)
        {
            var context = OntologyContext.GetContext(connectionString);
            var schema = _schemaService.GetOntologySchema(SelectedSchemaSource);
            var saver = new OntologyPatchSaver(context);

            var rawData = new NodesRawData(context.Nodes, context.Relations, context.Attributes);
            return new OntologyNodesData(rawData, schema, saver);
        }
        public OntologyNodesData GetOntologyData()
        {
            if (SelectedConnectionString == null) return null;
            using var context = OntologyContext.GetContext(SelectedConnectionString);
            var schema = _schemaService.GetOntologySchema(SelectedSchemaSource);
            var saver = new OntologyPatchSaver(context);

            var rawData = new NodesRawData(context.Nodes, context.Relations, context.Attributes);
            return new OntologyNodesData(rawData, schema, saver);
        }

        #endregion
    }
}
