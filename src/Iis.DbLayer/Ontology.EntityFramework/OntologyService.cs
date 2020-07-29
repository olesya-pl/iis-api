using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using IIS.Domain;
using IIS.Repository;
using IIS.Repository.Factories;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Attribute = Iis.Domain.Attribute;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class OntologyService<TUnitOfWork> : BaseService<TUnitOfWork>, IOntologyService where TUnitOfWork : IIISUnitOfWork
    {
        //private readonly OntologyContext _context;
        private readonly IOntologyModel _ontology;
        private readonly IElasticService _elasticService;

        public OntologyService(OntologyContext context, IOntologyModel ontology,
            IElasticService elasticService,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
            //_context = context;
            _elasticService = elasticService;
            _ontology = ontology;
        }

        public async Task SaveNodeAsync(Node source, CancellationToken cancellationToken = default)
        {
            var nodeEntity = await RunAsync((unitOfWork) => unitOfWork.OntologyRepository.UpdateNodeAsync(source.Id, 
                n => SaveRelations(source, n)));
            
            if (nodeEntity != null) return;

            nodeEntity = new NodeEntity
            {
                Id = source.Id,
                NodeTypeId = source.Type.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt
            };
            SaveRelations(source, nodeEntity);
            Run((unitOfWork) => unitOfWork.OntologyRepository.AddNode(nodeEntity));
            await _elasticService.PutNodeAsync(source.Id);
        }

        private void SaveRelations(Node source, NodeEntity existing)
        {
            foreach (IEmbeddingRelationTypeModel relationType in source.Type.AllProperties)
            {
                if (relationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                {
                    Relation sourceRelation = source.Nodes.OfType<Relation>().SingleOrDefault(e => e.Type.Id == relationType.Id);
                    RelationEntity existingRelation = existing.OutgoingRelations
                        .SingleOrDefault(e => e.Node.NodeTypeId == relationType.Id && !e.Node.IsArchived);
                    ApplyChanges(existing, sourceRelation, existingRelation);
                }
                else
                {
                    IEnumerable<Relation> sourceRelations = source.Nodes.OfType<Relation>().Where(e => e.Type.Id == relationType.Id);
                    IEnumerable<RelationEntity> existingRelations = existing.OutgoingRelations.Where(e => e.Node.NodeTypeId == relationType.Id);
                    var pairs = sourceRelations.FullOuterJoin(existingRelations, e => e.Id, e => e.Id);
                    foreach (var pair in pairs)
                    {
                        ApplyChanges(existing, pair.Left, pair.Right);
                    }
                }
            }
        }

        void ApplyChanges(NodeEntity existing, Relation sourceRelation, RelationEntity existingRelation)
        {
            if (sourceRelation == null && existingRelation == null)
                return;

            // Set null value
            if (sourceRelation == null && existingRelation != null)
            {
                Archive(existingRelation.Node);
                //Archive(existingRelation.TargetNode);
            }
            // New relation
            else if (sourceRelation != null && existingRelation == null)
            {
                var relation = MapRelation(sourceRelation);
                relation.TargetNodeId = sourceRelation.Target.Id;
                existing.OutgoingRelations.Add(relation);
            }
            // Change target
            else
            {
                Guid existingId = existingRelation.TargetNode.Id;
                Guid targetId = sourceRelation.Target.Id;
                if (existingId != targetId)
                {
                    Archive(existingRelation.Node);

                    RelationEntity relation = MapRelation(sourceRelation);
                    relation.Id = Guid.NewGuid();
                    relation.Node.Id = relation.Id;
                    relation.SourceNodeId = existing.Id;
                    // set tracked target
                    if (!(sourceRelation.Target is Attribute))
                    { 
                        relation.TargetNode = null;
                        relation.TargetNodeId = targetId;
                    }
                    existing.OutgoingRelations.Add(relation);

                    sourceRelation.Id = relation.Id;
                    sourceRelation.CreatedAt = DateTime.UtcNow;
                    sourceRelation.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        NodeEntity MapAttribute(Attribute attribute)
        {
            return new NodeEntity
            {
                Id = attribute.Id,
                CreatedAt = attribute.CreatedAt,
                UpdatedAt = attribute.UpdatedAt,
                NodeTypeId = attribute.Type.Id,
                Attribute = new AttributeEntity
                {
                    Id = attribute.Id,
                    Value = AttributeType.ValueToString(attribute.Value, ((IAttributeTypeModel)attribute.Type).ScalarTypeEnum)
                }
            };
        }

        NodeEntity MapEntity(Entity entity)
        {
            return RunWithoutCommit((unitOfWork) => unitOfWork.OntologyRepository.GetNodeEntityById(entity.Id));
        }

        RelationEntity MapRelation(Relation relation)
        {
            var target = relation.Target is Attribute
                ? MapAttribute((Attribute)relation.Target)
                : null;
            return new RelationEntity
            {
                Id = relation.Id,
                Node = new NodeEntity
                {
                    Id = relation.Id,
                    CreatedAt = relation.CreatedAt,
                    UpdatedAt = relation.UpdatedAt,
                    NodeTypeId = relation.Type.Id
                },
                TargetNode = target,
                TargetNodeId = relation.Target.Id
            };
        }

        void Archive(NodeEntity node)
        {
            node.IsArchived = true;
            node.UpdatedAt = DateTime.UtcNow;
        }

        private async Task<IEnumerable<Node>> GetNodesAsync(IQueryable<NodeEntity> relationsQ, ElasticFilter filter, CancellationToken cancellationToken = default)
        {
            NodeEntity[] ctxNodes;
            // workaround because of wrong generated query with distinct and order by
            var workaroundQ = from a in relationsQ.Distinct()
                              select new { Node = a, dummy = string.Empty };

            ctxNodes = await workaroundQ
                .Skip(filter.Offset)
                .Take(filter.Limit)
                .Select(r => r.Node)
                .ToArrayAsync(cancellationToken);
            return ctxNodes.Select(MapNode).ToArray();
        }
        public async Task<(IEnumerable<Node> nodes, int count)> GetNodesAsync(IEnumerable<Guid> matchList, CancellationToken cancellationToken = default)
        {
            var nodeEntities = await RunWithoutCommitAsync((unitOfWork) =>
                unitOfWork.OntologyRepository.GetNodeEntitiesByIdsAsync(matchList));
            var nodes = nodeEntities.Select(MapNode).ToList();
            return (nodes, nodes.Count);
        }
        public async Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<INodeTypeModel> types, ElasticFilter filter, CancellationToken cancellationToken = default)
        {
            var derivedTypes = types.SelectMany(e => _ontology.GetChildTypes(e))
                .Concat(types).Distinct().ToArray();

            var isElasticSearch = _elasticService.UseElastic && !string.IsNullOrEmpty(filter.Suggestion) && _elasticService.TypesAreSupported(derivedTypes.Select(nt => nt.Name));
            if (isElasticSearch)
            {
                var searchResult = await _elasticService.SearchByAllFieldsAsync(derivedTypes.Select(t => t.Name), filter, cancellationToken);
                var nodeEntities = await RunWithoutCommitAsync((unitOfWork) =>
                    unitOfWork.OntologyRepository.GetNodeEntitiesByIdsAsync(searchResult.ids));
                var nodes = nodeEntities.Select(MapNode);
                return nodes;
            }
            else
            {
                var query = string.IsNullOrEmpty(filter.Suggestion)
                    ? await RunWithoutCommitAsync(async unitOfWork =>
                    await unitOfWork.OntologyRepository.GetNodesAsync(derivedTypes.Select(nt => nt.Id), filter)) :
                    await RunWithoutCommitAsync(async unitOfWork =>
                    await unitOfWork.OntologyRepository.GetNodesWithSuggestionAsync(derivedTypes.Select(nt => nt.Id), filter.Suggestion, filter));
                var nodes = query.Select(MapNode);
                return nodes;
            }
        }

        public Task<(IEnumerable<JObject> nodes, int count)> FilterObjectsOfStudyAsync(ElasticFilter filter, CancellationToken cancellationToken = default)
        {
            return FilterNodeAsync("ObjectOfStudy", filter, cancellationToken);
        }

        public async Task<int> GetNodesCountAsync(IEnumerable<INodeTypeModel> types, ElasticFilter filter, CancellationToken cancellationToken = default)
        {

            var derivedTypes = types.SelectMany(e => _ontology.GetChildTypes(e))
                .Concat(types).Distinct().ToArray();

            var isElasticSearch = _elasticService.UseElastic && !string.IsNullOrEmpty(filter.Suggestion) && _elasticService.TypesAreSupported(derivedTypes.Select(nt => nt.Name));
            if (isElasticSearch)
            {
                var searchResult = await _elasticService.SearchByAllFieldsAsync(derivedTypes.Select(t => t.Name), filter);
                return searchResult.count;
            }
            else
            {
                return string.IsNullOrEmpty(filter.Suggestion)
                    ? await RunWithoutCommitAsync(async unitOfWork =>
                        await unitOfWork.OntologyRepository.GetNodesCountAsync(derivedTypes.Select(nt => nt.Id))) :
                    await RunWithoutCommitAsync(async unitOfWork =>
                        await unitOfWork.OntologyRepository.GetNodesCountWithSuggestionAsync(derivedTypes.Select(nt => nt.Id), filter.Suggestion));
                //var count = await query.Distinct().CountAsync();
                //return count;
            }
        }

        private async Task<(IEnumerable<JObject> nodes, int count)> FilterNodeAsync(string typeName, ElasticFilter filter, CancellationToken cancellationToken = default)
        {
            var types = _ontology.EntityTypes.Where(p => p.Name == typeName);
            var derivedTypes = types.SelectMany(e => _ontology.GetChildTypes(e))
                .Concat(types).Distinct().ToArray();

            var isElasticSearch = _elasticService.UseElastic && _elasticService.TypesAreSupported(derivedTypes.Select(nt => nt.Name));
            if (isElasticSearch)
            {
                var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(derivedTypes.Select(t => t.Name), filter);
                return (searchResult.Items.Values.Select(p => p.SearchResult), searchResult.Count);
            }
            else
            {
                return (new List<JObject>(), 0);
            }
        }

        public async Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<IRelationTypeModel> toLoad, CancellationToken cancellationToken = default)
        {
            var ctxSource = RunWithoutCommit(unitOfWork => unitOfWork.OntologyRepository.GetNodeEntityById(nodeId));

            if (ctxSource is null) return null;
            if (!ctxSource.OutgoingRelations.Any())
            {
                var relations = await RunWithoutCommitAsync(async unitOfWork =>
                    await unitOfWork.OntologyRepository.GetSourceRelationByIdAsync(nodeId, cancellationToken));
                ctxSource.OutgoingRelations = relations;
            }

            var node = MapNode(ctxSource);

            return node;
        }

        public async Task<IEnumerable<Node>> LoadNodesAsync(IEnumerable<Guid> nodeIds,
            IEnumerable<IEmbeddingRelationTypeModel> relationTypes, CancellationToken cancellationToken = default)
        {

            var nodes = await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.OntologyRepository.GetNodeEntitiesByIdsAsync(nodeIds));

            if (relationTypes == null)
            {
                var relations = await RunWithoutCommitAsync(async unitOfWork =>
                    await unitOfWork.OntologyRepository.GetDirectRelationsQuery(nodeIds, null));
                FillRelations(nodes, relations);
            }
            else
            {
                var directIds = relationTypes.Where(r => !r.IsInversed).Select(r => r.Id).ToArray();
                var inversedIds = relationTypes.Where(r => r.IsInversed).Select(r => r.DirectRelationType.Id).ToArray();
                var relations = new List<RelationEntity>();
                if (directIds.Length > 0)
                {
                    var result = await RunWithoutCommitAsync(async unitOfWork =>
                        await unitOfWork.OntologyRepository.GetDirectRelationsQuery(nodeIds, directIds));
                    relations.AddRange(result);
                }

                if (inversedIds.Length > 0)
                {
                    var result = await RunWithoutCommitAsync(async unitOfWork =>
                            await unitOfWork.OntologyRepository.GetInversedRelationsQuery(nodeIds, inversedIds));
                    var map = relationTypes.Where(r => r.IsInversed).ToDictionary(r => r.DirectRelationType.Id, r => r.Id);
                    foreach (var rel in result)
                    {
                        var r = new RelationEntity
                        {
                            Id = rel.Id,
                            TargetNodeId = rel.SourceNodeId,
                            TargetNode = rel.SourceNode,
                            SourceNodeId = rel.TargetNodeId,
                            SourceNode = rel.TargetNode
                        };
                        r.Node = new NodeEntity
                        {
                            Id = rel.Id,
                            NodeTypeId = rel.Node.NodeTypeId,
                            Relation = r
                        };
                        relations.Add(r);
                    }
                }
                FillRelations(nodes, relations);
            }

            return nodes.Select(n => MapNode(n)).ToList();
        }



        private void FillRelations(List<NodeEntity> nodes, List<RelationEntity> relations)
        {
            var nodesDict = nodes.ToDictionary(n => n.Id);
            foreach (var node in nodesDict.Values)
                node.OutgoingRelations = new List<RelationEntity>();
            foreach (var relation in relations)
                nodesDict[relation.SourceNodeId].OutgoingRelations.Add(relation);
        }
        private Node MapNode(NodeEntity ctxNode)
        {
            return MapNode(ctxNode, new List<Node>());
        }

        private Node MapNode(NodeEntity ctxNode, List<Node> mappedNodes)
        {
            var m = mappedNodes.SingleOrDefault(e => e.Id == ctxNode.Id);
            if (m != null) return m;

            var type = _ontology.GetType(ctxNode.NodeTypeId)
                       ?? throw new ArgumentException($"Ontology type with id {ctxNode.NodeTypeId} was not found.");
            Node node;
            if (type is IAttributeTypeModel attrType)
            {
                var attribute = ctxNode.Attribute ?? RunWithoutCommit((unitOfWork) =>
                                    unitOfWork.OntologyRepository.GetAttributeEntityById(ctxNode.Id));
                if (attribute == null)
                {
                    throw new Exception($"Attribute does not exists for attribute type node id = {ctxNode.Id}");
                }
                var value = AttributeType.ParseValue(attribute.Value, attrType.ScalarTypeEnum);
                node = new Attribute(ctxNode.Id, attrType, value, ctxNode.CreatedAt, ctxNode.UpdatedAt);
            }
            else if (type is IEntityTypeModel entityType)
            {
                node = new Entity(ctxNode.Id, entityType, ctxNode.CreatedAt, ctxNode.UpdatedAt);
                mappedNodes.Add(node);
            }
            else if (type is IEmbeddingRelationTypeModel relationType)
            {
                node = new Relation(ctxNode.Id, relationType, ctxNode.CreatedAt, ctxNode.UpdatedAt);
                var target = MapNode(ctxNode.Relation.TargetNode, mappedNodes);
                node.AddNode(target);
            }
            else throw new Exception($"Node mapping does not support ontology type {type.GetType()}.");

            foreach (var relatedNode in ctxNode.OutgoingRelations.Where(e => !e.Node.IsArchived))
            {
                var mapped = MapNode(relatedNode.Node, mappedNodes);
                node.AddNode(mapped);
            }

            return node;
        }
        public async Task RemoveNodeAsync(Node node, CancellationToken cancellationToken = default)
        {
            var ctxNode = RunWithoutCommit((unitOfWork) => unitOfWork.OntologyRepository.GetNodeEntityById(node.Id));

            var relations = await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.OntologyRepository.GetAllRelationsAsync(node.Id));
            var updatedNodeEntities = new List<NodeEntity>();
            foreach (var relation in relations)
            {
                Archive(relation.Node);
                updatedNodeEntities.Add(relation.Node);
            }

            Archive(ctxNode);
            updatedNodeEntities.Add(ctxNode);
            Run(unitOfWork => unitOfWork.OntologyRepository.UpdateNodes(updatedNodeEntities));
        }

        public async Task<Node> GetNodeByUniqueValue(Guid nodeTypeId, string value, string valueTypeName)
        {
            var nodeEntities = await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.OntologyRepository.GetNodeByUniqueValue(nodeTypeId, value, valueTypeName));

            return nodeEntities
                .Select(n => (Entity)MapNode(n))
                .FirstOrDefault();
        }

        public Task<IEnumerable<AttributeEntity>> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName, int limit)
        {
            return RunWithoutCommitAsync(async unitOfWork =>
                   await unitOfWork.OntologyRepository.GetNodesByUniqueValue(nodeTypeId, value, valueTypeName, limit));

        }
        //public async Task CreateRelation(Guid sourceNodeId, Guid targetNodeId)
        //{
        //    var relationEntity = new RelationEntity
        //    {
        //        Id = Guid.NewGuid(),
        //        SourceNodeId = sourceNodeId,
        //        TargetNodeId = targetNodeId
        //    };
        //    _context.Relations.Add(relationEntity);
        //    await _context.SaveChangesAsync();
        //}

        public Task<List<Guid>> GetNodeIdListByFeatureIdListAsync(IEnumerable<Guid> featureIdList)
        {
            return RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.OntologyRepository.GetNodeIdListByFeatureIdListAsync(featureIdList));

        }
    }
}
