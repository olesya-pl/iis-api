using AutoMapper;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class MigrationService
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private TypeMappings _typeMappings;
        private OntologySnapshot _snapshotOld;
        private OntologySnapshot _snapshotNew;
        private ILogger<MigrationService> _logger;
        private MigrationValueDevider _devider;
        private Dictionary<Guid, NodeEntity> _migratedNodes = new Dictionary<Guid, NodeEntity>();
        private List<RelationEntity> _migratedRelations = new List<RelationEntity>();
        public MigrationService(OntologyContext context, IMapper mapper, ILogger<MigrationService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _devider = new MigrationValueDevider();
        }
        public async Task MigrateAsync()
        {
            _typeMappings = new TypeMappings();
            _typeMappings.InitFromFile("typemapping.txt");

            _snapshotOld = GetSnapshotFromFiles();
            _snapshotNew = GetSnapshotFromDb(false);
            //_snapshot = GetSnapshotFromDb();
            //Save(_snapshot, "snapshot");

            //MigrateDirectMappedCases();
            var migrationCondition = new MigrationCondition { TypeFrom = "Person.education", TypeTo = "Person.education.additionalInfo" };
            var conditions = new List<MigrationCondition> 
            {
                new MigrationCondition { TypeFrom = "Person.education", TypeTo = "Person.education.additionalInfo" },
                new MigrationCondition { TypeFrom = "Person.dateOfBirth", TypeTo = "Person.dateOfBirth" },
                new MigrationCondition { TypeFrom = "Person.dateOfJobPositionActualization", TypeTo = "Person.dateOfJobPositionActualization" },
                new MigrationCondition { TypeFrom = "Person.dateOfRankActualization", TypeTo = "Person.dateOfRankActualization" },
                new MigrationCondition { TypeFrom = "Person.dateOfServingPlaceActualization", TypeTo = "Person.dateOfServingPlaceActualization" },
                new MigrationCondition { TypeFrom = "Person.firstName", TypeTo = "Person.realName.firstName.ukr" },
                new MigrationCondition { TypeFrom = "Person.secondName", TypeTo = "Person.realName.lastName.ukr" },
                new MigrationCondition { TypeFrom = "Person.fatherName", TypeTo = "Person.realName.fatherName.ukr" },
                new MigrationCondition { TypeFrom = "Person.position", TypeTo = "Person.currentJob.position" },
                new MigrationCondition { TypeFrom = "Person.registrationPlace", TypeTo = "Person.registrationAddress.location" },
                new MigrationCondition { TypeFrom = "Person.livingPlace", TypeTo = "Person.currentLocation.location" },
                new MigrationCondition { TypeFrom = "Person.placeOfBirth", TypeTo = "Person.actualAccommodation.location" },
                
                new MigrationCondition { TypeFrom = "Person.description", TypeTo = "Person.otherInterestingData" },
                //new MigrationCondition { TypeFrom = "Person.relatesToCountry", TypeTo = "Person.citizenship" },

            };
            MigrateDirectMappedCases();
            MigrateByConditions(conditions);
            SaveMigratedToDb();
            _snapshotOld.GetNotMappedNodeStatistics();

            await Task.Yield();
        }

        private void AddMigratedNode(NodeEntity node)
        {
            if (!_migratedNodes.ContainsKey(node.Id))
            {
                _migratedNodes[node.Id] = node;
            }
        }

        private void MigrateDirectMappedCases()
        {
            _logger.LogInformation("Start migrating...");
            var snapshotNodes = _snapshotOld.GetNodesReadyForMigration();
            foreach (var snapshotNode in snapshotNodes)
            {
                var node = _mapper.Map<NodeEntity>(snapshotNode);
                node.NodeTypeId = _typeMappings.GetMapTypeId(node.NodeTypeId);
                node.IncomingRelations = null;
                AddMigratedNode(node);
                snapshotNode.IsMigrated = true;
            }

            //var relations = snapshotNodes.SelectMany(n => n.IncomingRelations).ToList();
            //var badRelations = relations.Where(r => !snapshotNodes.Any(n => n.Id == r.SourceNodeId)).ToList();

            var ignoredTypeIds = new List<Guid> { new Guid("177f4fbd-f2ea-4d7c-836e-738c48aece4c") };

            foreach (var snapshotNode in snapshotNodes)
            {
                var relations = snapshotNode.IncomingRelations.Select(r => _mapper.Map<RelationEntity>(r)).ToList();
                foreach (var relation in relations)
                {
                    var snapshotRelationNode = _snapshotOld.Nodes[relation.Id];
                    if (ignoredTypeIds.Contains(snapshotRelationNode.NodeTypeId)) continue;
                    var relationNode = _mapper.Map<NodeEntity>(snapshotRelationNode);
                    relationNode.NodeTypeId = _typeMappings.GetMapTypeId(relationNode.NodeTypeId);
                    AddMigratedNode(relationNode);
                    
                    _migratedRelations.Add(relation);
                }
            }
        }

        private void MigrateByConditions(List<MigrationCondition> conditions)
        {
            conditions.ForEach(c => MigrateByCondition(c));
        }

        private void MigrateByCondition(MigrationCondition condition)
        {
            var tFrom = _snapshotOld.GetNodeTypesByDotName(condition.TypeFrom);
            var tTo = _snapshotNew.GetNodeTypesByDotName(condition.TypeTo);
            var migratingNodes = _snapshotOld.GetNodesByTypeId(tFrom.Last());

            var shortRelations = tTo.Skip(1).ToList();
            foreach (var migratingNode in migratingNodes)
            {
                MigrateNodeByCondition(migratingNode, tFrom[0].NodeTypeId, shortRelations);
            }
             
        }

        private NodeEntity CreateNodeEntity(Guid nodeTypeId)
        {
            return new NodeEntity
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now, 
                UpdatedAt = DateTime.Now, 
                IsArchived = false,
                NodeTypeId = nodeTypeId
            };
        }

        private void AddRelation(Guid sourceNodeId, Guid targetNodeId, Guid relationTypeId, Guid nodeTypeId)
        {
            var relationNode = CreateNodeEntity(relationTypeId);
            var relation = new RelationEntity
            {
                Id = relationNode.Id,
                SourceNodeId = sourceNodeId,
                TargetNodeId = targetNodeId
            };
            AddMigratedNode(relationNode);
            _migratedRelations.Add(relation);
        }

        private Guid AddMiddleNode(Guid sourceNodeId, Guid relationTypeId, Guid nodeTypeId)
        {
            var node = CreateNodeEntity(nodeTypeId);
            AddRelation(sourceNodeId, node.Id, relationTypeId, nodeTypeId);
            AddMigratedNode(node);
            return node.Id;
        }

        private void MigrateNodeByCondition(SnapshotNode node, Guid parentTypeId, List<ShortRelation> shortRelations)
        {
            var sourceNodeId = node.IncomingRelations.Where(r => _snapshotOld.Nodes[r.SourceNodeId].NodeTypeId == parentTypeId).SingleOrDefault().SourceNodeId;
            foreach (var shortRelation in shortRelations.Take(shortRelations.Count - 1))
            {
                sourceNodeId = AddMiddleNode(sourceNodeId, (Guid)shortRelation.RelationTypeId, shortRelation.NodeTypeId);
            }
            var lastRelation = shortRelations.Last();
            var targetTypeName = _snapshotNew.NodeTypes[lastRelation.NodeTypeId].Name;
            var devidedValues = _devider.DevideValue(node.Attribute.Value, targetTypeName);
            if (devidedValues == null)
            {
                var nodeEntity = _mapper.Map<NodeEntity>(node);
                nodeEntity.NodeTypeId = lastRelation.NodeTypeId;
                nodeEntity.IncomingRelations = null;
                AddMigratedNode(nodeEntity);

                AddRelation(sourceNodeId, node.Id, (Guid)lastRelation.RelationTypeId, lastRelation.NodeTypeId);
            }
            else
            {
                foreach (var devidedValue in devidedValues)
                {
                    var shortRelation = _snapshotNew.GetChildTypeIdByName(lastRelation.NodeTypeId, devidedValue.TypeName);
                    var nodeEntity = CreateNodeEntity(shortRelation.NodeTypeId);
                    nodeEntity.Attribute = new AttributeEntity { Id = nodeEntity.Id, Value = devidedValue.Value };
                    AddMigratedNode(nodeEntity);
                    AddRelation(sourceNodeId, nodeEntity.Id, (Guid)shortRelation.RelationTypeId, shortRelation.NodeTypeId);
                }
            }
            node.IsMigrated = true;
        }

        private OntologySnapshot GetSnapshotFromDb(bool toSave)
        {
             var nodes = _context.Nodes
                .Include(n => n.NodeType)
                .Include(n => n.Attribute)
                .Include(n => n.IncomingRelations)
                .ToList();

            var nodeTypes = _context.NodeTypes
                .Include(n => n.AttributeType)
                .Include(n => n.IncomingRelations)
                .Include(n => n.OutgoingRelations)
                .ToList();

            var snapshotNodes = nodes.Select(n => _mapper.Map<SnapshotNode>(n)).ToList();
            var snapshotNodeTypes = nodeTypes.Select(nt => _mapper.Map<SnapshotNodeType>(nt)).ToList();
            if (toSave)
            {
                Save(snapshotNodes, "nodes");
                Save(snapshotNodeTypes, "nodetypes");
            }

            var snapshot = new OntologySnapshot(snapshotNodeTypes, snapshotNodes, _typeMappings);
            return snapshot;
        }

        private OntologySnapshot GetSnapshotFromFiles()
        {
            var snapshotNodes = Load<List<SnapshotNode>>("nodes");
            var snapshotNodeTypes = Load<List<SnapshotNodeType>>("nodetypes");
            var snapshot = new OntologySnapshot(snapshotNodeTypes, snapshotNodes, _typeMappings);
            return snapshot;
        }

        private void SaveMigratedToDb()
        {
            _context.Nodes.AddRange(_migratedNodes.Values);
            _context.SaveChanges();
            _context.Relations.AddRange(_migratedRelations);
            _context.SaveChanges();
        }

        private void Save(object obj, string name)
        {
            var json = JsonConvert.SerializeObject(obj);
            var fileName = $"mg_{name}.json";
            File.WriteAllText(fileName, json);
        }

        private T Load<T>(string name)
        {
            var fileName = $"mg_{name}.json";
            var json = File.ReadAllText(fileName);
            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }
    }
}
