using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.Ontology;
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
    }

    public class EntityAttributePrimitive : IEntityAttribute
    {
        protected EmbeddingRelationType Source { get; }

        public EntityAttributePrimitive(EmbeddingRelationType source)
        {
            Source = source;
        }
        
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLType(typeof(EntityAttributeTypeEnum))]
        public string Type => Source.AttributeType.ScalarType; //Source.Attribute.Type.ToString();
            
        [GraphQLNonNullType]
        public string Title => Source.Title;
            
        [GraphQLNonNullType]
        public string Code => Source.Name;

        public string Hint => null; // null on dev also

        public bool Multiple => Source.IsArray;
            
        public string Format => Source.Meta?.Value<string>("format");
            
        // TODO: formField: JSON
        // TODO: validation: JSON
        
        [GraphQLName("_relationInfo")]
        public EntityType AttributeType => new EntityType(Source.AttributeType);

        public string MetaJson => Source.Meta.ToString(); // absent on schema
    }

    public class EntityAttributeRelation : EntityAttributePrimitive
    {
        public EntityAttributeRelation(EmbeddingRelationType source) : base(source){}

        public string[] AcceptsEntityOperations => new[] {"Create", "Read"};

        public EntityType To() => new EntityType(null); // todo
    }
}