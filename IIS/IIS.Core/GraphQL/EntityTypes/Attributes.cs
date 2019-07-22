using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.Ontology;
using Newtonsoft.Json.Linq;
using OScalarType = IIS.Core.Ontology.ScalarType;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityAttributeTypeEnum : EnumType
    {
        protected override void Configure(IEnumTypeDescriptor descriptor)
        {
            descriptor.Name("EntityAttributeType"); // Typo on dev: EnityAttributeType 
            descriptor.Item(OScalarType.Integer).Name("int");
            descriptor.Item(OScalarType.Decimal).Name("float");
            descriptor.Item(OScalarType.String).Name("string");
            descriptor.Item(OScalarType.Boolean).Name("boolean");
            descriptor.Item(OScalarType.DateTime).Name("date");
            descriptor.Item(OScalarType.Geo).Name("geo");
            descriptor.Item(OScalarType.File).Name("file");
            descriptor.Item("relation").Name("relation");
        }
    }
    
    public class EntityAttributeType : InterfaceType<IEntityAttribute>
    {
        protected override void Configure(IInterfaceTypeDescriptor<IEntityAttribute> descriptor) =>
            descriptor.Name("EntityAttribute");
    }
    
    public interface IEntityAttribute
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        Guid Id { get; }
//        [GraphQLNonNullType]
        [GraphQLType(typeof(EntityAttributeTypeEnum))]
        object Type { get; }
        [GraphQLNonNullType]
        string Title { get; }
        [GraphQLNonNullType]
        string Code { get; }
        string Hint { get; }
        bool Multiple { get; }
        string Format { get; }
        // TODO: formField: JSON
        // TODO: validation: JSON
        string MetaString { get; }
        [GraphQLType(typeof(JsonScalarType))] JObject Meta { get; }
    }

    public abstract class EntityAttributeBase : IEntityAttribute
    {
        protected EmbeddingRelationType Source { get; }
        
        public EntityAttributeBase(EmbeddingRelationType source)
        {
            Source = source;
        }

        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLType(typeof(EntityAttributeTypeEnum))]
        public abstract object Type { get; }

        [GraphQLNonNullType]
        public string Title => Source.TargetType.Title;

        [GraphQLNonNullType]
        public string Code => Source.TargetType.Name;

        public string Hint => null; // null on dev also
        public bool Multiple => Source.EmbeddingOptions == EmbeddingOptions.Multiple;
        public string Format => Source.Meta?.Value<string>("format");

        [GraphQLName("_targetType")]
        public EntityType TargetType => new EntityType(Source.TargetType);

        public string MetaString => Source.Meta?.ToString(); // absent on schema
        
        [GraphQLType(typeof(JsonScalarType))]
        public JObject Meta => Source.Meta;
    }

    public class EntityAttributePrimitive : EntityAttributeBase
    {
        public EntityAttributePrimitive(EmbeddingRelationType source) : base(source){}
        
        public override object Type => Source.AttributeType.ScalarTypeEnum;
    }


    public class EntityAttributeRelation : EntityAttributeBase
    {
        public EntityAttributeRelation(EmbeddingRelationType source) : base(source){}

        public override object Type => "relation";

        public string[] AcceptsEntityOperations => new[] {"Create", "Read"}; // TODO: take from meta

        public EntityType To => new EntityType(Source.EntityType);
    }
}