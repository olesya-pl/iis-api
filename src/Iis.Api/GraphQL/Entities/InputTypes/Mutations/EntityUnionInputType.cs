using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.Entities.InputTypes.Mutations
{
    // Pseudo-union of all child types. Only one field should be present.
    public class EntityUnionInputType : InputObjectType
    {
        private readonly Operation _operation;
        private readonly INodeTypeLinked _type;
        private readonly TypeRepository _typeRepository;

        public EntityUnionInputType(Operation operation, INodeTypeLinked type, TypeRepository typeRepository)
        {
            _operation = operation;
            _type = type;
            _typeRepository = typeRepository;
        }

        public static string GetName(Operation operation, INodeTypeLinked type)
        {
            return $"UnionInput_{operation.Short()}_{OntologyObjectType.GetName(type)}";
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name(GetName(_operation, _type));
            d.Description("Unites multiple input types. Specify only single field.");
            if (_type.IsAbstract)
            {
                foreach (var child in _typeRepository.GetChildTypes(_type).OfType<INodeTypeLinked>().Where(t => !t.IsAbstract))
                {
                    var type = _typeRepository.GetMutatorInputType(_operation, child);
                    d.Field(child.Name).Type(type);
                }
            }
            else
            {
                var type = _typeRepository.GetMutatorInputType(_operation, _type);
                d.Field(_type.Name).Type(type);
            }
        }
    }
}
