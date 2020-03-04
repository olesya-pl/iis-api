using AutoMapper;
using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.Saver
{
    public class OntologySchemaSaver
    {
        OntologyContext _context;
        IMapper _mapper;
        public OntologySchemaSaver(OntologyContext context)
        {
            _context = context;
            _mapper = GetMapper();
        }

        public void SaveToDatabase(ISchemaCompareResult compareResult, IOntologySchema schemaTo)
        {
            AddNodes(compareResult.ItemsToAdd);
            DeleteNodes(compareResult.ItemsToDelete);
            UpdateNodes(compareResult.ItemsToUpdate, schemaTo);
            _context.SaveChanges();
        }

        private void AddNodes(IReadOnlyList<INodeTypeLinked> nodeTypesToAdd)
        {
            foreach (var nodeType in nodeTypesToAdd)
            {
                var nodeTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)nodeType);
                _context.NodeTypes.Add(nodeTypeEntity);

                if (nodeType.Kind == Kind.Relation)
                {
                    if (nodeType.RelationType.TargetType.Kind == Kind.Attribute)
                    {
                        var targetType = nodeType.RelationType.TargetType;
                        var targetTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)targetType);
                        _context.NodeTypes.Add(targetTypeEntity);

                        var attributeTypeEntity = _mapper.Map<AttributeTypeEntity>((IAttributeType)targetType.AttributeType);
                        _context.AttributeTypes.Add(attributeTypeEntity);
                    }
                    var relationType = _mapper.Map<RelationTypeEntity>((IRelationType)nodeType.RelationType);
                    _context.RelationTypes.Add(relationType);
                }
            }
        }

        private void DeleteNodes(IReadOnlyList<INodeTypeLinked> nodeTypesToDelete)
        {
            foreach (var nodeType in nodeTypesToDelete)
            {
                var nodeTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)nodeType);
                nodeTypeEntity.IsArchived = true;
                _context.NodeTypes.Update(nodeTypeEntity);
            }
        }

        private void UpdateNodes(IReadOnlyList<ISchemaCompareDiffItem> itemsToUpdate, ISchemaEntityTypeFinder entityTypeFinder)
        {
            foreach (var item in itemsToUpdate)
            {
                var nodeType = _mapper.Map<NodeTypeEntity>((INodeType)item.NodeTypeFrom);
                nodeType.Id = item.NodeTypeTo.Id;
                _context.NodeTypes.Update(nodeType);

                if (item.NodeTypeFrom.RelationType == null) continue;

                var relationType = _mapper.Map<RelationTypeEntity>((IRelationType)item.NodeTypeFrom.RelationType);
                relationType.Id = item.NodeTypeTo.RelationType.Id;
                relationType.SourceTypeId = item.NodeTypeTo.RelationType.SourceTypeId;

                var fromTargetType = item.NodeTypeFrom.RelationType.TargetType;
                var toTargetType = item.NodeTypeTo.RelationType.TargetType;
                if (fromTargetType.Kind == Kind.Entity && fromTargetType.Name != toTargetType.Name)
                {
                    var newToTargetType = entityTypeFinder.GetEntityTypeByName(fromTargetType.Name);
                    relationType.TargetTypeId = newToTargetType.Id;
                }
                else
                {
                    relationType.TargetTypeId = item.NodeTypeTo.RelationType.TargetTypeId;
                }
                _context.RelationTypes.Update(relationType);

                if (item.NodeTypeFrom.RelationType.Kind == RelationKind.Embedding && item.NodeTypeFrom.RelationType.TargetType.Kind == Kind.Attribute)
                {
                    var attributeNodeType = _mapper.Map<NodeTypeEntity>((INodeType)item.NodeTypeFrom.RelationType.TargetType);
                    attributeNodeType.Id = item.NodeTypeTo.RelationType.TargetType.Id;
                    _context.NodeTypes.Update(attributeNodeType);

                    var attributeType = _mapper.Map<AttributeTypeEntity>((IAttributeType)item.NodeTypeFrom.RelationType.TargetType.AttributeType);
                    attributeType.Id = item.NodeTypeTo.RelationType.TargetType.Id;
                    _context.AttributeTypes.Update(attributeType);
                }
            }
        }

        private IMapper GetMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<INodeType, NodeTypeEntity>();
                cfg.CreateMap<IRelationType, RelationTypeEntity>();
                cfg.CreateMap<IAttributeType, AttributeTypeEntity>();
            });

            return new Mapper(configuration);
        }
    }
}
