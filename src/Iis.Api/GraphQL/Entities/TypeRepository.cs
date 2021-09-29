using System;
using System.Collections.Generic;
using System.Linq;
using IIS.Core.GraphQL.Entities.InputTypes.Mutations;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes.Mutations;
using Iis.Domain;
using OScalarType = Iis.Interfaces.Ontology.Schema.ScalarType;
using HotChocolate.Types;
using Iis.OntologySchema.DataTypes;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.Entities
{
    public class OntologyTypesException : Exception
    {
        public OntologyTypesException(string message) : base(message)
        {
        }
    }

    public class TypeRepository : TypeStorage
    {
        private readonly IOntologySchema _ontologySchema;
        private readonly OntologyQueryTypesCreator _creator;
        private readonly OntologyMutationTypesCreator[] _mutators;

        public TypeRepository(IOntologySchema ontologySchema)
        {
            _ontologySchema = ontologySchema;
            _creator = new OntologyQueryTypesCreator(this);
            _mutators = new OntologyMutationTypesCreator[]
            {
                new CreateOntologyMutationTypesCreator(this),
                new DeleteOntologyMutationTypesCreator(this),
                new UpdateOntologyMutationTypesCreator(this)
            };
        }

        public void InitializeTypes()
        {
            var entityTypes = _ontologySchema.GetEntityTypes().ToList();
            if (entityTypes.Count == 0)
                throw new OntologyTypesException("There are no entity types to register on graphql schema");

            // Create output types
            foreach (var type in entityTypes)
                GetOntologyType(type);

            // Create input types
            var mutations = _mutators.Select(m => m.Operation).ToArray();
            foreach (var type in entityTypes.Where(t => !t.IsAbstract))
            foreach (var mutator in mutations)
            {
                GetMutatorInputType(mutator, type);
                GetMutatorResponseType(mutator, type);
            }
        }

        private OntologyMutationTypesCreator GetMutator(Operation operation)
        {
            return _mutators.Single(m => m.Operation == operation);
        }

        // ----- READ QUERY TYPES ----- //

        public IOntologyType GetOntologyType(INodeTypeLinked type)
        {
            return GetOrCreate(type.Name, () => _creator.NewOntologyType(type));
        }

        public IOutputType GetScalarOutputType(OScalarType scalarType)
        {
            IOutputType type;
            if (scalarType == OScalarType.File)
                type = GetType<AttachmentType>();
            else
                type = Scalars[scalarType];
            return type;
        }

        public IOutputType GetMultipleOutputType(OScalarType scalarType)
        {
            var name = scalarType.ToString();
            return GetOrCreate(name, () =>
                new MultipleOutputType(name, GetScalarOutputType(scalarType)));
        }

        public OutputUnionType GetOutputUnionType(INodeTypeLinked source, string propertyName,
            IEnumerable<ObjectType> outputTypes)
        {
            var name = OutputUnionType.GetName(source, propertyName);
            var union = GetOrCreate(name, () =>
                new OutputUnionType(source, propertyName, outputTypes));
            return union;
        }

        // ----- GENERIC SCHEMA TYPES ----- //

        public IInputType GetInputAttributeType(INodeTypeLinked attributeType)
        {
            IInputType type;
            if (attributeType.ScalarTypeEnum == OScalarType.File)
                type = GetType<FileValueInputType>();
            else
                type = Scalars[attributeType.ScalarTypeEnum];
            return type;
        }

        // ----- GENERIC MUTATOR TYPES ----- //

        public MutatorInputType GetMutatorInputType(Operation operation, INodeTypeLinked type)
        {
            var name = MutatorInputType.GetName(operation, type.Name);
            return GetOrCreate(name, () => GetMutator(operation).NewMutatorInputType(type));
        }

        public MutatorResponseType GetMutatorResponseType(Operation operation, INodeTypeLinked type)
        {
            var name = MutatorResponseType.GetName(operation, type);
            return GetOrCreate(name, () =>
                new MutatorResponseType(GetMutator(operation).Operation, type, GetOntologyType(type)));
        }

        public EntityRelationToInputType GetEntityRelationToInputType(Operation operation, INodeTypeLinked type)
        {
            var name = EntityRelationToInputType.GetName(operation, type);
            return GetOrCreate(name, () =>
                new EntityRelationToInputType(operation, type, GetEntityUnionInputType(operation, type)));
        }

        public EntityUnionInputType GetEntityUnionInputType(Operation operation, INodeTypeLinked type)
        {
            var name = EntityUnionInputType.GetName(operation, type);
            return GetOrCreate(name, () =>
                new EntityUnionInputType(operation, type, this));
        }

        public MultipleInputType GetMultipleInputType(Operation operation, INodeTypeLinked type)
        {
            var scalarName = type.ScalarTypeEnum.ToString();
            var name = MultipleInputType.GetName(operation, scalarName);
            return GetOrCreate(name, () =>
                new MultipleInputType(operation, scalarName, GetInputAttributeType(type)));
        }

        // ----- UPDATE TYPES ----- //

        public RelationPatchType GetRelationPatchType(INodeTypeLinked relationType)
        {
            var name = RelationPatchType.GetName(relationType);
            return GetOrCreate(name, () =>
                new RelationPatchType(relationType, this));
        }

        public SingularRelationPatchType GetSingularRelationPatchType(INodeTypeLinked relationType)
        {
            var name = RelationPatchType.GetName(relationType);
            return GetOrCreate(name, () =>
                new SingularRelationPatchType(relationType, this));
        }
    }
}
