using System.Collections;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Common
{
    public class CollectionType : ObjectType
    {
        private readonly string _itemsTypeName;
        private readonly IOutputType _itemsType;

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
                .Resolver(ctx => ctx.Parent<ICollection>().Count);
            descriptor.Field("items")
                .Type(new NonNullType(new ListType(new NonNullType(_itemsType))))
                .Resolver(ctx => ctx.Parent<object>());
        }
    }

    public class CollectionResolvers
    {
        public int Count([Parent] ICollection collection) => collection.Count;
    }
}