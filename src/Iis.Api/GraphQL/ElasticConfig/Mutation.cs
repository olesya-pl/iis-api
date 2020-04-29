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
    public class Mutation
    {
        public async Task<GraphQLCollection<ElasticField>> UpdateElasticFields(
            [Service] IElasticConfiguration configuration,
            [Service] IIisElasticConfigService service,
            [Service] IMapper mapper,
            [GraphQLNonNullType] ElasticFieldsInput input)
        {
            var fieldEntities = await service.SaveElasticFieldsAsync(input.TypeName, input.Fields);
            configuration.ReloadFields(fieldEntities, input.TypeName);
            var result = fieldEntities.Select(ef => mapper.Map<ElasticField>(ef)).ToList();
            return new GraphQLCollection<ElasticField>(result, result.Count);
        }
    }
}
