using AutoMapper;
using Iis.DataModel;
using Iis.DbLayer.OntologyData;
using Iis.DbLayer.OntologySchema;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData;
using Iis.OntologyData.Migration;
using Iis.OntologyManager.Configurations;
using Iis.OntologyManager.Style;
using Iis.OntologyManager.Helpers;
using Iis.OntologyManager.UiControls;
using Iis.OntologyManager.Dictionaries;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Net.Http;
using System.Text;
using static Iis.OntologyManager.UiControls.UiFilterControl;
using Iis.Services.Contracts.Interfaces;
using Iis.Interfaces.AccessLevels;
using System.Threading.Tasks;
using Iis.Services.Contracts.Params;
using Iis.OntologyManager.DTO;

namespace Iis.OntologyManager
{
    public partial class MainForm : Form
    {
        #region Fields

        const string EnvironmentPropertiesSectionName = "environmentProperties";
        const string UserCredentialsSectionName = "userCredentials";
        const string RequestSettingsSectionName = "requestSettings";
        const string DefaultName = "__default";

        IReadOnlyCollection<SchemaDataSource> _schemaSources;

        IConfiguration _configuration;
        IOntologySchema _schema;
        IOntologyNodesData _ontologyData;
        IOntologyManagerStyle _style;
        UiControlsCreator _uiControlsCreator;
        INodeTypeLinked _currentNodeType;
        IOntologySchemaService _schemaService;
        IList<INodeTypeLinked> _history = new List<INodeTypeLinked>();
        UserCredentials _userCredentials;
        RequestSettings _requestSettings;

        UiFilterControl _filterControl;
        UiMigrationControl _migrationControl;
        UiDuplicatesControl _duplicatesControl;
        UiEntityTypeControl _uiEntityTypeControl;
        UiRelationAttributeControl _uiRelationAttributeControl;
        UiRelationEntityControl _uiRelationEntityControl;
        UiOntologyDataControl _uiOntologyDataControl;
        UiAccessLevelControl _uiAccessLevelControl;
        RemoveEntityUiControl _removeEntityUiControl;
        Dictionary<NodeViewType, IUiNodeTypeControl> _nodeTypeControls = new Dictionary<NodeViewType, IUiNodeTypeControl>();
        Dictionary<string, IDataViewControl> _dataViewControls = new Dictionary<string, IDataViewControl>();
        const string VERSION = "1.33";
        Button btnMigrate;
        Button btnDuplicates;
        ILogger _logger;
        IAccessLevels _accessLevels;

        #endregion

        #region Properties and Constructors

        bool _ontologyDataView = false;

        private enum NodeViewType : byte
        {
            Entity,
            RelationEntity,
            RelationAttribute,
            Data
        }

        string DefaultSchemaStorage => _configuration.GetValue<string>("DefaultSchemaStorage");
        INodeTypeLinked SelectedNodeType =>
            gridTypes.SelectedRows.Count == 0 ?
                null : (INodeTypeLinked)gridTypes.SelectedRows[0].DataBoundItem;
        SchemaDataSource SelectedSchemaSource => cmbSchemaSources?.SelectedItem as SchemaDataSource ?? _schemaSources?.FirstOrDefault();

        string SelectedConnectionString =>
            SelectedSchemaSource?.SourceKind == SchemaSourceKind.Database ?
                SelectedSchemaSource.Data : null;


