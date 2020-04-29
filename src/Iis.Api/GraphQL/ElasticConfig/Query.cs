using AutoMapper;
using HotChocolate;
using Iis.Interfaces.Elastic;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.ElasticConfig
{
    public class Query
    {
        public Task<GraphQLCollection<ElasticField>> GetElasticFields([Service] IElasticConfiguration configuration, [Service] IMapper mapper,
            [GraphQLNonNullType] string typeName)
        {
            var elasticFields = configuration.GetIncludedFieldsByTypeNames(new[] { typeName });
            var result = elasticFields.Select(ef => mapper.Map<ElasticField>(ef)).ToList();
            return Task.FromResult(new GraphQLCollection<ElasticField>(result, result.Count));
        }
    }
}
