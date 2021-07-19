using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using HotChocolate;
using IIS.Core.GraphQL.Common;
using Iis.Services.Contracts.Interfaces;
namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class SituationQuery
    {
        public async Task<GraphQLCollection<SituationNode>> GetRadioElectronicSituation(
            [Service] IRadioElectronicSituationService service,
            [Service] IMapper mapper
        )
        {
            var dataResult = await service.GetSituationNodesAsync();

            var mappedResult = mapper.Map<IList<SituationNode>>(dataResult);

            return new GraphQLCollection<SituationNode>(mappedResult, mappedResult.Count);
        }
    }
}