        public MainForm(
            IConfiguration configuration,
            IOntologySchemaService schemaService,
            ILogger logger)
        {
            InitializeComponent();
            Text = $"Володар Онтології {VERSION}";

            _configuration = configuration;
            _style = OntologyManagerStyle.GetDefaultStyle(this);
            _uiControlsCreator = new UiControlsCreator(_style);
            _logger = logger;
            _schemaService = schemaService;

            _schemaSources = ReadSchemaDataSourcesFromConfiguration();

            _userCredentials = _configuration
                                    .GetSection(UserCredentialsSectionName)
                                    .Get<UserCredentials>();

            _requestSettings = _configuration
                                    .GetSection(RequestSettingsSectionName)
                                    .Get<RequestSettings>();

            SuspendLayout();
            SetBackColor();
            _uiControlsCreator.SetGridTypesStyle(gridTypes);
            gridTypes.CellFormatting += GridTypes_CellFormatting;
            AddGridTypesMenu();
            SetControlsTabMain(panelRight);

            SetControlsTopPanel();

            LoadCurrentSchema(SelectedSchemaSource);

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
            pnlTop.Size = new Size(rootPanel.Width, _style.ButtonHeightDefault * 2);
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

            var pnlOntologyData = _uiControlsCreator.GetFillPanel(pnlBottom, true);
            _uiOntologyDataControl = new UiOntologyDataControl(_uiControlsCreator);
            _uiOntologyDataControl.Initialize("OntologyDataControl", pnlOntologyData);

            var pnlAccessLevels = _uiControlsCreator.GetFillPanel(pnlBottom, true);
            _uiAccessLevelControl = new UiAccessLevelControl(_uiControlsCreator);
            _uiAccessLevelControl.Initialize("AccessLevelControl", pnlAccessLevels);
            _uiAccessLevelControl.OnSave += SaveAccessLevels;

            _dataViewControls[EntityTypeNames.AccessLevel.ToString()] = _uiAccessLevelControl;
            _dataViewControls[DefaultName] = _uiOntologyDataControl;

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
            var rootContainer = new UiContainerManager("RootPanel", rootPanel);

            btnTypeBack = new Button { Text = "Назад" };
            btnTypeBack.Click += (sender, e) => { GoBack(); };
            rootContainer.Add(btnTypeBack);

            btnSwitch = new Button();
            SetSwitchViewTypeText();
            btnSwitch.Click += (sender, e) => { SwitchOntologyViewType(); };
            rootContainer.Add(btnSwitch);

            lblTypeHeaderName = new Label
            {
                ForeColor = Color.DarkBlue,
                Font = _style.TypeHeaderNameFont,
                AutoSize = true,
                Text = ""
            };

            rootContainer.GoToNewColumn();
            rootContainer.AutoWidth = true;
            rootContainer.Add(lblTypeHeaderName);
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
            cmbSchemaSources.SelectedIndexChanged += SourceSelectionChanged;
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

            var btnRemoveEntity = new Button { Text = "Видалення", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnRemoveEntity.Click += RemoveEntityClick;

            container.Add(btnRemoveEntity);

            container.GoToNewColumn();

            var menuElastic = new ContextMenuStrip();
            menuElastic.Items.Add("Індекси Онтології");
            menuElastic.Items[0].Click += (sender, e) => { ReindexElastic(IndexKeys.Ontology); };
            menuElastic.Items.Add("Історічні Індекси");
            menuElastic.Items[1].Click += (sender, e) => { ReindexElastic(IndexKeys.OntologyHistorical); };
            menuElastic.Items.Add("Індекси Ознак");
            menuElastic.Items[2].Click += (sender, e) => { ReindexElastic(IndexKeys.Signs); };
            menuElastic.Items.Add("Індекси Подій");
            menuElastic.Items[3].Click += (sender, e) => { ReindexElastic(IndexKeys.Events); };
            menuElastic.Items.Add("Індекси Звітів");
            menuElastic.Items[4].Click += (sender, e) => { ReindexElastic(IndexKeys.Reports); };
            menuElastic.Items.Add("Індекси Матеріалів");
            menuElastic.Items[5].Click += (sender, e) => { ReindexElastic(IndexKeys.Materials); };
            menuElastic.Items.Add("Індекси Wiki");
            menuElastic.Items[6].Click += (sender, e) => { ReindexElastic(IndexKeys.Wiki); };
            menuElastic.Items.Add("Історічні Індекси Wiki");
            menuElastic.Items[7].Click += (sender, e) => { ReindexElastic(IndexKeys.WikiHistorical); };
            menuElastic.Items.Add("Індекси Користувачів");
            menuElastic.Items[8].Click += (sender, e) => { ReindexElastic(IndexKeys.Users); };
            var btnMenu = new Button { Text = "Перестворити Elastic " + char.ConvertFromUtf32(9660), MinimumSize = new Size { Height = _style.ButtonHeightDefault }, ContextMenuStrip = menuElastic};
            btnMenu.Click += (sender, e) => { menuElastic.Show(btnMenu, new Point(0, btnMenu.Height)); };
            container.Add(btnMenu);

            var menuReload = new ContextMenuStrip();
            menuReload.Items.Add("Кеш Онтології");
            menuReload.Items[0].Click += async (sender, e) => { await ReloadOntologyData(); };
            menuReload.Items.Add("Бек-енд вцілому");
            menuReload.Items[1].Click += (sender, e) => { RestartIisApp(); };
            var btnReload = new Button { Text = "Перезавантажити " + char.ConvertFromUtf32(9660), MinimumSize = new Size { Height = _style.ButtonHeightDefault }, ContextMenuStrip = menuReload };
            btnReload.Click += (sender, e) => { menuReload.Show(btnReload, new Point(0, btnReload.Height)); };
            container.Add(btnReload);

            panelTop.ResumeLayout();
        }
        #endregion

        private async Task ReloadOntologyData()
        {
            if (SelectedSchemaSource?.SourceKind != SchemaSourceKind.Database) return;

            await WaitCursorActionAsync(DoReloadOntologyData);
        }

        private RequestWraper GetRequestWrapper() =>
            new RequestWraper(SelectedSchemaSource.ApiAddress, _userCredentials, _requestSettings, _logger);


        private async Task DoReloadOntologyData()
        {
            var requestWrapper = GetRequestWrapper();

            var result = await requestWrapper.ReloadOntologyDataAsync();

            var sb = new StringBuilder()
                .AppendLine($"Адреса:{result.RequestUrl}")
                .AppendLine($"Повідомлення: {result.Message}");

            ShowMessage(sb.ToString(), result.IsSuccess ? string.Empty : "Помилка");
        }

        private void RestartIisApp()
        {

        }

        #region UI Control Events
        private void GoBack()
        {
            if (_history.Count == 0) return;
            var nodeType = _history[_history.Count - 1];
            _history.RemoveAt(_history.Count - 1);
            SetNodeTypeView(nodeType, false);
        }
        private void SetSwitchViewTypeText()
        {
            btnSwitch.Text = _ontologyDataView ? "Показати тип" : "Показати данi";
            btnSwitch.Enabled = _schema?.SchemaSource?.SourceKind == SchemaSourceKind.Database;
        }
        private void SwitchOntologyViewType()
        {
            _ontologyDataView = !_ontologyDataView;
            SetNodeTypeView(_currentNodeType, false);
            SetSwitchViewTypeText();
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
            if (control.UpdatedDatabases.Contains(SelectedSchemaSource.Title))
                WaitCursorAction(() => LoadCurrentSchema(SelectedSchemaSource));
        }
        private void btnDuplicates_Click(object sender, EventArgs e)
        {
            var form = _uiControlsCreator.GetModalForm(this);
            var rootPanel = _uiControlsCreator.GetFillPanel(form);

            _duplicatesControl = new UiDuplicatesControl();
            _duplicatesControl.Initialize("DuplicatesControl", rootPanel);
            _duplicatesControl.OnGetData += () => _ontologyData;

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
        private void SourceSelectionChanged(object sender, EventArgs e)
        {
            WaitCursorAction(() => LoadCurrentSchema(SelectedSchemaSource));
        }
        public void GridTypes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            var nodeType = (INodeTypeLinked)grid.Rows[e.RowIndex].DataBoundItem;
            if (nodeType == null) return;
            var color = _style.GetColorByAncestor(nodeType);
            var row = (DataGridViewRow)grid.Rows[e.RowIndex];
            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }
        private void RemoveEntityClick(object sender, EventArgs e)
        {
            var form = _uiControlsCreator.GetModalForm(this);
            var rootPanel = _uiControlsCreator.GetFillPanel(form);

            form.Text = "Видалення";

            _removeEntityUiControl = new RemoveEntityUiControl(SelectedSchemaSource);
            _removeEntityUiControl.Initialize("RemoveEntityControl", rootPanel);
            _removeEntityUiControl.OnGetOntologyData += () => _ontologyData;
            _removeEntityUiControl.OnRemove += OnRemove;

            form.ShowDialog();

            form.Close();
        }

        private void WaitCursorAction(Action action)
        {
            UseWaitCursor = true;
            try
            {
                action();
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task WaitCursorActionAsync(Func<Task> action)
        {
            UseWaitCursor = true;
            try
            {
                await action();
            }
            finally
            {
                UseWaitCursor = false;
            }
        }
        public void ShowMessage(string message, string header = null)
        {
            var form = _uiControlsCreator.GetModalForm(this);
            form.Text = header;

            var rootPanel = _uiControlsCreator.GetFillPanel(form);
            var textBox = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, ScrollBars = RichTextBoxScrollBars.ForcedVertical };
            rootPanel.Controls.Add(textBox);
            textBox.Text = message;

            form.ShowDialog();
            form.Close();
        }

        private RequestResult SaveAccessLevels(ChangeAccessLevelsParams param)
        {
            var requestWrapper = GetRequestWrapper();

            var result = requestWrapper.ChangeAccessLevelsAsync(param).ConfigureAwait(false).GetAwaiter().GetResult();

            var sb = new StringBuilder()
                    .AppendLine($"Адреса:{result.RequestUrl}")
                    .AppendLine($"Повідомлення: {result.Message}");

            ShowMessage(sb.ToString(), result.IsSuccess ? string.Empty : "Помилка");

            return result;
        }

        private void ReindexElastic(IndexKeys indexKey)
        {
            if (SelectedSchemaSource?.SourceKind != SchemaSourceKind.Database) return;

            WaitCursorAction(() =>
            {
                var requestWrapper = GetRequestWrapper();

                var result = requestWrapper.ReIndexAsync(indexKey).ConfigureAwait(false).GetAwaiter().GetResult();

                var sb = new StringBuilder()
                    .AppendLine($"Адреса:{result.RequestUrl}")
                    .AppendLine($"Повідомлення: {result.Message}");

                ShowMessage(sb.ToString(), result.IsSuccess ? string.Empty : "Помилка");
            });
        }

        private void OnRemove(Guid entityId)
        {
            var requestWrapper = GetRequestWrapper();

            var result = requestWrapper.DeleteEntityAsync(entityId).ConfigureAwait(false).GetAwaiter().GetResult();

            var sb = new StringBuilder()
                .AppendLine($"Адреса:{result.RequestUrl}")
                .AppendLine($"Повідомлення: {result.Message}");

            ShowMessage(sb.ToString(), result.IsSuccess ? string.Empty : "Помилка");
        }
        #endregion

        #region Schema Logic
        private void LoadCurrentSchema(IOntologySchemaSource schemaSource)
        {
            var currentSchemaSource = SelectedSchemaSource;
            if (currentSchemaSource == null) return;

            _schema = _schemaService.GetOntologySchema(schemaSource);

            ReloadTypes(_filterControl.GetModel());

            _ontologyData =
                schemaSource.SourceKind == SchemaSourceKind.Database ?
                GetOntologyData(schemaSource.Data) :
                null;
        }

        private void UpdateSchemaSources()
        {
            _schemaSources = ReadSchemaDataSourcesFromConfiguration();
            _uiControlsCreator.UpdateComboSource(cmbSchemaSources, _schemaSources);
            var src = new List<IOntologySchemaSource>(_schemaSources);
        }

        private IReadOnlyCollection<SchemaDataSource> ReadSchemaDataSourcesFromConfiguration()
        {
            var result = new List<SchemaDataSource>();

            var environmentProps = _configuration.GetSection(EnvironmentPropertiesSectionName)
                                        .Get<IReadOnlyDictionary<string, EnvConfig>>()
                                        .OrderBy(property => property.Value.SortOrder);

            result.AddRange(environmentProps.Select(property => SchemaDataSource.CreateForDb(property.Key, property.Value.ConnectionString, property.Value.ApiUri, property.Value.AppUri)));

            var filesNames = Directory.GetFiles(DefaultSchemaStorage, "*.ont");

            result.AddRange(filesNames.Select(SchemaDataSource.CreateForFile));

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
        private void SetNodeTypeViewVisibility(NodeViewType? nodeViewType)
        {
            foreach (var key in _nodeTypeControls.Keys)
            {
                _nodeTypeControls[key].Visible = nodeViewType == key;
            }
        }
        private void SetDataViewVisibility(string nodeTypeName)
        {
            var selectedControl = GetDataViewControl(nodeTypeName);
            foreach (var control in _dataViewControls.Values)
            {
                control.Visible = control == selectedControl;
            }
        }
        private IDataViewControl GetDataViewControl(string nodeTypeName) =>
            nodeTypeName == null ?
                _dataViewControls[DefaultName] :
                _dataViewControls.GetValueOrDefault(nodeTypeName) ?? _dataViewControls[DefaultName];

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
            SetSwitchViewTypeText();
            if (nodeType == null) return;
            if (addToHistory && _currentNodeType != null)
            {
                _history.Add(_currentNodeType);
            }
            if (_ontologyDataView)
            {
                SetNodeTypeViewVisibility(null);
                SetDataViewVisibility(nodeType.Name);
                GetDataViewControl(nodeType.Name).SetUiValues(nodeType, _ontologyData);
            }
            else
            {
                var nodeViewType = GetNodeViewType(nodeType);
                SetNodeTypeViewVisibility(nodeViewType);
                SetDataViewVisibility(null);
                var aliases = nodeType.Kind == Kind.Entity ? _schema.Aliases.GetStrings(nodeType.Name) : new List<string>();
                _nodeTypeControls[nodeViewType].SetUiValues(nodeType, aliases);

                lblTypeHeaderName.Text = nodeType.Name;
                _currentNodeType = nodeType;
            }
        }
        private INodeTypeLinked ChooseEntityTypeFromCombo()
        {
            return _uiControlsCreator.ChooseFromModalComboBox(GetAllEntities(), "Name");
        }
        public void ReloadTypes(NodeTypeFilterFunc filter)
        {
            if (_schema == null) return;
            var ds = _schema.GetEntityTypes()
                .Where(nt => filter == null || filter(nt))
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

        public OntologyNodesData GetOntologyData() => GetOntologyData(SelectedConnectionString);

        #endregion
    }
}
