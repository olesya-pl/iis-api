using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Ontology;
using Iis.OntologyManager.Style;
using Iis.OntologyManager.UiControls;
using Iis.OntologySchema;
using Iis.OntologySchema.ChangeParameters;
using Iis.OntologySchema.DataTypes;
using Iis.OntologySchema.Saver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        IConfiguration _configuration;
        IOntologySchema _schema;
        IOntologyManagerStyle _style;
        UiControlsCreator _uiControlsCreator;
        INodeTypeLinked _currentNodeType;
        OntologySchemaService _schemaService;
        List<IOntologySchemaSource> _schemaSources;
        IList<INodeTypeLinked> _history = new List<INodeTypeLinked>();
        ISchemaCompareResult _compareResult;
        Font SelectedFont { get; set; }
        Font TypeHeaderNameFont { get; set; }
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
            IOntologyManagerStyle style,
            UiControlsCreator uiControlsCreator,
            OntologySchemaService schemaService)
        {
            InitializeComponent();
            _configuration = configuration;
            _style = style;
            _uiControlsCreator = uiControlsCreator;
            _schemaService = schemaService;
            _schemaSources = GetSchemaSources();
            
            SelectedFont = new Font(DefaultFont, FontStyle.Bold);
            TypeHeaderNameFont = new Font("Arial", 16, FontStyle.Bold);
            SuspendLayout();
            SetBackColor();
            _uiControlsCreator.SetGridTypesStyle(gridTypes);
            SetAdditionalControls();
            CreateComparisonPanel();
            LoadCurrentSchema();
            ResumeLayout();
            ReloadTypes();
        }

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
            pnlTop.Size = new Size(rootPanel.Width, 50);
            pnlTop.BorderStyle = BorderStyle.FixedSingle;
            pnlTop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var pnlBottom = new Panel();
            pnlBottom.Location = new Point(0, 51);
            pnlBottom.Size = new Size(rootPanel.Width, rootPanel.Height - 51);
            pnlBottom.BorderStyle = BorderStyle.FixedSingle;
            pnlBottom.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SetTypeViewHeader(pnlTop);
            SetTypeViewControls(pnlBottom);

            rootPanel.SuspendLayout();
            rootPanel.Controls.Add(pnlTop);
            rootPanel.Controls.Add(pnlBottom);
            rootPanel.ResumeLayout();
        }

        private void SetTypeViewHeader(Panel rootPanel)
        {
            rootPanel.SuspendLayout();
            
            btnTypeBack = new Button
            {
                Location = new Point(_style.MarginHor, _style.MarginVer),
                Width = 60,
                Text = "Back"
            };
            btnTypeBack.Click += btnTypeBack_Click;

            lblTypeHeaderName = new Label
            {
                Location = new Point(btnTypeBack.Right + _style.MarginHor, _style.MarginVer),
                ForeColor = Color.DarkBlue,
                Font = TypeHeaderNameFont,
                AutoSize = true,
                Text = ""
            };

            rootPanel.Controls.Add(btnTypeBack);
            rootPanel.Controls.Add(lblTypeHeaderName);
            rootPanel.ResumeLayout();
        }
        
        private void SetTypeViewControls(Panel rootPanel)
        {
            rootPanel.SuspendLayout();

            var container = new UiContainerManager(rootPanel, _style);

            container.Add(txtId = new TextBox { ReadOnly = true }, "Id");
            container.Add(txtName = new TextBox(), "Name");
            container.Add(txtTitle = new TextBox(), "Title");
            container.GoToNewColumn();
            
            gridInheritance = _uiControlsCreator.GetDataGridView("gridChildren", null,
                new List<string> { "Name" });
            gridInheritance.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            gridInheritance.CellFormatting += gridTypes_CellFormatting;
            gridInheritance.DoubleClick += gridInheritance_DoubleClick;
            lblGridInheritance = container.Add(gridInheritance, "Inheritance", true);

            container.GoToNewColumn();
            container.Add(txtMeta = new RichTextBox(), "Meta", true);

            container.GoToNewColumn(_style.ButtonWidthDefault);
            btnTypeSave = new Button { Text = "Save" };
            btnTypeSave.Click += (sender, e) => { SaveTypeProperties(); };
            container.Add(btnTypeSave);

            
            container.GoToBottom();
            container.StepDown();
            container.SetFullWidthColumn();

            menuChildren = new ContextMenuStrip();
            menuChildren.Items.Add("Show Relation");
            menuChildren.Items[0].Click += (sender, e) => { gridChildrenEvent(ChildrenShowRelation); };
            menuChildren.Items.Add("Change Target Type");
            menuChildren.Items[1].Click += (sender, e) => { gridChildrenEvent(ChildrenChangeTargetType); };

            gridChildren = _uiControlsCreator.GetDataGridView("gridChildren", null,
                new List<string> { "RelationName", "RelationTitle", "Name", "InheritedFrom", "EmbeddingOptions", "ScalarType" });
            gridChildren.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridChildren.ColumnHeadersVisible = true;
            gridChildren.AutoGenerateColumns = false;
            gridChildren.DoubleClick += gridChildren_DoubleClick;
            gridChildren.CellFormatting += gridChildren_CellFormatting;
            gridChildren.ContextMenuStrip = menuChildren;

            container.Add(gridChildren, null, true);
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
            var container = new UiContainerManager(panels.panelTop, _style);

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

            var btnComparisonUpdate = new Button { Text = "Update database" };
            btnComparisonUpdate.Click += (sender, e) => { UpdateComparedDatabase(); };
            container.Add(btnComparisonUpdate);

            txtComparison = new RichTextBox { Dock = DockStyle.Fill, BackColor = panelComparison.BackColor };
            panels.panelBottom.Controls.Add(txtComparison);

            panelComparison.ResumeLayout();
            this.Controls.Add(panelComparison);
            panelComparison.BringToFront();
        }
        private void SetControlsTopPanel()
        {
            panelTop.SuspendLayout();
            var container = new UiContainerManager(panelTop, _style);
            container.SetColWidth(100);
            container.Add(cbFilterEntities = new CheckBox { Text = "Entities", Checked = true });
            container.Add(cbFilterAttributes = new CheckBox { Text = "Attributes" });
            container.Add(cbFilterRelations = new CheckBox { Text = "Relations" });

            cbFilterEntities.CheckedChanged += FilterChanged;
            cbFilterAttributes.CheckedChanged += FilterChanged;
            cbFilterRelations.CheckedChanged += FilterChanged;

            container.GoToNewColumn();
            container.Add(txtFilterName = new TextBox(), "Name");
            txtFilterName.TextChanged += FilterChanged;

            container.GoToNewColumn(_style.ControlWidthDefault);

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

            btnSaveSchema = new Button { Text = "Save" };
            btnSaveSchema.Click += btnSave_Click;
            btnCompare = new Button { Text = "Compare" };
            btnCompare.Click += btnCompare_Click;
            container.AddInRow(new List<Control> { btnSaveSchema, btnCompare });

            panelTop.ResumeLayout();
        }

        private void LoadCurrentSchema()
        {
            var currentSchemaSource = (OntologySchemaSource)cmbSchemaSources.SelectedItem ?? 
                (_schemaSources?.Count > 0 ? _schemaSources[0] : (OntologySchemaSource)null);
            if (currentSchemaSource == null) return;
            _schema = _schemaService.GetOntologySchema(currentSchemaSource);
            ReloadTypes();
        }

        private void SaveTypeProperties()
        {
            if (_currentNodeType == null) return;
            var updateParameter = new NodeTypeUpdateParameter
            {
                Id = _currentNodeType.Id,
                Title = txtTitle.Text,
                Meta = txtMeta.Text
            };
            _schema.UpdateNodeType(updateParameter);
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            ReloadTypes();
        }

        private void SetAdditionalControls()
        {
            SetControlsTabMain(panelRight);
            SetControlsTopPanel();
        }

        private INodeTypeLinked ChooseEntityTypeFromCombo()
        {
            var filter = new GetTypesFilter { Kinds = new[] { Kind.Entity } };
            var entities = _schema.GetTypes(filter)
                .OrderBy(t => t.Name)
                .ToList();
            return _uiControlsCreator.ChooseFromModalComboBox(entities, "Name");
        }

        private IGetTypesFilter GetFilter()
        {
            var kinds = new List<Kind>();
            if (cbFilterEntities.Checked) kinds.Add(Kind.Entity);
            if (cbFilterAttributes.Checked) kinds.Add(Kind.Attribute);
            if (cbFilterRelations.Checked) kinds.Add(Kind.Relation);
            return new GetTypesFilter
            {
                Kinds = kinds,
                Name = txtFilterName.Text
            };
        }

        public void ReloadTypes()
        {
            if (_schema == null) return;
            var filter = GetFilter();
            var ds = _schema.GetTypes(filter)
                .OrderBy(t => t.Name)
                .ToList();
            this.gridTypes.DataSource = ds;
        }

        private void CompareSchemas()
        {
            var selectedSource = cmbSchemaSourcesCompare.SelectedItem;
            if (selectedSource == null) return;
            var schema = _schemaService.GetOntologySchema((IOntologySchemaSource)selectedSource);
            _compareResult = _schema.CompareTo(schema);
            txtComparison.Text = GetCompareText(_compareResult);
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            CompareSchemas();
            panelComparison.Visible = true;
        }

        private string GetCompareText(string title, IEnumerable<INodeTypeLinked> nodeTypes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("===============");
            sb.AppendLine(title);
            sb.AppendLine("===============");
            foreach (var nodeType in nodeTypes)
            {
                sb.AppendLine(nodeType.GetStringCode());
            }

            return sb.ToString();
        }

        private string GetCompareText(ISchemaCompareResult compareResult)
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetCompareText("NODES TO ADD", compareResult.ItemsToAdd));
            sb.AppendLine(GetCompareText("NODES TO DELETE", compareResult.ItemsToDelete));
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
            return sb.ToString();
        }

        private void gridTypes_SelectionChanged(object sender, EventArgs e)
        {
            if (SelectedNodeType == null) return;
            _history.Clear();
            SetNodeTypeView(SelectedNodeType, false);
        }

        private void SetNodeTypeView(INodeTypeLinked nodeType, bool addToHistory)
        {
            if (addToHistory && _currentNodeType != null)
            {
                _history.Add(_currentNodeType);
            }
            lblTypeHeaderName.Text = nodeType.Name;
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            var children = nodeType.GetAllChildren();
            gridChildren.DataSource = children;
            var ancestors = nodeType.GetAllAncestors();
            gridInheritance.DataSource = ancestors;
            txtMeta.Text = nodeType.Meta;
            _currentNodeType = nodeType;
        }

        private Color GetColorByNodeType(Kind kind)
        {
            switch (kind)
            {
                case Kind.Entity:
                    return _style.EntityTypeBackColor;
                case Kind.Attribute:
                    return _style.AttributeTypeBackColor;
                case Kind.Relation:
                    return _style.RelationTypeBackColor;
                default:
                    return _style.BackgroundColor;
            }
        }

        private void gridTypes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            var nodeType = (INodeTypeLinked)grid.Rows[e.RowIndex].DataBoundItem;
            if (nodeType == null) return;
            var color = GetColorByNodeType(nodeType.Kind);
            var row = (DataGridViewRow)grid.Rows[e.RowIndex];
            var style = row.DefaultCellStyle;
            
            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? SelectedFont : DefaultFont;
        }

        private void gridChildren_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            var nodeType = (IChildNodeType)grid.Rows[e.RowIndex].DataBoundItem;
            if (nodeType == null) return;
            var color = GetColorByNodeType(nodeType.Kind);
            var row = (DataGridViewRow)grid.Rows[e.RowIndex];
            var style = row.DefaultCellStyle;

            style.BackColor = color;
            style.SelectionBackColor = color;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? SelectedFont : DefaultFont;
        }

        private void gridChildrenEvent(Action<IChildNodeType> action)
        {
            var grid = gridChildren;
            var selectedRow = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;
            if (selectedRow == null) return;
            var childNodeType = (IChildNodeType)selectedRow.DataBoundItem;
            action(childNodeType);
        }

        private void ChildrenShowRelation(IChildNodeType childNodeType)
        {
            var relationType = _schema.GetNodeTypeById(childNodeType.RelationId);
            if (relationType == null) return;
            SetNodeTypeView(relationType, true);
        }

        private void ChildrenChangeTargetType(IChildNodeType childNodeType)
        {
            if (childNodeType.TargetType.Kind != Kind.Entity)
            {
                MessageBox.Show("Only properties of Entity type can be changed");
                return;
            }
            var newTargetType = ChooseEntityTypeFromCombo();
            if (newTargetType == null) return;
            _schema.UpdateTargetType(childNodeType.RelationId, newTargetType.Id);
            SetNodeTypeView(_currentNodeType, false);
        }

        private void gridChildren_DoubleClick(object sender, EventArgs e)
        {
            gridChildrenEvent(cnt => SetNodeTypeView(cnt.TargetType, true));
        }

        private void menuChildren_showRelationClick(object sender, EventArgs e)
        {
            gridChildrenEvent(ChildrenShowRelation);
        }

        private void gridInheritance_DoubleClick(object sender, EventArgs e)
        {
            var grid = (DataGridView)sender;
            var selectedRow = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;
            if (selectedRow == null) return;
            var nodeType = (INodeTypeLinked)selectedRow.DataBoundItem;
            SetNodeTypeView(nodeType, true);
        }

        private void btnTypeBack_Click(object sender, EventArgs e)
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
            schemaSaver.SaveToDatabase(_compareResult, schema);
            CompareSchemas();
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
            var result = new List<IOntologySchemaSource> { 
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
    }
    // TODO: 
    // separate view for relation
    // separate view for attribute
    // 
}
