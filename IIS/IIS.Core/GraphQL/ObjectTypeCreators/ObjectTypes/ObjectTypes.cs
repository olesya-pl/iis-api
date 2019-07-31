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
            descriptor.Field("_relation").Type<NotImplementedType>();
        }
    }

    public class OntologyInterfaceType : InterfaceType
    {
        public OntologyInterfaceType(Action<IInterfaceTypeDescriptor> configure) : base(configure)
        {
        }
    }
    
    public class OntologyObjectType : ObjectType
    {
        public OntologyObjectType(Action<IObjectTypeDescriptor> configure) : base(configure)
        {
        }
    }
    
    // ----- Mutator types ----- //
    
    public class MutatorInputType : InputObjectType
    {
        public static string GetName(string operation, string entityTypeName) => $"{operation}{entityTypeName}Input";
        public MutatorInputType(Action<IInputObjectTypeDescriptor> configure) : base(configure)
        {
        }
    }

    public class MutatorResponseType : ObjectType
    {
        public static string GetName(string operation, string entityTypeName) => $"{operation}{entityTypeName}Response";

        private readonly string _operation;
        private readonly string _entityTypeName;
        private readonly IOutputType _ontologyType;

        public MutatorResponseType(string operation, string entityTypeName, IOutputType ontologyType)
        {
            _operation = operation;
            _entityTypeName = entityTypeName;
            _ontologyType = ontologyType;
        }

        protected override void Configure(IObjectTypeDescriptor d)
        {
            d.Name($"{_operation}{_entityTypeName}Response");
            d.Field("type").Type<NonNullType<StringType>>()
                .ResolverNotImplemented();
            d.Field("details").Type(new NonNullType(_ontologyType))
                .ResolverNotImplemented();
        }
    }

    // ----- Mutator CUD types ----- //

    public class CreateMultipleInputType : InputObjectType
    {
        private readonly string _scalarName;
        private readonly IInputType _inputType;

        public CreateMultipleInputType(string scalarName, IInputType inputType)
        {
            _scalarName = scalarName;
            _inputType = inputType;
        }
        
        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"MultipleInput_C_{_scalarName}");
            d.Field("value").Type(_inputType);
        }
    }

    // Pseudo-union of all child types. Only one field should be present.
    public class InputEntityUnionType : InputObjectType
    {
        private readonly string _operation;
        private EntityType _type;
        private GraphQlTypeCreator _typeCreator;
        private MutatorTypeCreator _mutator;

        public InputEntityUnionType(string operation, EntityType type, GraphQlTypeCreator typeCreator, MutatorTypeCreator mutator)
        {
            _operation = operation;
            _type = type;
            _typeCreator = typeCreator;
            _mutator = mutator;
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"UnionInput_{_operation[0]}_{_type.Name}");
            d.Description("Unites multiple input types. Specify only single field.");
            if (_type.IsAbstract)
                foreach (var child in _typeCreator.GetChildTypes(_type))
                {
                    var type = _typeCreator.GetMutatorInputType(_mutator, child);
                    d.Field(child.Name).Type(type);
                }
            else
            {
                var type = _typeCreator.GetMutatorInputType(_mutator, _type);
                d.Field(_type.Name).Type(type);
            }
        }
    }

    // GraphQL name = "CreateEntityRelationTo{_type.Name}TargetInput"
    public class CreateEntityRelationToTargetInputType : InputEntityUnionType
    {
        public CreateEntityRelationToTargetInputType(EntityType type, GraphQlTypeCreator typeCreator, CreateMutatorTypeCreator mutator)
            : base("Create", type, typeCreator, mutator)
        {
        }
    }

    // Specify relation to existing object or create new with target field
    public class EntityRelationToInputTypeBase : InputObjectType
    {
        private readonly string _operation;
        private InputEntityUnionType _target;
        private Type _type;

        public EntityRelationToInputTypeBase(string operation, Type type, InputEntityUnionType target)
        {
            _operation = operation;
            _target = target;
            _type = type;
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"RelationTo_{_operation[0]}_{_type.Name}");
            d.Description("Specify relation to existing object or create new with target field.");
            d.Field("startsAt").Type<DateTimeType>();
            d.Field("endsAt").Type<DateTimeType>();
            d.Field("targetId").Type<IdType>();
            d.Field("target").Type(_target);
        }
    }

    public class CreateEntityRelationToInputType : EntityRelationToInputTypeBase
    {
        public CreateEntityRelationToInputType(Type type, CreateEntityRelationToTargetInputType target)
            : base("Create", type, target)
        {
        }
    }
    
    // ----- Update ----- //
    
    public class UpdateMultipleInputType : InputObjectType
    {
        private readonly string _scalarName;
        private readonly IInputType _inputType;

        public UpdateMultipleInputType(string scalarName, IInputType inputType)
        {
            _scalarName = scalarName;
            _inputType = inputType;
        }
        
        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"MultipleInput_U_{_scalarName}");
            d.Field("id").Type<IdType>();
            d.Field("value").Type(_inputType);
        }
    }

    public class AttributeRelationPatchType : InputObjectType
    {
        private AttributeType _type;
        private IType _createType;
        private IType _updateType;

        public AttributeRelationPatchType(AttributeType type, GraphQlTypeCreator typeCreator)
        {
            _type = type;
            _createType = typeCreator.GetCreateMultipleInputType(_type);
            _updateType = typeCreator.GetUpdateMultipleInputType(_type);
        }
        
        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"PatchInput_{_type.ScalarTypeEnum.ToString()}");
            d.Field("delete").Type<ListType<NonNullType<IdType>>>();
            d.Field("create").Type(new ListType(new NonNullType(_createType)));
            d.Field("update").Type(new ListType(new NonNullType(_updateType)));
        }
    }
    
    // GraphQL name = "UpdateEntityRelationTo{_type.Name}TargetInput"
    public class UpdateEntityRelationToTargetInputType : InputEntityUnionType
    {
        public UpdateEntityRelationToTargetInputType(EntityType type, GraphQlTypeCreator typeCreator, UpdateMutatorTypeCreator mutator)
            : base("Update", type, typeCreator, mutator)
        {
        }
    }
    
    public class UpdateEntityRelationToInputType : EntityRelationToInputTypeBase
    {
        public UpdateEntityRelationToInputType(Type type, UpdateEntityRelationToTargetInputType target)
            : base("Update", type, target)
        {
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
            _createType = typeCreator.GetCreateEntityRelationToInputType(_type);
            _updateType = typeCreator.GetUpdateEntityRelationToInputType(_type);
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