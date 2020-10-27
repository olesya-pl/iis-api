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
            SourceOld = source;
            MetaObjectOld = SourceOld.EmbeddingMeta;

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IFormField, FormField>();
                cfg.CreateMap<IContainerMeta, ContainerMeta>();
                cfg.CreateMap<IValidation, Validation>();
            });

            _mapper = new Mapper(configuration);
        }
        public EntityAttributeBase(INodeTypeLinked source)
        {
            Source = source;

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IFormField, FormField>();
                cfg.CreateMap<IContainerMeta, ContainerMeta>();
                cfg.CreateMap<IValidation, Validation>();
            });

            _mapper = new Mapper(configuration);
        }
        protected IMapper _mapper;
        protected IEmbeddingRelationTypeModel SourceOld { get; }
        protected IRelationMetaBase MetaObjectOld { get; }
        protected INodeTypeLinked Source { get; }

        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLType(typeof(NonNullType<EntityAttributeTypeEnum>))]
        public abstract string Type { get; }

        [GraphQLNonNullType] public string Title => Source.Title ?? Source.RelationType.TargetType?.Title; // fallback to target type

        [GraphQLNonNullType] public string Code => Source.Name ?? Source.RelationType.TargetType?.Name; // fallback to target type

        public bool Editable => !(IsInversed || IsComputed);
        public bool IsInversed => Source.IsInversed;
        public bool IsComputed => Source.IsComputed;

        public bool Multiple => Source.RelationType.EmbeddingOptions == EmbeddingOptions.Multiple;
        public string Format => Source.MetaObject?.Format;
        [GraphQLNonNullType] public bool IsLinkToObjectOfStudy => Source.RelationType.TargetType.IsObjectOfStudy;

        [GraphQLType(typeof(AnyType))]
        public FormField FormField => _mapper.Map<FormField>(Source.MetaObject.FormField);

        [GraphQLType(typeof(AnyType))]
        public ContainerMeta Container => _mapper.Map<ContainerMeta>(Source.MetaObject.Container);

        public int? SortOrder => Source.MetaObject.SortOrder;

        [GraphQLType(typeof(AnyType))]
        public Validation Validation {
            get {
                var validation = _mapper.Map<Validation>(Source.MetaObject.Validation);

                if (Source.RelationType.EmbeddingOptions == EmbeddingOptions.Required)
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
        public EntityAttributePrimitive(INodeTypeLinked source) : base(source)
        {
        }

        public override string Type => Source.RelationType.TargetType.AttributeType.ScalarType.ToString();
    }


    public class EntityAttributeRelation : EntityAttributeBase
    {
        public EntityAttributeRelation(INodeTypeLinked source): base(source) { }

        public override string Type => "relation";

        [GraphQLType(typeof(NonNullType<ListType<NonNullType<StringType>>>))]
        public IEnumerable<string> AcceptsEntityOperations =>
            (SourceOld.GetOperations()?? new EntityOperation[]{})
                .Select(e => e.ToString().ToLower());

        [GraphQLNonNullType]
        [GraphQLDescription("Retrieves relation target type. Type may be abstract.")]
        public EntityType Target => new EntityType(Source.RelationType.TargetType);

        [GraphQLType(typeof(ListType<NonNullType<ObjectType<EntityType>>>))]
        [GraphQLDescription("Retrieve all possible target types (inheritors of Target type).")]
        public async Task<IEnumerable<EntityType>> TargetTypes([Service] IOntologyModel ontology,
            bool? concreteTypes = false)
        {
            var types = Source.GetAllDescendants()
                .Where(nt => concreteTypes != true || !nt.IsAbstract);
            return types.Select(nt => new EntityType(nt));
        }
    }
}
