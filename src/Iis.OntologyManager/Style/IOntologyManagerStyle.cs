using Iis.Desktop.Common.Styles;
using Iis.Interfaces.Ontology.Schema;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Iis.OntologyManager.Style
{
    public interface IOntologyManagerStyle
    {
        IDesktopStyle Common { get; }
        Color AttributeTypeBackColor { get; }
        Color ComparisonBackColor { get; set; }
        int ButtonWidthDefault { get; }
        int CheckboxHeightDefault { get; }
        Color EntityTypeBackColor { get; }
        Color RelationTypeBackColor { get; }
        Font DefaultFont { get; }
        Font SelectedFont { get; }
        Font TypeHeaderNameFont { get; }
        Dictionary<string, Color> EntityColors { get; }
        Color EntityOtherColor { get; }
        Color GetColorByNodeTypeKind(Kind kind);
        Color GetColorByAncestor(INodeTypeLinked nodeType);
        void GridTypes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e);

    }
}