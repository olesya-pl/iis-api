using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.UiControls
{
    public interface IUiNodeTypeControl
    {
        void SetUiValues(INodeTypeLinked nodeType);
        void CreateNew();
        void SetParentTypeId(Guid? parentTypeId);
        bool Visible { get; set; }
    }
}
