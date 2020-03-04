using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface ISchemaEntityTypeFinder
    {
        INodeTypeLinked GetEntityTypeByName(string entityTypeName);
    }
}
