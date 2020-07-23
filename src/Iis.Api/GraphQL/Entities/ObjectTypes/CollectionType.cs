using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using Iis.Domain;

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
                    var service = ctx.Service<IOntologyService>();
                    
                    if(matchList != null && matchList.Any())
                    {
                        var result = await service.GetNodesAsync(matchList);
                        return result.nodes.Cast<Entity>();
                    }
                    
                    var nodes = await service.GetNodesAsync(types, filter);
                    return nodes.Cast<Entity>();
                });
        }
    }
}
