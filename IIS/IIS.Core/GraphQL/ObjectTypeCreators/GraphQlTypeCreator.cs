using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using Microsoft.EntityFrameworkCore.Design;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class GraphQlTypeCreator
    {
        private IOntologyProvider _ontologyProvider;
        private IGraphQlTypeRepository _typeRepository;
        private List<Type> _ontologyTypes;
        
        private ReadQueryTypeCreator _readCreator;
        private CreateMutatorTypeCreator _createCreator;
        private DeleteMutatorTypeCreator _deleteCreator;
        private UpdateMutatorTypeCreator _updateCreator;
        private MutatorTypeCreator[] _mutatorCreators;

        public GraphQlTypeCreator(IGraphQlTypeRepository typeRepository, IOntologyProvider ontologyProvider)
        {
            _typeRepository = typeRepository;
            _ontologyProvider = ontologyProvider;
            _ontologyTypes = ontologyProvider.GetTypes().ToList();
            _readCreator = new ReadQueryTypeCreator(_typeRepository);
            _createCreator = new CreateMutatorTypeCreator(this);
            _deleteCreator = new DeleteMutatorTypeCreator(this);
            _updateCreator = new UpdateMutatorTypeCreator(this);
            _mutatorCreators = new MutatorTypeCreator[] {_createCreator, _deleteCreator, _updateCreator};
        }

        public void Create()
        {
            var entityTypes = _ontologyProvider.GetTypes().OfType<EntityType>().ToList();
            // Create output types
            foreach (var type in entityTypes)
                _readCreator.CreateOutputType(type);
            // Create input types
            foreach (var type in entityTypes)
            {
                foreach (var mutator in _mutatorCreators)
                {
                    GetMutatorInputType(mutator, type);
                    GetMutatorResponseType(mutator, type);
                }

                GetCreateEntityRelationToTargetInputType(type);
            }

            var attributeTypes = _ontologyProvider.GetTypes().OfType<AttributeType>().ToList();
            foreach (var attr in attributeTypes)
            {
                GetCreateMultipleInputType(attr);
            }
        }
        
        public IEnumerable<Type> GetChildTypes(Type parent) =>
            _ontologyTypes.Where(t => t.Nodes.OfType<InheritanceRelationType>().Any(r => r.ParentType.Name == parent.Name));
        
        
        // ----- READ QUERY TYPES ----- //
        
        public IOutputType GetOntologyType(Type type)
        {
            if (_typeRepository.OntologyTypes.ContainsKey(type.Name))
                return _typeRepository.OntologyTypes[type.Name];
            var typeGraph = _ontologyTypes.OfType<EntityType>().Single(t => t.Name == type.Name);
            return _readCreator.CreateOutputType(typeGraph);
        }
        
        // ----- GENERIC SCHEMA TYPES ----- //
        
        public T GetType<T>() where T : IType, new() => _typeRepository.GetType<T>();

        public IInputType GetInputAttributeType(AttributeType attributeType)
        {
            IInputType type;
            if (attributeType.ScalarTypeEnum == Core.Ontology.ScalarType.File)
                type = _typeRepository.GetType<InputObjectType<FileValueInput>>();
            else
                type = _typeRepository.GetScalarType(attributeType);
            return type;
        }
        
        // ----- GENERIC MUTATOR TYPES ----- //
        
        public MutatorInputType GetMutatorInputType(MutatorTypeCreator creator, Type type)
        {
            var name = MutatorInputType.GetName(creator.Operation, type.Name);
            return _typeRepository.GetOrCreate(name, () => creator.NewMutatorInputType(type));
        }

        public MutatorResponseType GetMutatorResponseType(MutatorTypeCreator creator, Type type)
        {
            var name = MutatorResponseType.GetName(creator.Operation, type.Name);
            return _typeRepository.GetOrCreate(name, () => creator.NewMutatorResponseType(type));
        }
        
        // ----- CREATE TYPES ----- //

        public CreateMultipleInputType GetCreateMultipleInputType(AttributeType type)
        {
            var name = type.ScalarTypeEnum.ToString();
            return _typeRepository.GetOrCreate(name, () =>
                _createCreator.NewCreateMultipleInputType(type));
        }

        public CreateEntityRelationToInputType GetCreateEntityRelationToInputType(EntityType type)
        {
            var name = type.Name;
            return _typeRepository.GetOrCreate(name, () =>
                _createCreator.NewCreateEntityRelationToInputType(type));
        }

        public CreateEntityRelationToTargetInputType GetCreateEntityRelationToTargetInputType(EntityType type)
        {
            return _typeRepository.GetOrCreate(type.Name, () =>
                _createCreator.NewCreateEntityRelationToTargetInputType(type));
        }
        
        // ----- UPDATE TYPES ----- //
        
        public UpdateMultipleInputType GetUpdateMultipleInputType(AttributeType type)
        {
            var name = type.ScalarTypeEnum.ToString();
            return _typeRepository.GetOrCreate(name, () =>
                _updateCreator.NewUpdateMultipleInputType(type));
        }

        public UpdateEntityRelationToInputType GetUpdateEntityRelationToInputType(EntityType type)
        {
            var name = type.Name;
            return _typeRepository.GetOrCreate(name, () =>
                _updateCreator.NewUpdateEntityRelationToInputType(type));
        }

        public UpdateEntityRelationToTargetInputType GetUpdateEntityRelationToTargetInputType(EntityType type)
        {
            var name = type.Name;
            return _typeRepository.GetOrCreate(name, () =>
                _updateCreator.NewUpdateEntityRelationToTargetInputType(type));
        }

        public EntityRelationPatchType GetEntityRelationPatchType(EntityType type)
        {
            var name = type.Name;
            return _typeRepository.GetOrCreate(name, () =>
                _updateCreator.NewEntityRelationPatchType(type));
        }
        
        public AttributeRelationPatchType GetAttributeRelationPatchType(AttributeType type)
        {
            var name = type.Name;
            return _typeRepository.GetOrCreate(name, () =>
                _updateCreator.NewAttributeRelationPatchType(type));
        }
    }
}