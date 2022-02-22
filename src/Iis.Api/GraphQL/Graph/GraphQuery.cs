using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL;
using Iis.Domain;
using Iis.Interfaces.SecurityLevels;
using Iis.Services.Contracts.Interfaces;
namespace Iis.Api.GraphQL.Graph
{

    public class GraphQuery
    {
        public async Task<GraphResponse> GetGraphData(
            [Service] IGraphService graphService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
            Guid[] nodeIdList,
            [Service] IOntologyService ontologyService,
            [Service] ISecurityLevelChecker securityLevelChecker,
            IResolverContext context)
        {
            var user = context.GetToken().User;
            var graphDataResult = await graphService.GetGraphDataForNodeListAsync(nodeIdList, securityLevelChecker, ontologyService, user);

            return new GraphResponse(mapper.Map<IReadOnlyCollection<GraphLink>>(graphDataResult.LinkList),  mapper.Map<IReadOnlyCollection<GraphNode>>(graphDataResult.NodeList));
        }
    }
}