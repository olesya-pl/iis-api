using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;

namespace Iis.Domain
{
    public interface IOntologyModel
    {
        IEnumerable<INodeTypeLinked> EntityTypes { get; }
        IEnumerable<INodeTypeLinked> GetChildTypes(INodeTypeLinked type);
        INodeTypeLinked GetEntityType(string name);
        INodeTypeLinked GetType(Guid id);
        IEnumerable<T> GetTypes<T>(string name) where T : INodeTypeLinked;
    }
}