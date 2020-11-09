using Iis.Services.Contracts.Enums;
using System;
using Microsoft.Extensions.DependencyInjection;
using Iis.Services.Contracts.Interfaces.Elastic;

namespace Iis.Services.Elastic
{
    public class ElasticResponseManagerFactory : IElasticResponseManagerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ElasticResponseManagerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IElasticResponseManager Create(SearchType type)
        {
            return type switch
            {
                SearchType.Ontology => _serviceProvider.GetService<OntologyElasticResponseManager>(),
                SearchType.Material => _serviceProvider.GetService<MaterialElasticResponseManager>(),
                _ => throw new ArgumentException($"{type} is not supported"),
            };
        }
    }
}
