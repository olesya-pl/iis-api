using System.Linq;
using HotChocolate.Types;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.GraphQL.Entities.InputTypes
{
    public class CriteriaInputType : InputObjectType
    {
        public static string ANY_OF_CRITERIA_FIELD = "_anyOf";

        private readonly EntityType _type;

        public CriteriaInputType(EntityType type)
        {
            _type = type;
        }

        protected override void Configure(IInputObjectTypeDescriptor descriptor)
        {
            descriptor.Name($"{_type.Name}CriteriaInput");
//            foreach (var property in _type.AllProperties.Where(p => p.IsAttributeType && !p.IsComputed() && !p.IsInversed))
            foreach (var property in _type.AllProperties.Where(p => !p.IsComputed() && !p.IsInversed))
            {
                if (property.IsAttributeType)
                    descriptor.Field(property.Name).Type<StringType>();
                else
                    descriptor.Field(property.Name).Type<IdType>();
            }
            descriptor.Field(ANY_OF_CRITERIA_FIELD).Type<BooleanType>().DefaultValue(false);
        }
    }
}
