using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class CompositeMutatorTypeCreator : ITypeFieldPopulator
    {
        private IGraphQlTypeProvider _graphQlTypeProvider;
        private MutatorTypeCreator[] creators;

        public CompositeMutatorTypeCreator(IGraphQlTypeProvider graphQlTypeProvider)
        {
            _graphQlTypeProvider = graphQlTypeProvider;
            creators = new MutatorTypeCreator[]
            {
                new CreateMutatorTypeCreator(_graphQlTypeProvider),
                new UpdateMutatorTypeCreator(_graphQlTypeProvider),
                new DeleteMutatorTypeCreator(_graphQlTypeProvider),
            };
        }


        public void AddFields(IObjectTypeDescriptor descriptor, Type type) 
        {
            foreach (var creator in creators)
                creator.AddFields(descriptor, type);
        }
    }
}