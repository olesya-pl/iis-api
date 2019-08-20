using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class GraphQlTypeCreator
    {
        private IOntologyRepository _ontologyRepository;
        private IGraphQlTypeRepository _typeRepository;

        private ReadQueryTypeCreator _readCreator;
        private CreateMutatorTypeCreator _createCreator;
        private DeleteMutatorTypeCreator _deleteCreator;
        private UpdateMutatorTypeCreator _updateCreator;

        public GraphQlTypeCreator(IGraphQlTypeRepository typeRepository, IOntologyRepository ontologyRepository)
        {
            _typeRepository = typeRepository;
            _ontologyRepository = ontologyRepository;
            _readCreator = new ReadQueryTypeCreator(_typeRepository, this);
            _createCreator = new CreateMutatorTypeCreator(this);
            _deleteCreator = new DeleteMutatorTypeCreator(this);
            _updateCreator = new UpdateMutatorTypeCreator(this);
        }

        public void Create()
        {
            var entityTypes = _ontologyRepository.EntityTypes;
            // Create output types
            foreach (Core.Ontology.ScalarType scalar in Enum.GetValues(typeof(Core.Ontology.ScalarType)))
                GetMultipleOutputType(scalar);
            foreach (var type in entityTypes)
                GetOntologyType(type);
            // Create input types
            var mutations = new[] {Operation.Create, Operation.Delete, Operation.Update};
            foreach (var type in entityTypes.Where(t => !t.IsAbstract))
            {
                foreach (var mutator in mutations)
                {
                    GetMutatorInputType(mutator, type);
                    GetMutatorResponseType(mutator, type);
                }
            }
        }

        public IEnumerable<Type> GetChildTypes(Type parent) => _ontologyRepository.GetChildTypes(parent);

        private MutatorTypeCreator GetMutator(Operation operation)
        {
            switch (operation)
            {
                case Operation.Create: return _createCreator;
                case Operation.Update: return _updateCreator;
                case Operation.Delete: return _deleteCreator;
                default: throw new ArgumentException(nameof(operation));
            }
        }

        // ----- READ QUERY TYPES ----- //

        public IOntologyType GetOntologyType(EntityType type)
        {
            return _typeRepository.GetOrCreate(type.Name, () => _readCreator.NewOntologyType(type));
        }

        public IOutputType GetScalarOutputType(Core.Ontology.ScalarType scalarType)
        {
            IOutputType type;
            if (scalarType == Core.Ontology.ScalarType.File)
                type = _typeRepository.GetType<AttachmentType>();
            else
                type = _typeRepository.Scalars[scalarType];
            return type;
        }

        public IOutputType GetMultipleOutputType(Core.Ontology.ScalarType scalarType)
        {
            var name = scalarType.ToString();
            return _typeRepository.GetOrCreate(name, () =>
                new MultipleOutputType(name, GetScalarOutputType(scalarType)));
        }

        public OutputUnionType GetOutputUnionType(EntityType source, string propertyName, IEnumerable<ObjectType> outputTypes)
        {
            var name = OutputUnionType.GetName(source, propertyName);
            var union = _typeRepository.GetOrCreate(name, () =>
                new OutputUnionType(source, propertyName, outputTypes));
            return union;
        }

        // ----- GENERIC SCHEMA TYPES ----- //

        public T GetType<T>() where T : IType, new() => _typeRepository.GetType<T>();

        public IInputType GetInputAttributeType(AttributeType attributeType)
        {
            IInputType type;
            if (attributeType.ScalarTypeEnum == Core.Ontology.ScalarType.File)
                type = _typeRepository.GetType<FileValueInputType>();
            else
                type = _typeRepository.Scalars[attributeType.ScalarTypeEnum];
            return type;
        }

        // ----- GENERIC MUTATOR TYPES ----- //

        public MutatorInputType GetMutatorInputType(Operation operation, Type type)
        {
            var name = MutatorInputType.GetName(operation, type.Name);
            return _typeRepository.GetOrCreate(name, () => GetMutator(operation).NewMutatorInputType(type));
        }

        public MutatorResponseType GetMutatorResponseType(Operation operation, EntityType type)
        {
            var name = MutatorResponseType.GetName(operation, type);
            return _typeRepository.GetOrCreate(name, () =>
                new MutatorResponseType(GetMutator(operation).Operation, type, GetOntologyType(type)));
        }

        public EntityRelationToInputType GetEntityRelationToInputType(Operation operation, EntityType type)
        {
            var name = EntityRelationToInputType.GetName(operation, type);
            return _typeRepository.GetOrCreate(name, () =>
                new EntityRelationToInputType(operation, type, GetEntityUnionInputType(operation, type)));
        }

        public EntityUnionInputType GetEntityUnionInputType(Operation operation, EntityType type)
        {
            var name = EntityUnionInputType.GetName(operation, type);
            return _typeRepository.GetOrCreate(name, () =>
                new EntityUnionInputType(operation, type, this));
        }

        public MultipleInputType GetMultipleInputType(Operation operation, AttributeType type)
        {
            var scalarName = type.ScalarTypeEnum.ToString();
            var name = MultipleInputType.GetName(operation, scalarName);
            return _typeRepository.GetOrCreate(name, () =>
                new MultipleInputType(operation, scalarName, GetInputAttributeType(type)));
        }

        // ----- UPDATE TYPES ----- //

        public RelationPatchType GetRelationPatchType(EmbeddingRelationType relationType)
        {
            var name = RelationPatchType.GetName(relationType);
            return _typeRepository.GetOrCreate(name, () =>
                new RelationPatchType(relationType, this));
        }
    }
}
