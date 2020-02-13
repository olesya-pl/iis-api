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
        private MigrationValueDivider _devider;
        private Dictionary<Guid, NodeEntity> _migratedNodes = new Dictionary<Guid, NodeEntity>();
        private List<RelationEntity> _migratedRelations = new List<RelationEntity>();
        private MigrationRules _rules;
        public MigrationService(OntologyContext context, IMapper mapper, ILogger<MigrationService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _devider = new MigrationValueDivider();
        }

        public void SetRules(MigrationRules rules)
        {
            _rules = rules;
            _typeMappings = rules.DirectMappings;
        }
        public async Task<MigrationResult> MigrateAsync()
        {
            try
            {
                _logger.LogInformation("Making snapshot...");
                _snapshotNew = GetSnapshotFromDb(false);
                _logger.LogInformation("Migrating simple cases...");
                MigrateDirectMappedCases();
                if (_rules.MigratePersonNames)
                {
                    _logger.LogInformation("Migrating persons names...");
                    MigratePersonNames();
                }
                MigrateByConditions(_rules.Conditions);
                _logger.LogInformation("Saving to db...");
                SaveMigratedToDb();
                //_snapshotOld.GetNotMappedNodeStatistics();

                return GetResult(true, "");
            }
            catch (Exception ex)
            {
                return GetResult(false, $"{ex.Message}; {ex.InnerException?.Message}");
            }
        }

        private MigrationResult GetResult(bool isSuccess, string log)
        {
            return new MigrationResult
            {
                IsSuccess = isSuccess,
                Log = log,
                StructureBefore = JsonConvert.SerializeObject(_snapshotOld?.NodeTypes?.Values),
                StructureAfter = JsonConvert.SerializeObject(_snapshotOld?.NodeTypes?.Values),
                MigrationRules = JsonConvert.SerializeObject(_rules)
            };
        }

        public void MakeSnapshotOld()
        {
            _snapshotOld = GetSnapshotFromDb(false);
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
            var snapshotNodes = _snapshotOld.GetNodesReadyForMigration();
            foreach (var snapshotNode in snapshotNodes)
            {
                var node = _mapper.Map<NodeEntity>(snapshotNode);
                node.NodeTypeId = _typeMappings.GetMapTypeId(node.NodeTypeId);
                node.IncomingRelations = null;
                AddMigratedNode(node);
                snapshotNode.IsMigrated = true;
            }

            foreach (var snapshotNode in snapshotNodes)
            {
                var relations = snapshotNode.IncomingRelations.Select(r => _mapper.Map<RelationEntity>(r)).ToList();
                foreach (var relation in relations)
                {
                    var snapshotRelationNode = _snapshotOld.Nodes[relation.Id];
                    var relationNode = _mapper.Map<NodeEntity>(snapshotRelationNode);
                    relationNode.NodeTypeId = _typeMappings.GetMapTypeId(relationNode.NodeTypeId);
                    AddMigratedNode(relationNode);
                    
                    _migratedRelations.Add(relation);
                }
            }
        }

        private void MigratePersonNames()
        {
            var persons = _snapshotOld.GetNodesByUniqueTypeName("Person");
            foreach (var person in persons)
            {
                var fullName = _snapshotOld.GetPersonFullNameOldStyle(person.Id, true);
                if (fullName.LastNameRu == null)
                {
                    SavePersonFullNameNewStyle(person.Id, fullName);
                }
                SavePersonFullNameNewStyle(person.Id, fullName);
            }
        }

        private void SavePersonFullNameNewStyle(Guid personNodeId, PersonFullName fullName)
        {
            var realNameId = AddMiddleNode(personNodeId, "Person.realName");
            var firstNameId = AddMiddleNode(realNameId, "Person.realName.firstName");
            AddMiddleNode(firstNameId, "Person.realName.firstName.ukr", fullName.FirstNameUkr);
            AddMiddleNode(firstNameId, "Person.realName.firstName.original", fullName.FirstNameRu);
            var lastNameId = AddMiddleNode(realNameId, "Person.realName.lastName");
            AddMiddleNode(lastNameId, "Person.realName.lastName.ukr", fullName.LastNameUkr);
            AddMiddleNode(lastNameId, "Person.realName.lastName.original", fullName.LastNameRu);
            var fatherNameId = AddMiddleNode(realNameId, "Person.realName.fatherName");
            AddMiddleNode(fatherNameId, "Person.realName.fatherName.ukr", fullName.FatherNameUkr);
            AddMiddleNode(fatherNameId, "Person.realName.fatherName.original", fullName.FatherNameRu);
        }

        private void MigrateByConditions(List<MigrationCondition> conditions)
        {
            conditions.ForEach(c => MigrateByCondition(c));
        }

        private void MigrateByCondition(MigrationCondition condition)
        {
            _logger.LogInformation("Migrating {from} ...", condition.TypeFrom);
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

        private Guid AddMiddleNode(Guid sourceNodeId, string dotTypeName, string value = null)
        {
            var rels = _snapshotNew.GetNodeTypesByDotName(dotTypeName);
            return AddMiddleNode(sourceNodeId, (Guid)rels.Last().RelationTypeId, rels.Last().NodeTypeId, value);
        }

        private Guid AddMiddleNode(Guid sourceNodeId, Guid relationTypeId, Guid nodeTypeId, string value = null)
        {
            var node = CreateNodeEntity(nodeTypeId);
            if (value != null)
            {
                node.Attribute = new AttributeEntity
                {
                    Id = node.Id, 
                    Value = value
                };
            }
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
            var devidedValues = _devider.DivideValue(node.Attribute.Value, targetTypeName);
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
                var middleNodeId = AddMiddleNode(sourceNodeId, (Guid)lastRelation.RelationTypeId, lastRelation.NodeTypeId);
                foreach (var devidedValue in devidedValues)
                {
                    var shortRelation = _snapshotNew.GetChildTypeIdByName(lastRelation.NodeTypeId, devidedValue.TypeName);
                    var nodeEntity = CreateNodeEntity(shortRelation.NodeTypeId);
                    nodeEntity.Attribute = new AttributeEntity { Id = nodeEntity.Id, Value = devidedValue.Value };
                    AddMigratedNode(nodeEntity);
                    AddRelation(middleNodeId, nodeEntity.Id, (Guid)shortRelation.RelationTypeId, shortRelation.NodeTypeId);
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
