using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Iis.OntologyManager.Style
{
    public class OntologyManagerStyle : IOntologyManagerStyle
    {
        public int MarginVer { get; set; }
        public int MarginVerSmall { get; set; }
        public int MarginHor { get; set; }
        public int ControlWidthDefault { get; set; }
        public int ButtonWidthDefault { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ComparisonBackColor { get; set; }
        public Color EntityTypeBackColor { get; set; }
        public Color AttributeTypeBackColor { get; set; }
        public Color RelationTypeBackColor { get; set; }
        public Font SelectedFont { get; set; }
        public static IOntologyManagerStyle GetDefaultStyle()
        {
            return new OntologyManagerStyle
            {
                MarginVer = 4,
                MarginVerSmall = 2,
                MarginHor = 10,
                ControlWidthDefault = 210,
                ButtonWidthDefault = 100,
                BackgroundColor = Color.LightYellow,
                EntityTypeBackColor = Color.Khaki,
                AttributeTypeBackColor = Color.PaleGreen,
                RelationTypeBackColor = Color.Lavender,
                ComparisonBackColor = Color.Honeydew
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

    }
}
