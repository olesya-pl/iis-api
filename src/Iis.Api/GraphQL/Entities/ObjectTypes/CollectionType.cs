using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology;
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
                    var (types, filter) = ctx.Parent<Tuple<IEnumerable<EntityType>, NodeFilter>>();
                    var service = ctx.Service<IOntologyService>();
                    return await service.GetNodesCountAsync(types, filter);
                });
            descriptor.Field("items")
                .Type(new NonNullType(new ListType(new NonNullType(_itemsType))))
                .Resolver(async ctx =>
                {
                    var (types, filter) = ctx.Parent<Tuple<IEnumerable<EntityType>, NodeFilter>>();
                    var service = ctx.Service<IOntologyService>();
                    var nodes = await service.GetNodesAsync(types, filter);
                    return nodes.Cast<Entity>();
                });
        }
    }
}
