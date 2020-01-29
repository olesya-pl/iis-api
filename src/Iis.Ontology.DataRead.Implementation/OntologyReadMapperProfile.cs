using System;
using AutoMapper;
using Iis.DataModel;
using Iis.Ontology.DataRead.Concept;
using Iis.Ontology.DataRead.Raw;

namespace Iis.Ontology.DataRead
{
    internal sealed class OntologyReadMapperProfile : Profile
    {
        public OntologyReadMapperProfile()
        {
            CreateMap<Kind, ItemKind>().ConvertUsing(v => ConvertKind(v));
            CreateMap<NodeTypeEntity, OntologyItem>();

            CreateMap<NodeTypeEntity, NodeConcept>();
            CreateMap<NodeTypeEntity, EntityConcept>().IncludeBase<NodeTypeEntity, NodeConcept>();
            CreateMap<NodeTypeEntity, EntityRelation>().IncludeBase<NodeTypeEntity, NodeConcept>();
            CreateMap<NodeTypeEntity, AttributeRelation>().IncludeBase<NodeTypeEntity, NodeConcept>();
            CreateMap<NodeTypeEntity, AttributeConcept>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertType(src.AttributeType.ScalarType)))
                .IncludeBase<NodeTypeEntity, NodeConcept>();
        }

        private static ItemKind ConvertKind(Kind kind) =>
            kind switch
            {
                Kind.Entity => ItemKind.Entity,
                Kind.Attribute => ItemKind.Attribute,
                Kind.Relation => ItemKind.Relation,
                _ => throw new ArgumentException()
            };

        private static AttributeType ConvertType(ScalarType type) =>
            type switch
            {
                ScalarType.String => AttributeType.String,
                ScalarType.Int => AttributeType.Int,
                ScalarType.Decimal => AttributeType.Decimal,
                ScalarType.Date => AttributeType.Date,
                ScalarType.Boolean => AttributeType.Boolean,
                ScalarType.Geo => AttributeType.Geo,
                ScalarType.File => AttributeType.File,
                ScalarType.Json => AttributeType.Json,
                _ => throw new ArgumentException()
            };
    }
}