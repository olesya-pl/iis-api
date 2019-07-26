using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class CompositeMutatorTypeCreator : ITypeFieldPopulator
    {
        private GraphQlTypeCreator _graphQlTypeRepository;
        private MutatorTypeCreator[] creators;

        public CompositeMutatorTypeCreator(GraphQlTypeCreator graphQlTypeRepository)
        {
            _graphQlTypeRepository = graphQlTypeRepository;
            creators = new MutatorTypeCreator[]
            {
                new CreateMutatorTypeCreator(_graphQlTypeRepository),
                new UpdateMutatorTypeCreator(_graphQlTypeRepository),
                new DeleteMutatorTypeCreator(_graphQlTypeRepository),
            };
        }


        public void AddFields(IObjectTypeDescriptor descriptor, EntityType type) 
        {
            foreach (var creator in creators)
                creator.AddFields(descriptor, type);
        }

//        public void CreateTypes(EntityType type)
//        {
//            foreach (var creator in creators)
//                creator.CreateTypes(type);
//        }
    }
}