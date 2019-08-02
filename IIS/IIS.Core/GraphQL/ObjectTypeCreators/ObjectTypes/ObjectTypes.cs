using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes
{
    // Explicit interface declaration, that would be implemented by each EntityType
    public class EntityInterface : InterfaceType
    {
        protected override void Configure(IInterfaceTypeDescriptor descriptor)
        {
            descriptor.Field("id").Type<NonNullType<IdType>>();
            descriptor.Field("createdAt").Type<NonNullType<DateTimeType>>();
            descriptor.Field("updatedAt").Type<NonNullType<DateTimeType>>();
            descriptor.Field("_relation").Type<Entities.RelationType>();
            descriptor.Description("Interface that is implemented by each ontology type");
        }
    }

    public interface IOntologyType : IOutputType, INamedType
    {
    }

    public class OntologyInterfaceType : InterfaceType, IOntologyType
    {
        public OntologyInterfaceType(Action<IInterfaceTypeDescriptor> configure) : base(configure)
        {
        }
    }
    
    public class OntologyObjectType : ObjectType, IOntologyType
    {
        public OntologyObjectType(Action<IObjectTypeDescriptor> configure) : base(configure)
        {
        }
    }
    
    public class OutputUnionType : UnionType
    {
        public static string GetName(EntityType source, string relationName) =>
            $"{source.Name}_{relationName}_Union";
        
        private readonly EntityType _source;
        private readonly IEnumerable<ObjectType> _objectTypes;
        private string _relationName;

        public OutputUnionType(EntityType source, string relationName, IEnumerable<ObjectType> objectTypes)
        {
            _source = source;
            _relationName = relationName;
            _objectTypes = objectTypes;
            if (_objectTypes.Count() < 2)
                throw new ArgumentException("Two or more types are required to create a union.");
        }

        protected override void Configure(IUnionTypeDescriptor d)
        {
            d.Name(GetName(_source, _relationName));
            foreach (var type in _objectTypes)
                d.Type(type);
        }
    }
    
    // ----- Mutator types ----- //
    
    public class MutatorInputType : InputObjectType
    {
        public static string GetName(Operation operation, string entityTypeName) => $"{operation}{entityTypeName}Input";
        public MutatorInputType(Action<IInputObjectTypeDescriptor> configure) : base(configure)
        {
        }
    }

    public class MutatorResponseType : ObjectType
    {
        public static string GetName(Operation operation, string entityTypeName) => $"{operation}{entityTypeName}Response";

        private readonly Operation _operation;
        private readonly string _entityTypeName;
        private readonly IOutputType _ontologyType;

        public MutatorResponseType(Operation operation, string entityTypeName, IOutputType ontologyType)
        {
            _operation = operation;
            _entityTypeName = entityTypeName;
            _ontologyType = ontologyType;
        }

        protected override void Configure(IObjectTypeDescriptor d)
        {
            d.Name(GetName(_operation, _entityTypeName));
            d.Field("type").Type<NonNullType<StringType>>()
                .ResolverNotImplemented();
            d.Field("details").Type(new NonNullType(_ontologyType))
                .ResolverNotImplemented();
        }
    }

    // ----- Mutator CUD types ----- //

    // Pseudo-union of all child types. Only one field should be present.
    public class InputEntityUnionType : InputObjectType
    {
        public static string GetName(Operation operation, EntityType type) => $"UnionInput_{operation.Short()}_{type.Name}";
        
        private readonly Operation _operation;
        private EntityType _type;
        private GraphQlTypeCreator _typeCreator;

        public InputEntityUnionType(Operation operation, EntityType type, GraphQlTypeCreator typeCreator)
        {
            _operation = operation;
            _type = type;
            _typeCreator = typeCreator;
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name(GetName(_operation, _type));
            d.Description("Unites multiple input types. Specify only single field.");
            if (_type.IsAbstract)
                foreach (var child in _typeCreator.GetChildTypes(_type))
                {
                    var type = _typeCreator.GetMutatorInputType(_operation, child);
                    d.Field(child.Name).Type(type);
                }
            else
            {
                var type = _typeCreator.GetMutatorInputType(_operation, _type);
                d.Field(_type.Name).Type(type);
            }
        }
    }

    // Specify relation to existing object or create new with target field
    public class EntityRelationToInputTypeBase : InputObjectType
    {
        public static string GetName(Operation operation, Type type) => $"RelationTo_{operation.Short()}_{type.Name}";
        
        private readonly Operation _operation;
        private InputEntityUnionType _target;
        private Type _type;

        public EntityRelationToInputTypeBase(Operation operation, Type type, InputEntityUnionType target)
        {
            _operation = operation;
            _target = target;
            _type = type;
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name(GetName(_operation, _type));
            d.Description("Specify relation to existing object or create new with target field.");
            d.Field("startsAt").Type<DateTimeType>();
            d.Field("endsAt").Type<DateTimeType>();
            d.Field("targetId").Type<IdType>();
            d.Field("target").Type(_target);
        }
    }
    
    // ----- Create ----- //
    
    public class MultipleInputType : InputObjectType
    {
        public static string GetName(Operation operation, string scalarName) => $"MultipleInput_{operation.Short()}_{scalarName}";
        
        private readonly string _scalarName;
        private readonly IInputType _inputType;
        private readonly Operation _operation;

        public MultipleInputType(Operation operation, string scalarName, IInputType inputType)
        {
            _scalarName = scalarName;
            _inputType = inputType;
            _operation = operation;
        }
        
        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name(GetName(_operation, _scalarName));
            d.Field("value").Type(_inputType);
            if (_operation == Operation.Update)
                d.Field("id").Type<IdType>();
        }
    }

    // ----- Update ----- //
    
    public class RelationPatchType : InputObjectType
    {
        public static string GetName(EmbeddingRelationType relationType)
        {
            if (relationType.IsAttributeType)
                return relationType.AttributeType.ScalarTypeEnum.ToString();
            if (relationType.IsEntityType)
                return relationType.AcceptsOperation(EntityOperation.Update)
                    ? relationType.EntityType.Name
                    : nameof(EntityRelationInput);
            throw new ArgumentException(nameof(relationType));
        }
        
        private readonly string _typeName;
        private IType _createType;
        private IType _updateType;

        public RelationPatchType(EmbeddingRelationType relationType, GraphQlTypeCreator typeCreator)
        {
            _typeName = GetName(relationType);
            if (relationType.IsAttributeType)
            {
                _createType = typeCreator.GetMultipleInputType(Operation.Create, relationType.AttributeType);
                _updateType = typeCreator.GetMultipleInputType(Operation.Update, relationType.AttributeType);
            }
            else if (relationType.IsEntityType)
            {
                if (relationType.AcceptsOperation(EntityOperation.Update))
                {
                    _createType = typeCreator.GetEntityRelationToInputTypeBase(Operation.Create, relationType.EntityType);
                    _updateType = typeCreator.GetEntityRelationToInputTypeBase(Operation.Update, relationType.EntityType);
                }
                else
                {
                    _createType = null; // typeCreator.GetType<EntityRelationInputType>();
                    _updateType = null; // typeCreator.GetType<UpdateEntityRelationInputType>();
                }
            }
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"PatchInput_{_typeName}");
            d.Field("delete").Type<ListType<NonNullType<IdType>>>();
            if (_createType == null)
                d.Field("create").Type<ListType<NonNullType<EntityRelationInputType>>>();
            else
                d.Field("create").Type(new ListType(new NonNullType(_createType)));
            if (_updateType == null)
                d.Field("update").Type<ListType<NonNullType<UpdateEntityRelationInputType>>>();
            else
                d.Field("update").Type(new ListType(new NonNullType(_updateType)));
        }
    }
}