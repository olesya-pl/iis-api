using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Newtonsoft.Json.Linq;
using OScalarType = IIS.Core.Ontology.ScalarType;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityAttributeTypeEnum : EnumType
    {
        protected override void Configure(IEnumTypeDescriptor descriptor)
        {
            descriptor.Name("EntityAttributeType"); // Typo on dev: EnityAttributeType 
            descriptor.Item(OScalarType.Integer.ToString()).Name("int");
            descriptor.Item(OScalarType.Decimal.ToString()).Name("float");
            descriptor.Item(OScalarType.String.ToString()).Name("string");
            descriptor.Item(OScalarType.Boolean.ToString()).Name("boolean");
            descriptor.Item(OScalarType.DateTime.ToString()).Name("date");
            descriptor.Item(OScalarType.Geo.ToString()).Name("geo");
            descriptor.Item(OScalarType.File.ToString()).Name("file");
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
        [GraphQLType(typeof(NonNullType<EntityAttributeTypeEnum>))]
        string Type { get; }
        [GraphQLNonNullType]
        string Title { get; }
        [GraphQLNonNullType]
        string Code { get; }
        string Hint { get; }
        bool Multiple { get; }
        string Format { get; }
        [GraphQLType(typeof(JsonScalarType))] JObject FormField { get; }
        [GraphQLType(typeof(JsonScalarType))] JObject Validation { get; }
        [GraphQLType(typeof(JsonScalarType))] JObject Meta { get; }
    }

    public abstract class EntityAttributeBase : IEntityAttribute
    {
        protected EmbeddingRelationType Source { get; }
        protected RelationMetaBase MetaObject { get; }
        
        public EntityAttributeBase(EmbeddingRelationType source)
        {
            Source = source;
            MetaObject = Source.CreateMeta();
        }

        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLType(typeof(NonNullType<EntityAttributeTypeEnum>))]
        public abstract string Type { get; }

        [GraphQLNonNullType]
        public string Title => Source.Title ?? Source.TargetType.Title; // fallback to target type 

        [GraphQLNonNullType]
        public string Code => Source.Name ?? Source.TargetType.Name; // fallback to target type 

        public string Hint => null; // null on dev also
        public bool Multiple => Source.EmbeddingOptions == EmbeddingOptions.Multiple;
        public string Format => Source.Meta?.Value<string>("format");

        [GraphQLType(typeof(JsonScalarType))]
        public JObject Meta => Source.Meta;

        [GraphQLType(typeof(JsonScalarType))]
        public JObject FormField => (JObject) Source.Meta["formField"]; // Source.CreateMeta().FormField;
        
        [GraphQLType(typeof(JsonScalarType))]
        public JObject Validation => (JObject) Source.Meta["validation"];
    }

    public class EntityAttributePrimitive : EntityAttributeBase
    {
        public EntityAttributePrimitive(EmbeddingRelationType source) : base(source){}
        
        public override string Type => Source.AttributeType.ScalarTypeEnum.ToString();
    }


    public class EntityAttributeRelation : EntityAttributeBase
    {
        protected new EntityRelationMeta MetaObject => (EntityRelationMeta) base.MetaObject;
        public EntityAttributeRelation(EmbeddingRelationType source) : base(source){}

        public override string Type => "relation";

        [GraphQLType(typeof(ListType<NonNullType<StringType>>))]
        public IEnumerable<string> AcceptsEntityOperations => MetaObject.AcceptsEntityOperations?.Select(e => e.ToString());

        [GraphQLNonNullType]
        [GraphQLDeprecated("Legacy version which is emulated for union types.")]
        public IEnumerable<EntityType> To([Service] IOntologyRepository repository)
        {
            if (repository.GetEntityType(Source.EntityType.Name) != null)
                return new [] {new EntityType(Source.EntityType)};
            // Only if entity type was not found in ontology root elements,
            // emulate old union behavior, and create response of all child types
            // todo: maybe we should get rid of this behaviour and change response type to single element (parent type)
            var children = repository.GetChildTypes(Source.EntityType);
            return children.Select(t => new EntityType(t));
        }
        
        [GraphQLNonNullType]
        [GraphQLDescription("Retrieves relation target type. Type may be abstract.")]
        public EntityType Target => new EntityType(Source.EntityType);
        
        [GraphQLType(typeof(ListType<NonNullType<ObjectType<EntityType>>>))]
        [GraphQLDescription("Retrieve all possible target types (inheritors of Target type).")]
        public IEnumerable<EntityType> TargetTypes([Service] IOntologyRepository repository, bool? concreteTypes = false)
        {
            var types = repository.GetChildTypes(Source.EntityType)?.OfType<Core.Ontology.EntityType>();
            if (types == null)
                return null;
            if (concreteTypes == true)
                types = types.Where(t => !t.IsAbstract);
            return types.Select(t => new EntityType(t));
        }
    }
}