using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iis.Domain.Users;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.SecurityLevels;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Ontology;
using Newtonsoft.Json.Linq;

namespace Iis.Services.Ontology
{
    public class NodeJsonService : INodeJsonService
    {
        private ISecurityLevelChecker _securityLevelChecker;

        public NodeJsonService(ISecurityLevelChecker securityLevelChecker)
        {
            _securityLevelChecker = securityLevelChecker;
        }

        public string GetJson(INode node, User user, GetEntityOptions options) =>
            GetJObject(node, user, options).ToString(Newtonsoft.Json.Formatting.None);

        public JObject GetJObject(INode node, User user, GetEntityOptions options)
        {
            var result = new JObject();
            result["id"] = node.Id.ToString("N");
            result["__typename"] = $"Entity{node.NodeType.Name}";

            var relationTypes = node.NodeType.GetAllProperties().OrderBy(_ => _.Name);

            foreach (var relationType in relationTypes)
            {
                var relations = node.GetDirectRelations(relationType.Name);

                if (relations.Count == 0)
                {
                    if (options.NullValues)
                    {
                        result[relationType.Name] = null;
                    }
                    continue;
                }

                if (relationType.IsMultiple)
                {
                    var jArray = new JArray(relations.Select(_ => GetToken(_, user, options)));
                    result[relationType.Name] = jArray;
                }
                else
                {
                    result[relationType.Name] = GetToken(relations.Single(), user, options);
                }
            }

            foreach (var item in node.GetComputedValues().Items)
            {
                result[item.DotName] = item.Value;
            }

            return result;
        }

        private static JObject GetDummy()
        {
            var result = new JObject();
            result["__noaccess"] = true;
            return result;
        }

        private static JObject GetRelationJObject(IRelation relation)
        {
            var result = new JObject();
            result["id"] = relation.TargetNodeId.ToString("N");
            result["__title"] = relation.TargetNode.GetTitleValue();
            result["__typename"] = $"Entity{relation.TargetNode.NodeType.Name}";

            var relationJObj = new JObject();
            relationJObj["id"] = relation.Id.ToString();
            result["_relation"] = relationJObj;

            return result;
        }

        private JToken GetToken(IRelation relation, User user, GetEntityOptions options)
        {
            if (relation.IsLinkToAttribute)
            {
                return relation.TargetNode.Value;
            }

            if (!relation.IsLinkToSeparateObject)
            {
                return GetJObject(relation.TargetNode, user, options);
            }

            if (!_securityLevelChecker.AccessGranted(user.SecurityLevelsIndexes, relation.TargetNode.GetSecurityLevelIndexes()))
            {
                return options.DummyIfNoAccess ? GetDummy() : null;
            }

            return GetRelationJObject(relation);
        }
    }
}
