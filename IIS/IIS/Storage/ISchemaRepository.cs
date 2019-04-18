using System;
using System.Threading.Tasks;

namespace IIS.Storage
{
    public interface ISchemaRepository
    {
        Task<object> Ping();
    }
}
