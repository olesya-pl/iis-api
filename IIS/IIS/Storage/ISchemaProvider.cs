using System;
using System.Threading.Tasks;
using GraphQL.Types;

namespace IIS.Storage
{
    public interface ISchemaProvider
    {
        Task<ISchema> GetSchemaAsync();
    }
}
