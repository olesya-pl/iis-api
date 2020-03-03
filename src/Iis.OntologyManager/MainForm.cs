﻿using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Ontology;
using Iis.OntologyManager.Style;
using Iis.OntologyManager.UiControls;
using Iis.OntologySchema;
using Iis.OntologySchema.DataTypes;
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
        List<OntologySchemaSource> _schemaSources;
        IList<INodeTypeLinked> _history = new List<INodeTypeLinked>();
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

            var top = _style.MarginVer * 2;
            lblId = new Label
            {
                Left = _style.MarginHor,
                Top = top,
                Width = _style.ControlWidthDefault,
                Text = "Id"
            };
            rootPanel.Controls.Add(lblId);
            top += lblId.Height;
            txtId = new TextBox
            {
                Left = _style.MarginHor,
                Top = top,
                Width = _style.ControlWidthDefault,
                ReadOnly = true
            };
            rootPanel.Controls.Add(txtId);
            top += txtId.Height + _style.MarginVer;
            lblName = new Label
            {
                Left = _style.MarginHor,
                Top = top,
                Width = _style.ControlWidthDefault,
                Text = "Name"
            };
            rootPanel.Controls.Add(lblName);
            top += lblName.Height;
            txtName = new TextBox
            {
                Left = _style.MarginHor,
                Top = top,
                Width = _style.ControlWidthDefault
            };
            rootPanel.Controls.Add(txtName);

            top += txtName.Height + _style.MarginVer;
            lblTitle = new Label
            {
                Left = _style.MarginHor,
                Top = top,
                Width = _style.ControlWidthDefault,
                Text = "Title"
            };
            rootPanel.Controls.Add(lblTitle);
            top += lblTitle.Height;
            txtTitle = new TextBox
            {
                Left = _style.MarginHor,
                Top = top,
                Width = _style.ControlWidthDefault
            };
            rootPanel.Controls.Add(txtTitle);
            top += txtTitle.Height + _style.MarginVer * 2;

            menuChildren = new ContextMenuStrip();
            menuChildren.Items.Add("Show Relation");
            menuChildren.Items[0].Click += menuChildren_showRelationClick;

            gridChildren = _uiControlsCreator.GetDataGridView("gridChildren", new Point(_style.MarginHor, top), 
                new List<string> { "RelationName", "RelationTitle", "Name", "InheritedFrom", "EmbeddingOptions", "ScalarType" });
            gridChildren.Width = rootPanel.Width - _style.MarginHor * 2;
            gridChildren.Height = rootPanel.Bottom - top - _style.MarginVer * 2;
            gridChildren.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        
            gridChildren.ColumnHeadersVisible = true;
            gridChildren.AutoGenerateColumns = false;
            gridChildren.DoubleClick += gridChildren_DoubleClick;
            gridChildren.CellFormatting += gridChildren_CellFormatting;
            gridChildren.ContextMenuStrip = menuChildren;
            
            rootPanel.Controls.Add(gridChildren);

            top = _style.MarginVer;
            var hor = lblId.Right + _style.MarginHor;
            lblGridInheritance = new Label
            {
                Left = hor,
                Top = top,
                Width = _style.ControlWidthDefault,
                Text = "Inherited from"
            };
            top += lblGridInheritance.Height;
            
            gridInheritance = _uiControlsCreator.GetDataGridView("gridChildren", new Point(hor, top),
                new List<string> { "Name" });
            gridInheritance.Width = _style.ControlWidthDefault;
            gridInheritance.Height = txtTitle.Bottom - gridInheritance.Top;
            gridInheritance.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            gridInheritance.CellFormatting += gridTypes_CellFormatting;
            gridInheritance.DoubleClick += gridInheritance_DoubleClick;
            rootPanel.Controls.Add(lblGridInheritance);
            rootPanel.Controls.Add(gridInheritance);

            top = _style.MarginVer;
            hor = gridInheritance.Right + _style.MarginHor;
            lblMeta = new Label
            {
                Left = hor,
                Top = top,
                Width = _style.ControlWidthDefault,
                Text = "Metadata"
            };
            top += lblMeta.Height;
            txtMeta = new RichTextBox
            {
                Location = new Point(hor, top),
                Width = _style.ControlWidthDefault,
                Height = txtTitle.Bottom - gridInheritance.Top
            };
            rootPanel.Controls.Add(lblMeta);
            rootPanel.Controls.Add(txtMeta);

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
            var btnComparisonClose = new Button
            {
                Location = new Point(panels.panelTop.Width - BtnCloseSize - BtnCloseMargin*2, BtnCloseMargin),
                Anchor = Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(BtnCloseSize, BtnCloseSize),
                Text = "X"
            };
            btnComparisonClose.Click += (sender, e) => { panelComparison.Visible = false; };

            var btnComparisonUpdate = new Button
            {
                Location = new Point(_style.MarginVer, _style.MarginHor),
                Anchor = Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Width = _style.ControlWidthDefault,
                Text = "Update database"
            };
            btnComparisonUpdate.Click += (sender, e) => { UpdateComparedDatabase(); };
            panels.panelTop.Controls.Add(btnComparisonClose);

            txtComparison = new RichTextBox { Dock = DockStyle.Fill, BackColor = panelComparison.BackColor };
            panels.panelBottom.Controls.Add(txtComparison);

            panelComparison.ResumeLayout();
            this.Controls.Add(panelComparison);
            panelComparison.BringToFront();
        }
        private void SetControlsTopPanel()
        {
            var top = _style.MarginVer;
            var left = _style.MarginHor;
            panelTop.SuspendLayout();
            cbFilterEntities = new CheckBox
            {
                Left = left,
                Top = top,
                Width = 100,
                Text = "Entities",
                Checked = true
            };
            panelTop.Controls.Add(cbFilterEntities);
            top += cbFilterEntities.Height + _style.MarginVerSmall;
            cbFilterEntities.CheckedChanged += FilterChanged;

            cbFilterAttributes = new CheckBox
            {
                Left = left,
                Top = top,
                Width = 100,
                Text = "Attributes",
                Checked = false
            };
            panelTop.Controls.Add(cbFilterAttributes);
            top += cbFilterAttributes.Height + _style.MarginVerSmall;
            cbFilterAttributes.CheckedChanged += FilterChanged;

            cbFilterRelations = new CheckBox
            {
                Left = left,
                Top = top,
                Width = 100,
                Text = "Relations",
                Checked = false
            };
            panelTop.Controls.Add(cbFilterRelations);
            top += cbFilterAttributes.Height + _style.MarginVerSmall;
            cbFilterRelations.CheckedChanged += FilterChanged;

            top = _style.MarginVer;
            left = cbFilterEntities.Right + _style.MarginHor;
            lblFilterName = new Label
            {
                Left = left,
                Top = top,
                Width = 100,
                Text = "Name"
            };
            panelTop.Controls.Add(lblFilterName);
            top += lblFilterName.Height + _style.MarginVerSmall;
            txtFilterName = new TextBox
            {
                Left = left,
                Top = top,
                Width = 100
            };
            panelTop.Controls.Add(txtFilterName);
            txtFilterName.TextChanged += FilterChanged;

            top = _style.MarginVer;
            left = txtFilterName.Right + _style.MarginHor;
            cmbSchemaSources = new ComboBox
            {
                Left = left,
                Top = top,
                Width = _style.ControlWidthDefault,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Title",
                BackColor = panelTop.BackColor
            };
            cmbSchemaSources.DataSource = _schemaSources;
            cmbSchemaSources.SelectedIndexChanged += (sender, e) => { LoadCurrentSchema(); };
            panelTop.Controls.Add(cmbSchemaSources);

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

        private void FilterChanged(object sender, EventArgs e)
        {
            ReloadTypes();
        }

        private void SetAdditionalControls()
        {
            SetControlsTabMain(panelRight);
            SetControlsTopPanel();
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

        private void MakeSchemaChanges()
        {
            
            
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fileName = Path.Combine(DefaultSchemaStorage, "dev.ont");
            //var schemaNew = _schemaService.LoadFromFile(fileName);
            var schemaNew = _schemaService.LoadFromDatabase(_configuration.GetConnectionString("localprod"));
            var schemaOld = _schemaService.LoadFromDatabase(_configuration.GetConnectionString("local"));
            var compareResult = schemaNew.CompareTo(schemaOld);
            txtComparison.Text = GetCompareText(compareResult);
            panelComparison.Visible = true;
            //schemaService.LoadFromFile();
            //schemaService.SaveToFile(_schema, fileName);
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
            sb.AppendLine(GetCompareText("NODES TO UPDATE", compareResult.ItemsToUpdate.Select(item => item.NewNode)));
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

        private void UpdateComparedDatabase()
        {

        }

        private List<OntologySchemaSource> GetSchemaSources()
        {
            var result = new List<OntologySchemaSource> { 
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
}