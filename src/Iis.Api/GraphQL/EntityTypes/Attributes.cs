using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Iis.Domain;
using Iis.Domain.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OScalarType = Iis.Domain.ScalarType;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityAttributeTypeEnum : EnumType
    {
        protected override void Configure(IEnumTypeDescriptor descriptor)
        {
            descriptor.Name("EntityAttributeType");
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
        protected override void Configure(IInterfaceTypeDescriptor<IEntityAttribute> descriptor)
        {
            descriptor.Name("EntityAttribute");
        }
    }

    public interface IEntityAttribute
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        Guid Id { get; }

        [GraphQLType(typeof(NonNullType<EntityAttributeTypeEnum>))]
        string Type { get; }

        [GraphQLNonNullType] string Title { get; }

        [GraphQLNonNullType] string Code { get; }

        [GraphQLNonNullType] bool Editable { get; }

        bool Multiple { get; }
        string Format { get; }
        [GraphQLType(typeof(AnyType))] FormField FormField { get; }
        [GraphQLType(typeof(AnyType))] IValidation Validation { get; }
    }

    public abstract class EntityAttributeBase : IEntityAttribute
    {
        public EntityAttributeBase(EmbeddingRelationType source)
        {
            Source = source;
            MetaObject = Source.EmbeddingMeta;
        }

        protected EmbeddingRelationType Source { get; }
        protected RelationMetaBase MetaObject { get; }

        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLType(typeof(NonNullType<EntityAttributeTypeEnum>))]
        public abstract string Type { get; }

        [GraphQLNonNullType] public string Title => Source.Title ?? Source.TargetType.Title; // fallback to target type

        [GraphQLNonNullType] public string Code => Source.Name ?? Source.TargetType.Name; // fallback to target type

        public bool Editable => !(Source.IsInversed || Source.IsComputed());
        public bool IsInversed => Source.IsInversed;
        public bool IsComputed => Source.IsComputed();

        public bool Multiple => Source.EmbeddingOptions == EmbeddingOptions.Multiple;
        public string Format => (MetaObject as AttributeRelationMeta)?.Format;

        [GraphQLType(typeof(AnyType))]
        public FormField FormField => MetaObject?.FormField;

        [GraphQLType(typeof(AnyType))]
        public IValidation Validation {
            get {
                var validation = MetaObject?.Validation;

                if (Source.EmbeddingOptions == EmbeddingOptions.Required)
                {
                    validation = validation ?? new Validation();
                    validation.Required = true;
                }

                return validation;
            }
        }
    }

    public class EntityAttributePrimitive : EntityAttributeBase
    {
        public EntityAttributePrimitive(EmbeddingRelationType source) : base(source)
        {
        }

        public override string Type => Source.AttributeType.ScalarTypeEnum.ToString();
    }


    public class EntityAttributeRelation : EntityAttributeBase
    {
        public EntityAttributeRelation(EmbeddingRelationType source, OntologyModel ontology) : base(source)
        {
            _ontology = ontology;
        }

        private OntologyModel _ontology;

        protected new EntityRelationMeta MetaObject => (EntityRelationMeta) base.MetaObject;

        public override string Type => "relation";

        [GraphQLType(typeof(NonNullType<ListType<NonNullType<StringType>>>))]
        public IEnumerable<string> AcceptsEntityOperations =>
            (Source.GetOperations()?? new EntityOperation[]{})
                .Select(e => e.ToString().ToLower());

        [GraphQLNonNullType]
        [GraphQLDescription("Retrieves relation target type. Type may be abstract.")]
        public EntityType Target => new EntityType(Source.EntityType, _ontology);

        [GraphQLType(typeof(ListType<NonNullType<ObjectType<EntityType>>>))]
        [GraphQLDescription("Retrieve all possible target types (inheritors of Target type).")]
        public async Task<IEnumerable<EntityType>> TargetTypes([Service] IOntologyProvider ontologyProvider,
            bool? concreteTypes = false)
        {
            var ontology = await ontologyProvider.GetOntologyAsync();
            var types = ontology.GetChildTypes(Source.EntityType)?.OfType<Iis.Domain.EntityType>();
            if (types == null)
                types = new[] {Source.EntityType};
            else
                types = types.Union(new[] {Source.EntityType});
            if (concreteTypes == true)
                types = types.Where(t => !t.IsAbstract);
            return types.Select(t => new EntityType(t, _ontology));
        }
    }
}