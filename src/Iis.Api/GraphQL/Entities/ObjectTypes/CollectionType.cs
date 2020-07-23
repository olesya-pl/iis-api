using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using Iis.Domain;
using Iis.Domain.Elastic;
using Iis.Interfaces.Elastic;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    public class CollectionType : ObjectType
    {
        private readonly IOutputType _itemsType;
        private readonly string _itemsTypeName;

        public CollectionType(string itemsTypeName, IOutputType itemsType)
        {
            _itemsTypeName = itemsTypeName;
            _itemsType = itemsType;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name($"{_itemsTypeName}Collection");
            descriptor.Field("count").Type<IntType>()
                .Resolver(async ctx =>
                {
                    var (types, filter, matchList) = ctx.Parent<Tuple<IEnumerable<IEntityTypeModel>, ElasticFilter,IEnumerable<Guid>>>();
                    var service = ctx.Service<IOntologyService>();

                    if(matchList != null && matchList.Any())
                    {
                        var result = await service.GetNodesAsync(matchList);
                        return result.count;
                    }
                    return await service.GetNodesCountAsync(types, filter);
                });
            descriptor.Field("items")
                .Type(new NonNullType(new ListType(new NonNullType(_itemsType))))
                .Resolver(async ctx =>
                {
                    var (types, filter, matchList) = ctx.Parent<Tuple<IEnumerable<IEntityTypeModel>, ElasticFilter, IEnumerable<Guid>>>();
                    var elasticManager = ctx.Service<IElasticManager>();
                    var elasticService = ctx.Service<IElasticService>();

                    if (matchList != null && matchList.Any())
                    {
                        return (await elasticManager.GetDocumentByIdsAsync(
                            elasticService.OntologyIndexes.ToArray(),
                            matchList.Select(p => p.ToString("N")).ToArray()))
                        .Items.Select(p => p.SearchResult);
                    }

                    return (await elasticManager.Search(new IisElasticSearchParams {
                        BaseIndexNames = types.Select(p => p.Name).ToList(),
                        From = filter.Offset,
                        Size = filter.Limit,
                        Query = filter.Suggestion ?? "*",
                        ResultFields = new List<string> { }
                    })).Items.Select(p => p.SearchResult);
                });
        }
    }
}
