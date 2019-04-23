using System.Collections.Generic;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;

namespace IIS.Storage.EntityFramework
{
    public interface ISchemaGenerator
    {
        ISchema Generate(IEnumerable<EntityType> entityTypes, string rootName);
    }
}
