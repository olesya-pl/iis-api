using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;

namespace Iis.Domain
{
    public interface IOntologyModel
    {
        IEnumerable<INodeTypeLinked> GetEntityTypes();
        INodeTypeLinked GetEntityTypeByName(string name);
    }
}