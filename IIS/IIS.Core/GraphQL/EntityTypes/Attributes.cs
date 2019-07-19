using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.Ontology;
using Newtonsoft.Json.Linq;
using Attribute = IIS.Core.Ontology.Attribute;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityAttributeTypeEnum : EnumType
    {
        protected override void Configure(IEnumTypeDescriptor descriptor)
        {
            descriptor.Name("EntityAttributeType"); // Typo on dev: EnityAttributeType 
            descriptor.Item("int");
            descriptor.Item("float");
            descriptor.Item("string");
            descriptor.Item("boolean");
            descriptor.Item("date");
            descriptor.Item("geo");
            descriptor.Item("relation");
            descriptor.Item("file");
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
        string Type { get; }
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
        public abstract string Type { get; }

        [GraphQLNonNullType]
        public string Title => Source.TargetType.Title;

        [GraphQLNonNullType]
        public string Code => Source.TargetType.Name;

        public string Hint => null; // null on dev also
        public bool Multiple => Source.IsArray;
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
        
        public override string Type => Source.AttributeType.ScalarType;
    }


    public class EntityAttributeRelation : EntityAttributeBase
    {
        public EntityAttributeRelation(EmbeddingRelationType source) : base(source){}

        public override string Type => "relation";

        public string[] AcceptsEntityOperations => new[] {"Create", "Read"}; // TODO: take from meta

        public EntityType To => new EntityType(Source.EntityType);
    }
}