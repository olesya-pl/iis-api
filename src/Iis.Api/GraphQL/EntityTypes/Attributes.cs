using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.Ontology;
using Iis.Domain;
using Iis.Domain.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OScalarType = Iis.Interfaces.Ontology.Schema.ScalarType;
using IIS.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Meta;
using AutoMapper;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityAttributeTypeEnum : EnumType
    {
        protected override void Configure(IEnumTypeDescriptor descriptor)
        {
            descriptor.Name("EntityAttributeType");
            descriptor.Item(OScalarType.Int.ToString()).Name("int");
            descriptor.Item(OScalarType.Decimal.ToString()).Name("float");
            descriptor.Item(OScalarType.String.ToString()).Name("string");
            descriptor.Item(OScalarType.Boolean.ToString()).Name("boolean");
            descriptor.Item(OScalarType.Date.ToString()).Name("date");
            descriptor.Item(OScalarType.Geo.ToString()).Name("geo");
            descriptor.Item(OScalarType.File.ToString()).Name("file");
            descriptor.Item(OScalarType.IntegerRange.ToString()).Name("integerRange");
            descriptor.Item(OScalarType.FloatRange.ToString()).Name("floatRange");
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
        [GraphQLNonNullType] bool IsLinkToObjectOfStudy { get; }

        bool Multiple { get; }
        string Format { get; }
        [GraphQLType(typeof(AnyType))] FormField FormField { get; }
        [GraphQLType(typeof(AnyType))] ContainerMeta Container { get; }

        int? SortOrder { get; }

        [GraphQLType(typeof(AnyType))] Validation Validation { get; }
    }

    public abstract class EntityAttributeBase : IEntityAttribute
    {
        public EntityAttributeBase(IEmbeddingRelationTypeModel source)
        {
            Source = source;
            MetaObject = Source.EmbeddingMeta;

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IFormField, FormField>();
                cfg.CreateMap<IContainerMeta, ContainerMeta>();
                cfg.CreateMap<IValidation, Validation>();
            });

            _mapper = new Mapper(configuration);
        }
        protected IMapper _mapper;
        protected IEmbeddingRelationTypeModel Source { get; }
        protected IRelationMetaBase MetaObject { get; }

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
        [GraphQLNonNullType] public bool IsLinkToObjectOfStudy => Source.TargetType.IsObjectOfStudy;

        [GraphQLType(typeof(AnyType))]
        public FormField FormField => _mapper.Map<FormField>(MetaObject?.FormField);

        [GraphQLType(typeof(AnyType))]
        public ContainerMeta Container => _mapper.Map<ContainerMeta>(MetaObject?.Container);

        public int? SortOrder => MetaObject?.SortOrder;

        [GraphQLType(typeof(AnyType))]
        public Validation Validation {
            get {
                var validation = _mapper.Map<Validation>(MetaObject?.Validation);

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
        public EntityAttributePrimitive(IEmbeddingRelationTypeModel source) : base(source)
        {
        }

        public override string Type => Source.AttributeType.ScalarTypeEnum.ToString();
    }


    public class EntityAttributeRelation : EntityAttributeBase
    {
        public EntityAttributeRelation(IEmbeddingRelationTypeModel source, IOntologyModel ontology) : base(source)
        {
            _ontology = ontology;
        }

        private IOntologyModel _ontology;

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
        public async Task<IEnumerable<EntityType>> TargetTypes([Service] IOntologyModel ontology)
        {
            var types = ontology.GetChildTypes(Source.EntityType)?.OfType<IEntityTypeModel>();
            if (types == null)
                types = new[] {Source.EntityType };
            else if (!types.Any(t => t.Id == Source.EntityType.Id))
                types = types.Union(new[] {Source.EntityType });

            var metaTargetTypes = (Source.Meta as IEntityRelationMeta)?.TargetTypes;
            if (metaTargetTypes != null && metaTargetTypes.Length > 0)
            {
                types = types.Where(t => metaTargetTypes.Contains(t.Name));
            }

            return types.Where(t => !t.IsAbstract).Select(t => new EntityType(t, _ontology));
        }
    }
}
