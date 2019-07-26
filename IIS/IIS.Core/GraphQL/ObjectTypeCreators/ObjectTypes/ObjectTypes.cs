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
            d.Name($"CreateMultiple{_scalarName}Input");
            d.Field("value").Type(_inputType);
        }
    }

    // Pseudo-union of all child types. Only one field should be present.
    public class CreateEntityRelationToTargetInputType : InputObjectType
    {
        private readonly EntityType _type;
        private readonly GraphQlTypeCreator _typeCreator;
        private readonly CreateMutatorTypeCreator _mutator;

        public CreateEntityRelationToTargetInputType(EntityType type, GraphQlTypeCreator typeCreator, CreateMutatorTypeCreator mutator)
        {
            _type = type;
            _typeCreator = typeCreator;
            _mutator = mutator;
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"CreateEntityRelationTo{_type.Name}TargetInput");
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

    // Specify relation to existing object or create new with target field
    public class CreateEntityRelationToInputType : InputObjectType
    {
        private readonly CreateEntityRelationToTargetInputType _target;
        private readonly Type _type;

        public CreateEntityRelationToInputType(Type type, CreateEntityRelationToTargetInputType target)
        {
            _target = target;
            _type = type;
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"CreateEntityRelationToInput{_type.Name}Type");
            d.Description("Specify relation to existing object or create new with target field.");
            d.Field("startsAt").Type<DateTimeType>();
            d.Field("endsAt").Type<DateTimeType>();
            d.Field("targetId").Type<IdType>();
            d.Field("target").Type(_target);
        }
    }
}