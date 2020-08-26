using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.Style
{
    public class OntologyManagerStyle : IOntologyManagerStyle
    {
        public int MarginVer { get; set; }
        public int MarginVerSmall { get; set; }
        public int MarginHor { get; set; }
        public int ControlWidthDefault { get; set; }
        public int ButtonHeightDefault { get; private set; }
        public int ButtonWidthDefault { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ComparisonBackColor { get; set; }
        public Color EntityTypeBackColor { get; set; }
        public Color AttributeTypeBackColor { get; set; }
        public Color RelationTypeBackColor { get; set; }
        public Font DefaultFont { get; set; }
        public Font SelectedFont { get; set; }
        public Font TypeHeaderNameFont { get; set; }
        public static IOntologyManagerStyle GetDefaultStyle()
        {
            return new OntologyManagerStyle
            {
                MarginVer = 16,
                MarginVerSmall = 8,
                MarginHor = 40,
                ControlWidthDefault = 410,
                ButtonHeightDefault = 50,
                ButtonWidthDefault = 200,
                BackgroundColor = Color.LightYellow,
                EntityTypeBackColor = Color.Khaki,
                AttributeTypeBackColor = Color.PaleGreen,
                RelationTypeBackColor = Color.Lavender,
                ComparisonBackColor = Color.Honeydew,
                DefaultFont = SystemFonts.DefaultFont,
                SelectedFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                TypeHeaderNameFont = new Font("Arial", 16, FontStyle.Bold)
        };
        }
        public Color GetColorByNodeType(Kind kind)
        {
            switch (kind)
            {
                case Kind.Entity:
                    return EntityTypeBackColor;
                case Kind.Attribute:
                    return AttributeTypeBackColor;
                case Kind.Relation:
                    return RelationTypeBackColor;
                default:
                    return BackgroundColor;
            }
        }
        public void GridTypes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
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
            style.Font = row.Selected ? this.SelectedFont : this.DefaultFont;
        }
    }
}
