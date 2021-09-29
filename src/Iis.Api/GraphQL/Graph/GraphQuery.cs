using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.Services.Contracts.Interfaces;
namespace Iis.Api.GraphQL.Graph
{

    public class GraphQuery
    {
        public async Task<GraphResponse> GetGraphData(
            [Service] IGraphService _graphService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
            Guid[] nodeIdList
        )
        {
            var graphDataResult = await _graphService.GetGraphDataForNodeListAsync(nodeIdList);

            return new GraphResponse(mapper.Map<IReadOnlyCollection<GraphLink>>(graphDataResult.LinkList),  mapper.Map<IReadOnlyCollection<GraphNode>>(graphDataResult.NodeList));
        }

    }
}