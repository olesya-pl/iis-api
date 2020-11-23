using System;
using System.Collections.Generic;

namespace Iis.Domain
{
    public interface IOntologyModel
    {
        IEnumerable<INodeTypeModel> EntityTypes { get; }
        IEnumerable<INodeTypeModel> GetChildTypes(INodeTypeModel type);
        INodeTypeModel GetEntityType(string name);
        INodeTypeModel GetType(Guid id);
        IEnumerable<T> GetTypes<T>(string name) where T : INodeTypeModel;
    }
}