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
        IMapper _mapper;
        public OntologySchemaSaver()
        {
            _mapper = GetMapper();
        }

        public void SaveToDatabase(OntologyContext context, ISchemaCompareResult compareResult)
        {
            AddNodes(context, compareResult.ItemsToAdd);
            DeleteNodes(context, compareResult.ItemsToDelete);
            UpdateNodes(context, compareResult.ItemsToUpdate);
            context.SaveChanges();
        }

        private void AddNodes(OntologyContext context, IReadOnlyList<INodeTypeLinked> nodeTypesToAdd)
        {
            foreach (var nodeType in nodeTypesToAdd)
            {
                var nodeTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)nodeType);
                context.NodeTypes.Add(nodeTypeEntity);

                if (nodeType.Kind == Kind.Relation)
                {
                    if (nodeType.RelationType.TargetType.Kind == Kind.Attribute)
                    {
                        var targetType = nodeType.RelationType.TargetType;
                        var targetTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)targetType);
                        context.NodeTypes.Add(targetTypeEntity);

                        var attributeTypeEntity = _mapper.Map<AttributeTypeEntity>((IAttributeType)targetType.AttributeType);
                        context.AttributeTypes.Add(attributeTypeEntity);
                    }
                    var relationType = _mapper.Map<RelationTypeEntity>((IRelationType)nodeType.RelationType);
                    context.RelationTypes.Add(relationType);
                }
            }
        }

        private void DeleteNodes(OntologyContext context, IReadOnlyList<INodeTypeLinked> nodeTypesToDelete)
        {
            foreach (var nodeType in nodeTypesToDelete)
            {
                var nodeTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)nodeType);
                nodeTypeEntity.IsArchived = true;
                context.NodeTypes.Update(nodeTypeEntity);
            }
        }

        private void UpdateNodes(OntologyContext context, IReadOnlyList<ISchemaCompareDiffItem> itemsToUpdate)
        {
            foreach (var item in itemsToUpdate)
            {
                var nodeType = _mapper.Map<NodeTypeEntity>((INodeType)item.NodeTypeFrom);
                nodeType.Id = item.NodeTypeTo.Id;
                context.NodeTypes.Update(nodeType);

                if (item.NodeTypeFrom.RelationType == null) continue;

                var relationType = _mapper.Map<RelationTypeEntity>((IRelationType)item.NodeTypeFrom.RelationType);
                relationType.Id = item.NodeTypeTo.RelationType.Id;
                relationType.SourceTypeId = item.NodeTypeTo.RelationType.SourceTypeId;
                relationType.TargetTypeId = item.NodeTypeTo.RelationType.TargetTypeId;
                context.RelationTypes.Update(relationType);

                if (item.NodeTypeFrom.RelationType.Kind == RelationKind.Embedding && item.NodeTypeFrom.RelationType.TargetType.Kind == Kind.Attribute)
                {
                    var attributeNodeType = _mapper.Map<NodeTypeEntity>((INodeType)item.NodeTypeFrom.RelationType.TargetType);
                    attributeNodeType.Id = item.NodeTypeTo.RelationType.TargetType.Id;
                    context.NodeTypes.Update(attributeNodeType);

                    var attributeType = _mapper.Map<AttributeTypeEntity>((IAttributeType)item.NodeTypeFrom.RelationType.TargetType.AttributeType);
                    attributeType.Id = item.NodeTypeTo.RelationType.TargetType.Id;
                    context.AttributeTypes.Update(attributeType);
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
