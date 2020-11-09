using Iis.Services.Contracts.Enums;

namespace Iis.Services.Contracts.Interfaces.Elastic
{
    public interface IElasticResponseManagerFactory
    {
        IElasticResponseManager Create(SearchType type);
    }
}