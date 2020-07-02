using System;
using System.Collections.Generic;

namespace Iis.Domain
{
    public interface IOntologyModel
    {
        IEnumerable<EntityType> EntityTypes { get; }
        IEnumerable<NodeType> Types { get; }
        IEnumerable<NodeType> GetChildTypes(NodeType type);
        EntityType GetEntityType(string name);
        NodeType GetType(Guid id);
        T GetType<T>(string name) where T : NodeType;
        T GetTypeOrNull<T>(string name) where T : NodeType;
        IEnumerable<T> GetTypes<T>(string name) where T : NodeType;
    }
}