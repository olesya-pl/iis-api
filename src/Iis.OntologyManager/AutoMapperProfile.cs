using AutoMapper;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;

namespace Iis.OntologyManager
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<INodeType, SchemaNodeType>();
            CreateMap<IRelationType, SchemaRelationType>();
            CreateMap<IAttributeType, SchemaAttributeType>();
        }
    }
}
