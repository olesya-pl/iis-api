using System.Collections;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Common
{
    public class CollectionType : ObjectType
    {
        private readonly string _itemsTypeName;
        private readonly ObjectType _itemsType;

        public CollectionType(string itemsTypeName, ObjectType itemsType)
        {
            _itemsTypeName = itemsTypeName;
            _itemsType = itemsType;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name($"{_itemsTypeName}Collection");
            descriptor.Field("meta")
                .Type<NonNullType<ObjectType<CollectionMeta>>>()
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