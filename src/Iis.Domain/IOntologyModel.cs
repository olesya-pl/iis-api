using System;
using System.Collections.Generic;

namespace Iis.Domain
{
    public interface IOntologyModel
    {
        IEnumerable<IEntityTypeModel> EntityTypes { get; }
        IEnumerable<INodeTypeModel> Types { get; }
        IEnumerable<INodeTypeModel> GetChildTypes(INodeTypeModel type);
        IEntityTypeModel GetEntityType(string name);
        INodeTypeModel GetType(Guid id);
        T GetType<T>(string name) where T : INodeTypeModel;
        T GetTypeOrNull<T>(string name) where T : INodeTypeModel;
        IEnumerable<T> GetTypes<T>(string name) where T : INodeTypeModel;
    }
}