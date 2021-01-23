using AutoMapper;
using Iis.DataModel;
using Iis.Interfaces.Enums;
using Iis.Interfaces.Ontology.Schema;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _mapper = GetMapper();
        }

        public void SaveToDatabase(ISchemaCompareResult compareResult, IOntologySchema schemaTo, ISchemaSaveParameters parameters = null)
        {
            if (parameters?.Create ?? true) AddNodes(compareResult.ItemsToAdd);
            if (parameters?.Delete ?? true) DeleteNodes(compareResult.ItemsToDelete);
            if (parameters?.Update ?? true) UpdateNodes(compareResult.ItemsToUpdate, schemaTo);
            if (parameters?.Aliases ?? true)
            {
                AddAliases(compareResult.AliasesToAdd);
                UpdateAliases(compareResult.AliasesToUpdate);
                DeleteAliases(compareResult.AliasesToDelete);
            }
            _context.SaveChanges();
        }

        private void AddOrResurrectNodeType(NodeTypeEntity nodeTypeEntity)
        {
            var archivedNodeType = _context.NodeTypes.SingleOrDefault(nt => nt.Id == nodeTypeEntity.Id);
            if (archivedNodeType == null)
            {
                _context.NodeTypes.Add(nodeTypeEntity);
            }
            else
            {
                _context.NodeTypes.Update(nodeTypeEntity);
            }
        }

        private void AddNodes(IReadOnlyList<INodeTypeLinked> nodeTypesToAdd)
        {
            foreach (var nodeType in nodeTypesToAdd)
            {
                var nodeTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)nodeType);
                AddOrResurrectNodeType(nodeTypeEntity);

                if (nodeType.Kind == Kind.Relation)
                {
                    if (nodeType.RelationType.TargetType.Kind == Kind.Attribute)                        
                    {
                        var targetType = nodeType.RelationType.TargetType;
                        if (!_context.NodeTypes.Local.Any(nt => nt.Id == targetType.Id))
                        {
                            var targetTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)targetType);
                            AddOrResurrectNodeType(targetTypeEntity);

                            var attributeTypeEntity = _mapper.Map<AttributeTypeEntity>((IAttributeType)targetType.AttributeType);
                            var existingAttributeType = _context.AttributeTypes.SingleOrDefault(at => at.Id == attributeTypeEntity.Id);
                            if (existingAttributeType == null)
                            {
                                _context.AttributeTypes.Add(attributeTypeEntity);
                            }
                            else
                            {
                                _context.AttributeTypes.Update(attributeTypeEntity);
                            }
                        }
                    }
                    var relationType = _mapper.Map<RelationTypeEntity>((IRelationType)nodeType.RelationType);
                    var existingRelationType = _context.RelationTypes.SingleOrDefault(rt => rt.Id == relationType.Id);
                    if (existingRelationType == null)
                    {
                        _context.RelationTypes.Add(relationType);
                    }
                    else
                    {
                        _context.RelationTypes.Update(relationType);
                    }
                    
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
            var updatedAttributesIds = new List<Guid>();
            foreach (var item in itemsToUpdate)
            {
                var nodeType = _mapper.Map<NodeTypeEntity>((INodeType)item.NodeTypeFrom);
                nodeType.Id = item.NodeTypeTo.Id;
                _context.NodeTypes.Update(nodeType);
                updatedAttributesIds.Add(nodeType.Id);

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
                    if (!updatedAttributesIds.Contains(attributeNodeType.Id))
                    {
                        _context.NodeTypes.Update(attributeNodeType);

                        var attributeType = _mapper.Map<AttributeTypeEntity>((IAttributeType)item.NodeTypeFrom.RelationType.TargetType.AttributeType);
                        attributeType.Id = item.NodeTypeTo.RelationType.TargetType.Id;
                        _context.AttributeTypes.Update(attributeType);
                        
                        updatedAttributesIds.Add(attributeNodeType.Id);
                    }
                }
            }
        }
        private void AddAliases(IEnumerable<IAlias> aliasesToAdd)
        {
            foreach (var alias in aliasesToAdd)
            {
                var aliasEntity = new AliasEntity
                {
                    Id = Guid.NewGuid(),
                    DotName = alias.DotName,
                    Value = alias.Value
                };
                _context.Aliases.Add(aliasEntity);
            }
        }
        private void UpdateAliases(IEnumerable<IAlias> aliasesToUpdate)
        {
            foreach (var alias in aliasesToUpdate)
            {
                var aliasEntity = _context.Aliases.Single(a => a.DotName == alias.DotName && a.Type == AliasType.Ontology);
                aliasEntity.Value = alias.Value;
                _context.Aliases.Update(aliasEntity);
            }
        }
        private void DeleteAliases(IEnumerable<IAlias> aliasesToDelete)
        {
            foreach (var alias in aliasesToDelete)
            {
                var aliasEntity = _context.Aliases.Single(a => a.DotName == alias.DotName && a.Type == AliasType.Ontology);
                _context.Aliases.Remove(aliasEntity);
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
