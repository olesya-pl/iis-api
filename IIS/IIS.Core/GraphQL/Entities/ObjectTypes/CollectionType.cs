using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;

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
            descriptor.Field("meta")
                .Deprecated("Use direct count property - get rid of useless meta")
                .Type<NonNullType<ObjectType<CollectionMeta>>>()
                .Resolver(ctx => new CollectionMeta(ctx.Parent<ICollection>().Count));
            descriptor.Field("count")
                .Resolver(ctx => ctx.Parent<IEnumerable<object>>().Count());
            descriptor.Field("items")
                .Type(new NonNullType(new ListType(new NonNullType(_itemsType))))
                .Resolver(ctx => ctx.Parent<object>());
        }
    }
}
