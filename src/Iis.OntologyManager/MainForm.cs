using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;
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
        const int MARGIN_VER = 4;
        const int MARGIN_VER_SMALL = 2;
        const int MARGIN_HOR = 10;
        INodeTypeLinked SelectedNodeType
        {
            get
            {
                return gridTypes.SelectedRows.Count == 0 ?
                    null :
                    (INodeTypeLinked)gridTypes.SelectedRows[0].DataBoundItem;
            }
        }
        public MainForm(OntologyContext context, IOntologySchema schema)
        {
            InitializeComponent();
            _context = context;
            _schema = schema;
            InitSchema();
            SetGridTypesStyle();
            SetAdditionalControls();
            //ReloadTypes();
        }

        private void SetGridTypesStyle()
        {
            gridTypes.ColumnCount = 1;
            gridTypes.Columns[0].DataPropertyName = "Name";
            gridTypes.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            gridTypes.AllowUserToResizeRows = false;
            gridTypes.MultiSelect = false;
            gridTypes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void SetControlsTabMain()
        {
            var pnlChildren = new Panel { Dock = DockStyle.Fill };
            const int WIDTH_DEFAULT = 200;
            tabNodeTypeMain.Controls.Add(pnlChildren);
            pnlChildren.SuspendLayout();

            var top = MARGIN_VER;
            lblId = new Label
            {
                Left = MARGIN_HOR,
                Top = top,
                Width = WIDTH_DEFAULT,
                Text = "Id"
            };
            pnlChildren.Controls.Add(lblId);
            top += lblId.Height + MARGIN_VER_SMALL;
            txtId = new TextBox
            {
                Left = MARGIN_HOR,
                Top = top,
                Width = WIDTH_DEFAULT,
                Enabled = false
            };
            pnlChildren.Controls.Add(txtId);
            top += txtId.Height + MARGIN_VER;
            lblName = new Label
            {
                Left = MARGIN_HOR,
                Top = top,
                Width = WIDTH_DEFAULT,
                Text = "Name"
            };
            pnlChildren.Controls.Add(lblName);
            top += lblName.Height + MARGIN_VER_SMALL;
            txtName = new TextBox
            {
                Left = MARGIN_HOR,
                Top = top,
                Width = WIDTH_DEFAULT
            };
            pnlChildren.Controls.Add(txtName);

            top += txtName.Height + MARGIN_VER;
            lblTitle = new Label
            {
                Left = MARGIN_HOR,
                Top = top,
                Width = WIDTH_DEFAULT,
                Text = "Title"
            };
            pnlChildren.Controls.Add(lblTitle);
            top += lblTitle.Height + MARGIN_VER_SMALL;
            txtTitle = new TextBox
            {
                Left = MARGIN_HOR,
                Top = top,
                Width = WIDTH_DEFAULT
            };
            pnlChildren.Controls.Add(txtTitle);
            pnlChildren.ResumeLayout();
        }

        private void SetControlsFilters()
        {
            var top = MARGIN_VER;
            var left = MARGIN_HOR;
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
            top += cbFilterEntities.Height + MARGIN_VER_SMALL;

            cbFilterAttributes = new CheckBox
            {
                Left = left,
                Top = top,
                Width = 100,
                Text = "Attributes",
                Checked = false
            };
            panelTop.Controls.Add(cbFilterAttributes);
            top += cbFilterAttributes.Height + MARGIN_VER_SMALL;

            cbFilterRelations = new CheckBox
            {
                Left = left,
                Top = top,
                Width = 100,
                Text = "Relations",
                Checked = false
            };
            panelTop.Controls.Add(cbFilterRelations);
            top += cbFilterAttributes.Height + MARGIN_VER_SMALL;

            panelTop.ResumeLayout();
        }

        private void SetAdditionalControls()
        {
            SetControlsTabMain();
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
                Kinds = kinds
            };
        }

        public void ReloadTypes()
        {
            var filter = GetFilter();
            var ds = _schema.GetTypes(filter).ToList();
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
            txtId.Text = nodeType.Id.ToString();
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
        }

        private Color GetColorByNodeType(INodeTypeLinked nodeType)
        {
            switch (nodeType.Kind)
            {
                case Kind.Entity:
                    return Color.Moccasin;
                case Kind.Attribute:
                    return Color.YellowGreen;
                case Kind.Relation:
                    return Color.Lavender;
                default:
                    return Color.Gray;
            }
        }

        private void gridTypes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            return;
            var nodeType = (INodeTypeLinked)gridTypes.Rows[e.RowIndex].DataBoundItem;
            if (nodeType == null) return;
            gridTypes.Rows[e.RowIndex].DefaultCellStyle.BackColor = GetColorByNodeType(nodeType);
        }
    }
}
