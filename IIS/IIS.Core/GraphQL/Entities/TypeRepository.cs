using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.InputTypes.Mutations;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes.Mutations;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

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
        private readonly IOntologyProvider _ontologyProvider;
        private readonly OntologyQueryTypesCreator _creator;
        private readonly OntologyMutationTypesCreator[] _mutators;
        private Core.Ontology.Ontology _ontology;

        public TypeRepository(IOntologyProvider ontologyProvider)
        {
            _ontologyProvider = ontologyProvider;
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
            _ontology = _ontologyProvider.GetOntologyAsync().Result; // todo: to async method
            var entityTypes = _ontology.EntityTypes.ToList();
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

        public IEnumerable<Type> GetChildTypes(Type parent)
        {
            return _ontology.GetChildTypes(parent);
        }

        private OntologyMutationTypesCreator GetMutator(Operation operation)
        {
            return _mutators.Single(m => m.Operation == operation);
        }

        // ----- READ QUERY TYPES ----- //

        public IOntologyType GetOntologyType(EntityType type)
        {
            return GetOrCreate(type.Name, () => _creator.NewOntologyType(type));
        }

        public IOutputType GetScalarOutputType(Core.Ontology.ScalarType scalarType)
        {
            IOutputType type;
            if (scalarType == Core.Ontology.ScalarType.File)
                type = GetType<AttachmentType>();
            else
                type = Scalars[scalarType];
            return type;
        }

        public IOutputType GetMultipleOutputType(Core.Ontology.ScalarType scalarType)
        {
            var name = scalarType.ToString();
            return GetOrCreate(name, () =>
                new MultipleOutputType(name, GetScalarOutputType(scalarType)));
        }

        public OutputUnionType GetOutputUnionType(EntityType source, string propertyName,
            IEnumerable<ObjectType> outputTypes)
        {
            var name = OutputUnionType.GetName(source, propertyName);
            var union = GetOrCreate(name, () =>
                new OutputUnionType(source, propertyName, outputTypes));
            return union;
        }

        // ----- GENERIC SCHEMA TYPES ----- //

        public IInputType GetInputAttributeType(AttributeType attributeType)
        {
            IInputType type;
            if (attributeType.ScalarTypeEnum == Core.Ontology.ScalarType.File)
                type = GetType<FileValueInputType>();
            else
                type = Scalars[attributeType.ScalarTypeEnum];
            return type;
        }

        // ----- GENERIC MUTATOR TYPES ----- //

        public MutatorInputType GetMutatorInputType(Operation operation, Type type)
        {
            var name = MutatorInputType.GetName(operation, type.Name);
            return GetOrCreate(name, () => GetMutator(operation).NewMutatorInputType(type));
        }

        public MutatorResponseType GetMutatorResponseType(Operation operation, EntityType type)
        {
            var name = MutatorResponseType.GetName(operation, type);
            return GetOrCreate(name, () =>
                new MutatorResponseType(GetMutator(operation).Operation, type, GetOntologyType(type)));
        }

        public EntityRelationToInputType GetEntityRelationToInputType(Operation operation, EntityType type)
        {
            var name = EntityRelationToInputType.GetName(operation, type);
            return GetOrCreate(name, () =>
                new EntityRelationToInputType(operation, type, GetEntityUnionInputType(operation, type)));
        }

        public EntityUnionInputType GetEntityUnionInputType(Operation operation, EntityType type)
        {
            var name = EntityUnionInputType.GetName(operation, type);
            return GetOrCreate(name, () =>
                new EntityUnionInputType(operation, type, this));
        }

        public MultipleInputType GetMultipleInputType(Operation operation, AttributeType type)
        {
            var scalarName = type.ScalarTypeEnum.ToString();
            var name = MultipleInputType.GetName(operation, scalarName);
            return GetOrCreate(name, () =>
                new MultipleInputType(operation, scalarName, GetInputAttributeType(type)));
        }

        // ----- UPDATE TYPES ----- //

        public RelationPatchType GetRelationPatchType(EmbeddingRelationType relationType)
        {
            var name = RelationPatchType.GetName(relationType);
            return GetOrCreate(name, () =>
                new RelationPatchType(relationType, this));
        }

        public SingularRelationPatchType GetSingularRelationPatchType(EmbeddingRelationType relationType)
        {
            var name = RelationPatchType.GetName(relationType);
            return GetOrCreate(name, () =>
                new SingularRelationPatchType(relationType, this));
        }
    }
}
