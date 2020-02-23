using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Iis.OntologySchema.DataTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Iis.OntologyManager
{
    public partial class MainForm : Form
    {
        OntologyContext _context;
        IOntologySchema _schema;
        IOntologyManagerStyle _style;
        Font SelectedFont { get; set; }
        Font TypeHeaderNameFont { get; set; }
        INodeTypeLinked SelectedNodeType
        {
            get
            {
                return gridTypes.SelectedRows.Count == 0 ?
                    null :
                    (INodeTypeLinked)gridTypes.SelectedRows[0].DataBoundItem;
            }
        }
        public MainForm(OntologyContext context, IOntologySchema schema, IOntologyManagerStyle style)
        {
            InitializeComponent();
            _context = context;
            _schema = schema;
            _style = style;
            SelectedFont = new Font(DefaultFont, FontStyle.Bold);
            TypeHeaderNameFont = new Font("Arial", 16, FontStyle.Bold);
            InitSchema();
            SetBackColor();
            SetGridTypesStyle();
            SetAdditionalControls();
            ReloadTypes();
        }

        private void SetBackColor()
        {
            panelTop.BackColor = _style.BackgroundColor;
            panelMain.BackColor = _style.BackgroundColor;
            panelLeft.BackColor = _style.BackgroundColor;
            panelRight.BackColor = _style.BackgroundColor;
        }

        private void SetGridTypesStyle()
        {
            gridTypes.ColumnCount = 1;
            gridTypes.Columns[0].DataPropertyName = "Name";
            gridTypes.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            gridTypes.AllowUserToResizeRows = false;
            gridTypes.MultiSelect = false;
            gridTypes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridTypes.ForeColor = Color.Black;
        }

        private DataGridView GetDataGridView(string name, Point location, List<string> dataNames)
        {
            var grid = new DataGridView
            {
                AutoGenerateColumns = false,
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                Dock = DockStyle.None,
                Location = location,
                Name = name,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                ReadOnly = true,
                ColumnCount = dataNames.Count,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = _style.BackgroundColor
            };
            grid.DefaultCellStyle.SelectionBackColor = grid.DefaultCellStyle.BackColor;
            grid.DefaultCellStyle.SelectionForeColor = grid.DefaultCellStyle.ForeColor;

            for (int i = 0; i < dataNames.Count; i++)
            {
                grid.Columns[i].DataPropertyName = dataNames[i];
                grid.Columns[i].HeaderText = dataNames[i];
                grid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            grid.CellFormatting += new DataGridViewCellFormattingEventHandler(gridChildren_CellFormatting);
            return grid;
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

            var top = _style.MarginVer;
            lblId = new Label
            {
                Left = _style.MarginHor,
                Top = top,
                Width = _style.ControlWidthDefault,
                Text = "Id"
            };
            rootPanel.Controls.Add(lblId);
            top += lblId.Height + _style.MarginVerSmall;
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
            top += lblName.Height + _style.MarginVerSmall;
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
            top += lblTitle.Height + _style.MarginVerSmall;
            txtTitle = new TextBox
            {
                Left = _style.MarginHor,
                Top = top,
                Width = _style.ControlWidthDefault
            };
            rootPanel.Controls.Add(txtTitle);
            top += txtTitle.Height + _style.MarginVerSmall;

            gridChildren = GetDataGridView("gridChildren", new Point(0, top), 
                new List<string> { "RelationName", "RelationTitle", "Name", "EmbeddingOptions" });
            gridChildren.Width = rootPanel.Width;
            gridChildren.Height = rootPanel.Bottom - top - _style.MarginVer * 2;
            gridChildren.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            
            gridChildren.ColumnHeadersVisible = true;
            gridChildren.AutoGenerateColumns = false;
            rootPanel.Controls.Add(gridChildren);

            rootPanel.ResumeLayout();
        }

        private void SetControlsFilters()
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

            panelTop.ResumeLayout();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            ReloadTypes();
        }

        private void SetAdditionalControls()
        {
            SetControlsTabMain(panelRight);
            SetControlsFilters();
        }

        private void InitSchema()
        {
            _schema.Initialize(_context.NodeTypes.ToList(), _context.RelationTypes.ToList(), _context.AttributeTypes.ToList());
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
            var filter = GetFilter();
            var ds = _schema.GetTypes(filter)
                .OrderBy(t => t.Name)
                .ToList();
            this.gridTypes.DataSource = ds;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReloadTypes();
        }

        private void gridTypes_SelectionChanged(object sender, EventArgs e)
        {
            if (SelectedNodeType == null) return;
            SetNodeTypeView(SelectedNodeType);
        }

        private void SetNodeTypeView(INodeTypeLinked nodeType)
        {
            lblTypeHeaderName.Text = nodeType.Name;
            txtId.Text = nodeType.Id.ToString();
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            var children = nodeType.GetChildren();
            //var children = new List<TestA> { new TestA { Id = 1, Name = "fff" } };
            gridChildren.DataSource = children;
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
    }

    public class TestA
    {
        public int Id { get; set; }
        public string Name { get; set; }
    };
}
