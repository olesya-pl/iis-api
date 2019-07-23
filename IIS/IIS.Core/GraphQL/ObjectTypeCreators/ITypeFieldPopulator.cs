using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public interface ITypeFieldPopulator
    {
        void AddFields(IObjectTypeDescriptor descriptor, Type type);
    }
}