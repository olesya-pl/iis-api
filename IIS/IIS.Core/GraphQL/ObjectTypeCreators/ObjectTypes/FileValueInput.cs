using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes
{
    // This class represents Ontology.ScalarType.File input type
//    public class FileValueInput
//    {
//        [GraphQLType(typeof(IdType))] public Guid FileId { get; set; }
//        [GraphQLNonNullType] public string Title { get; set; }
//        public string Type { get; set; }
//    }

    public class FileValueInputType : InputObjectType
    {
        protected override void Configure(IInputObjectTypeDescriptor descriptor)
        {
            descriptor.Field("fileId").Type<IdType>();
            descriptor.Field("title").Type<NonNullType<StringType>>();
            descriptor.Field("type").Type<StringType>();
        }
    }
}
