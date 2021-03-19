using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.UiControls
{
    public interface IDataViewControl
    {
        bool Visible { get; set; }
        void SetUiValues(INodeTypeLinked nodeType, IOntologyNodesData ontologyData);
    }
}
