using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Iis.Ontology.DataRead.Concept
{
    internal sealed class GetEntityConceptQueryHandler : IRequestHandler<GetEntityConceptQuery, EntityConcept>
    {
        private readonly IMapper _mapper;
        private readonly OntologyContext _context;

        public GetEntityConceptQueryHandler(
            IMapper mapper,
            OntologyContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<EntityConcept> Handle(GetEntityConceptQuery request, CancellationToken cancellationToken)
        {
            IQueryable<NodeTypeEntity> query =
                from nt in _context.NodeTypes.AsNoTracking()
                    .Include(x => x.OutgoingRelations)
                        .ThenInclude(x => x.NodeType)
                    .Include(x => x.OutgoingRelations)
                        .ThenInclude(x => x.TargetType)
                            .ThenInclude(x => x.AttributeType)
                where nt.Id == request.EntityId
                select nt;
            NodeTypeEntity nodeTypeEntity = await query.FirstAsync(cancellationToken);

            EntityConcept entityConcept = _mapper.Map<EntityConcept>(nodeTypeEntity);
            entityConcept.ParentEntities = new List<EntityRelation>();
            entityConcept.AttributeRelations = new List<AttributeRelation>();
            entityConcept.EntityRelations = new List<EntityRelation>();

            foreach (RelationTypeEntity relationTypeEntity in nodeTypeEntity.OutgoingRelations)
            {
                if (relationTypeEntity.Kind == RelationKind.Inheritance)
                {
                    EntityRelation relation = GetEntityRelation(relationTypeEntity);
                    entityConcept.ParentEntities.Add(relation);
                }
                else if (relationTypeEntity.Kind == RelationKind.Embedding)
                {
                    if (relationTypeEntity.TargetType.Kind == Kind.Entity)
                    {
                        EntityRelation relation = GetEntityRelation(relationTypeEntity);
                        entityConcept.EntityRelations.Add(relation);
                    }
                    else if (relationTypeEntity.TargetType.Kind == Kind.Attribute)
                    {
                        AttributeRelation relation = GetAttributeRelation(relationTypeEntity);
                        entityConcept.AttributeRelations.Add(relation);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            return entityConcept;
        }

        private EntityRelation GetEntityRelation(RelationTypeEntity relationTypeEntity)
        {
            if (relationTypeEntity.TargetType.Kind != Kind.Entity)
            {
                throw new ArgumentException();
            }

            EntityRelation entityRelation = _mapper.Map<EntityRelation>(relationTypeEntity.NodeType);
            entityRelation.Entity = _mapper.Map<NodeConcept>(relationTypeEntity.TargetType);
            return entityRelation;
        }

        private AttributeRelation GetAttributeRelation(RelationTypeEntity relationTypeEntity)
        {
            if (relationTypeEntity.TargetType.Kind != Kind.Attribute)
            {
                throw new ArgumentException();
            }

            AttributeRelation attributeRelation = _mapper.Map<AttributeRelation>(relationTypeEntity.NodeType);
            attributeRelation.Attribute = _mapper.Map<AttributeConcept>(relationTypeEntity.TargetType);
            return attributeRelation;
        }
    }
}