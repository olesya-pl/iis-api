using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.UiControls
{
    public class AddPanelOptions
    {
        public int? Width { get; set; } = null;
        public int? Height { get; set; } = null;
        public bool FitWidth { get; set; } = false;
        public bool FitHeight { get; set; } = false;
    }
}
