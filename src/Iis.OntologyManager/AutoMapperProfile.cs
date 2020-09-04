using AutoMapper;
using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;

namespace Iis.OntologyManager
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IAlias, SchemaAlias>();
            CreateMap<INodeBase, NodeEntity>();
            CreateMap<IRelationBase, RelationEntity>();
            CreateMap<IAttributeBase, AttributeEntity>();
        }
    }
}
