using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotChocolate.Types;
using Humanizer.Configuration;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology;
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

    public class AttributeRelationPatchType : InputObjectType
    {
        private AttributeType _type;
        private IType _createType;
        private IType _updateType;

        public AttributeRelationPatchType(AttributeType type, GraphQlTypeCreator typeCreator)
        {
            _type = type;
            _createType = typeCreator.GetMultipleInputType(Operation.Create, _type);
            _updateType = typeCreator.GetMultipleInputType(Operation.Update, _type);
        }
        
        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"PatchInput_{_type.ScalarTypeEnum.ToString()}");
            d.Field("delete").Type<ListType<NonNullType<IdType>>>();
            d.Field("create").Type(new ListType(new NonNullType(_createType)));
            d.Field("update").Type(new ListType(new NonNullType(_updateType)));
        }
    }

    public class EntityRelationPatchType : InputObjectType
    {
        private EntityType _type;
        private IType _createType;
        private IType _updateType;

        public EntityRelationPatchType(EntityType type, GraphQlTypeCreator typeCreator)
        {
            _type = type;
            _createType = typeCreator.GetEntityRelationToInputTypeBase(Operation.Create, _type);
            _updateType = typeCreator.GetEntityRelationToInputTypeBase(Operation.Update, _type);
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"PatchInput_{_type.Name}");
            d.Field("delete").Type<ListType<NonNullType<IdType>>>();
            d.Field("create").Type(new ListType(new NonNullType(_createType)));
            d.Field("update").Type(new ListType(new NonNullType(_updateType)));
        }
    }
}