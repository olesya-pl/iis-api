using Iis.Interfaces.Ontology.Schema;
using System.Drawing;

namespace Iis.OntologyManager.Style
{
    public interface IOntologyManagerStyle
    {
        Color AttributeTypeBackColor { get; }
        Color BackgroundColor { get; }
        Color ComparisonBackColor { get; set; }
        int ControlWidthDefault { get; }
        int ButtonWidthDefault { get; }
        Color EntityTypeBackColor { get; }
        int MarginHor { get; }
        int MarginVer { get; }
        int MarginVerSmall { get; }
        Color RelationTypeBackColor { get; }
        Color GetColorByNodeType(Kind kind);
        Font DefaultFont { get; }
        Font SelectedFont { get; }
        Font TypeHeaderNameFont { get; }
    }
}