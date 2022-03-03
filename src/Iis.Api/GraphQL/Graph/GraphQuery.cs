using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL;
using Iis.Services.Contracts.Interfaces;

namespace Iis.Api.GraphQL.Graph
{
    public class GraphQuery
    {
        public async Task<GraphResponse> GetGraphData(
            IResolverContext context,
            [Service] IGraphService graphService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
            Guid[] nodeIdList,
            CancellationToken cancellationToken = default)
        {
            var user = context.GetToken().User;
            var graphDataResult = await graphService.GetGraphDataForNodeListAsync(nodeIdList, user, cancellationToken);

            return new GraphResponse(mapper.Map<IReadOnlyCollection<GraphLink>>(graphDataResult.LinkList),  mapper.Map<IReadOnlyCollection<GraphNode>>(graphDataResult.NodeList));
        }
    }
}