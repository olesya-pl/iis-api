using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyData
{
    public class OntologyDoctor
    {
        private readonly IOntologyNodesData _ontologyData;
        public OntologyDoctor(IOntologyNodesData ontologyData)
        {
            _ontologyData = ontologyData;
        }

        public string Heal()
        {
            var sb = new StringBuilder();

            sb.AppendLine(HealEmptyRequiredProperties());

            return sb.ToString();
        }

        private string HealEmptyRequiredProperties()
        {
            var entities = _ontologyData.Nodes.Where(n => n.NodeType.Kind == Kind.Entity).ToList();
            int cnt = 0;
            _ontologyData.WriteLock(() =>
            {
                foreach (var entity in entities)
                {
                    if (HealEmptyRequiredProperties(entity))
                    {
                        cnt++;
                    }
                }
            });
            return $"{cnt} об'єктів вилікувані";
        }

        private bool HealEmptyRequiredProperties(INode entity)
        {
            bool isHealed = false;

            var requiredProperties = entity.NodeType.GetAllOutgoingRelations()
                .Where(r => r.Kind == RelationKind.Embedding && r.EmbeddingOptions == EmbeddingOptions.Required)
                .Select(r => r.NodeType);

            foreach (var requiredProperty in requiredProperties)
            {
                if (entity.OutgoingRelations.Any(r => r.Node.NodeType.Name == requiredProperty.Name)) continue;

                var targetType = requiredProperty.RelationType.TargetType;
                if (targetType.IsEnum)
                {
                    var enumNode = _ontologyData.GetDefaultEnumNode(targetType.Id);
                    _ontologyData.CreateRelation(entity.Id, enumNode.Id, requiredProperty.Id);
                }
                if (targetType.Kind == Kind.Attribute)
                {
                    _ontologyData.CreateRelationWithAttribute(entity.Id, requiredProperty.Id, targetType.AttributeType.GetDefaultValue());
                }

                isHealed = true;
                }

            return isHealed;
        }
    }
}
