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
        IOntologySchema _schemaTo;
        IMapper _mapper;
        List<INodeType> _createdEntityTypes;
        public OntologySchemaSaver(OntologyContext context, IOntologySchema schemaTo)
        {
            _context = context;
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _schemaTo = schemaTo;
            _mapper = GetMapper();
        }

        public void SaveToDatabase(ISchemaCompareResult compareResult, ISchemaSaveParameters parameters = null)
        {
            _createdEntityTypes = new List<INodeType>();
            AddNodes(compareResult.ItemsToAdd.Where(item => parameters?.IsChecked(item) == true));
            DeleteNodes(compareResult.ItemsToDelete.Where(item => parameters?.IsChecked(item) == true));
            UpdateNodes(compareResult.ItemsToUpdate.Where(item => parameters?.IsChecked(item) == true));

            AddAliases(compareResult.AliasesToAdd.Where(item => parameters?.IsChecked(item) == true));
            UpdateAliases(compareResult.AliasesToUpdate.Where(item => parameters?.IsChecked(item) == true));
            DeleteAliases(compareResult.AliasesToDelete.Where(item => parameters?.IsChecked(item) == true));

            _context.SaveChanges();
        }

        private void AddNodeType(NodeTypeEntity nodeTypeEntity)
        {
            _context.NodeTypes.Add(nodeTypeEntity);
            
            if (nodeTypeEntity.Kind == Kind.Entity)
                _createdEntityTypes.Add(nodeTypeEntity);
        }

        private void AddNodes(IEnumerable<INodeTypeLinked> nodeTypesToAdd)
        {
            foreach (var nodeType in nodeTypesToAdd)
            {
                var nodeTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)nodeType);
                AddNodeType(nodeTypeEntity);

                if (nodeType.Kind == Kind.Relation)
                {
                    var targetType = nodeType.RelationType.TargetType;
                    if (targetType.Kind == Kind.Attribute)                     
                    {
                        if (!_context.NodeTypes.Local.Any(nt => nt.Id == targetType.Id))
                        {
                            var targetTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)targetType);
                            AddNodeType(targetTypeEntity);
                            AddAttributeType(targetType.AttributeType);
                            
                        }
                    }
                    AddRelationType(nodeType.RelationType);
                }
            }
        }
        private void AddRelationType(IRelationTypeLinked relationType)
        {
            var relationTypeEntity = GetRelationTypeEntity(relationType);

            var existingRelationType = _context.RelationTypes.SingleOrDefault(rt => rt.Id == relationTypeEntity.Id);
            if (existingRelationType == null)
            {
                _context.RelationTypes.Add(relationTypeEntity);
            }
            else
            {
                _context.RelationTypes.Update(relationTypeEntity);
            }
        }
        private RelationTypeEntity GetRelationTypeEntity(IRelationTypeLinked relationType)
        {
            var relationTypeEntity = _mapper.Map<RelationTypeEntity>(relationType);

            relationTypeEntity.SourceTypeId = GetEntityTypeIdByName(relationType.SourceType.Name);

            if (relationType.TargetType.Kind == Kind.Entity)
                relationTypeEntity.TargetTypeId = GetEntityTypeIdByName(relationType.TargetType.Name);

            return relationTypeEntity;
        }
        private Guid GetEntityTypeIdByName(string name)
        {
            var entityType = (INodeType)_schemaTo.GetEntityTypeByName(name) ??
                _createdEntityTypes.FirstOrDefault(nt => nt.Name == name);

            if (entityType == null)
                throw new Exception($"Неможливо знайти тип {name}");

            return entityType.Id;
        }
        private void AddAttributeType(IAttributeType attributeType)
        {
            var attributeTypeEntity = _mapper.Map<AttributeTypeEntity>(attributeType);
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
        private void DeleteNodes(IEnumerable<INodeTypeLinked> nodeTypesToDelete)
        {
            foreach (var nodeType in nodeTypesToDelete)
            {
                var nodeTypeEntity = _mapper.Map<NodeTypeEntity>((INodeType)nodeType);
                nodeTypeEntity.IsArchived = true;
                _context.NodeTypes.Update(nodeTypeEntity);
            }
        }
        private void UpdateNodes(IEnumerable<ISchemaCompareDiffItem> itemsToUpdate)
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
                    var newToTargetType = _schemaTo.GetEntityTypeByName(fromTargetType.Name);
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
