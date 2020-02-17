using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class OntologySnapshot
    {
        public Dictionary<Guid, SnapshotNodeType> NodeTypes { get; private set; }
        public Dictionary<Guid, SnapshotNode> Nodes { get; private set; }
        private ITypeMappings _typeMappings;

        public OntologySnapshot(List<SnapshotNodeType> nodeTypes, List<SnapshotNode> nodes, ITypeMappings typeMappings)
        {
            NodeTypes = nodeTypes.ToDictionary(nt => nt.Id, nt => nt);
            Nodes = nodes.ToDictionary(n => n.Id, n => n);
            _typeMappings = typeMappings;
        }

        public List<SnapshotNode> GetNodesByTypeId(ShortRelation shortRelation)
        {
            var result = Nodes.Values.
                Where(n => n.NodeTypeId == shortRelation.NodeTypeId &&
                    n.IncomingRelations.Any(r => Nodes[r.Id].NodeTypeId == shortRelation.RelationTypeId)
                ).ToList();
            return result;
        }

        public List<SnapshotNode> GetNodesByTypeId(Guid typeId)
        {
            return Nodes.Values.Where(n => n.NodeTypeId == typeId).ToList();
        }

        public List<SnapshotNode> GetNodesByUniqueTypeName(string typeName)
        {
            var nodeType = GetNodeTypeByUniqueName(typeName);
            return GetNodesByTypeId(nodeType.Id);
        }

        public SnapshotNodeType GetNodeTypeByUniqueName(string typeName)
        {
            return NodeTypes.Values.Where(nt => nt.Name == typeName).SingleOrDefault();
        }

        public bool IsNodeMapped(Guid nodeId)
        {
            var node = Nodes[nodeId];
            return _typeMappings.IsMapped(node.NodeTypeId);
        }

        public bool IsRelationMapped(SnapshotRelation relation)
        {
            return IsNodeMapped(relation.Id) && IsNodeMapped(relation.SourceNodeId) && IsNodeMapped(relation.TargetNodeId);
        }

        public string GetTypeNameByNodeId(Guid nodeId)
        {
            return NodeTypes[Nodes[nodeId].NodeTypeId].Name;
        }

        public (string relationType, string sourceType, string targetType) GetRelationTypes(SnapshotRelation relation)
        {
            return (
                NodeTypes[Nodes[relation.Id].NodeTypeId].Name,
                NodeTypes[Nodes[relation.SourceNodeId].NodeTypeId].Name,
                NodeTypes[Nodes[relation.TargetNodeId].NodeTypeId].Name);
        }

        public bool NodeIsReadyForMigration(SnapshotNode node)
        {
            return IsNodeMapped(node.Id) && (NodeTypes[node.NodeTypeId].Kind == Kind.Entity || node.IncomingRelations.All(r => IsRelationMapped(r)));
        }

        public List<SnapshotNode> GetNodesReadyForMigration()
        {
            return Nodes.Values.Where(n => NodeIsReadyForMigration(n)).ToList();
        }

        public ShortRelation GetChildTypeIdByName(Guid parentTypeId, string childTypeName)
        {
            var parentType = NodeTypes[parentTypeId];
            var relationType = parentType.OutgoingRelations.Where(r => NodeTypes[r.Id].Name == childTypeName).SingleOrDefault();
            if (relationType != null)
            {
                return new ShortRelation { NodeTypeId = relationType.TargetTypeId, RelationTypeId = relationType.Id };
            }

            foreach (var ancestorRelation in parentType.OutgoingRelations.Where(r => r.Kind == RelationKind.Inheritance))
            {
                var result = GetChildTypeIdByName(ancestorRelation.TargetTypeId, childTypeName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private bool IsRelationMatchToTypes(SnapshotRelation relation, Guid sourceTypeId, Guid relationTypeId, Guid targetTypeId)
        {
            return Nodes[relation.SourceNodeId].NodeTypeId == sourceTypeId
                && Nodes[relation.Id].NodeTypeId == relationTypeId
                && Nodes[relation.TargetNodeId].NodeTypeId == targetTypeId;
        }

        private string GetPersonNameByTypeName(Guid personNodeId, string typeName, bool markAsMigrated = false)
        {
            var rels = GetNodeTypesByDotName("Person." + typeName);
            var node = Nodes.Values
                .Where(
                    n => n.IncomingRelations
                        .Any(r => IsRelationMatchToTypes(r, rels[0].NodeTypeId, (Guid)rels[1].RelationTypeId, rels[1].NodeTypeId)
                            && r.SourceNodeId == personNodeId))
                .FirstOrDefault();
            if (node != null && markAsMigrated)
            {
                node.IsMigrated = true;
            }
            return node?.Attribute.Value ?? string.Empty;
        }

        public PersonFullName GetPersonFullNameOldStyle(Guid personNodeId, bool markAsMigrated = false)
        {
            var result = new PersonFullName
            {
                FirstNameUkr = GetPersonNameByTypeName(personNodeId, "firstName", markAsMigrated),
                LastNameUkr = GetPersonNameByTypeName(personNodeId, "secondName", markAsMigrated),
                FatherNameUkr = GetPersonNameByTypeName(personNodeId, "fatherName", markAsMigrated)
            };

            var fullNameRu = GetPersonNameByTypeName(personNodeId, "fullNameRu", markAsMigrated);
            var parts = fullNameRu.Split(' ');
            result.LastNameRu = parts.Length > 0 ? parts[0] : string.Empty;
            result.FirstNameRu = parts.Length > 1 ? parts[1] : string.Empty;
            result.FatherNameRu = parts.Length > 2 ? parts[2] : string.Empty;

            return result;
        }

        public List<ShortRelation> GetNodeTypesByDotName(string typeDotName)
        {
            var parts = typeDotName.Split('.');
            var result = new List<ShortRelation>();
            ShortRelation curRelation = null;

            foreach (var name in parts)
            {
                if (curRelation == null)
                {
                    var nodeType = NodeTypes.Values.Where(nt => nt.Name == name).FirstOrDefault();
                    curRelation = new ShortRelation { NodeTypeId = nodeType.Id, RelationTypeId = null};
                }
                else
                {
                    var nodeType = NodeTypes[curRelation.NodeTypeId];
                    curRelation = GetChildTypeIdByName(nodeType.Id, name);
                }
                
                if (curRelation == null)
                {
                    throw new Exception($"Type is not found for {typeDotName}: {name}");
                }

                result.Add(curRelation);
            }

            return result;
        }

        public List<SnapshotNode> GetNotMappedNodeStatistics()
        {
            var stats = new Dictionary<string, int>();

            void incStat(string key)
            {
                if (stats.ContainsKey(key)) stats[key]++;
                else stats[key] = 1;
            }

            var result = new List<SnapshotNode>();
            foreach (var node in Nodes.Values.Where(n => !n.IsMigrated && NodeTypes[n.NodeTypeId].Kind != Kind.Relation))
            {
                if (NodeTypes[node.NodeTypeId].Kind != Kind.Relation && node.IncomingRelations.Count == 0 && !_typeMappings.IsMapped(node.NodeTypeId))
                {
                    incStat(NodeTypes[node.NodeTypeId].Name);
                }

                foreach (var relation in node.IncomingRelations)
                {
                    if (!IsRelationMapped(relation))
                    {
                        var types = GetRelationTypes(relation);
                        incStat($"{types.sourceType} => {types.relationType} => {types.targetType}");
                    }
                }
            }

            var sb = new StringBuilder();
            foreach (var key in stats.Keys)
            {
                sb.AppendLine($"{key} ({stats[key]})");
            }
            Console.WriteLine(sb.ToString());
            
            return result;
        }

    }
}
