using System;
using System.Linq;
using System.Collections.Generic;
using Iis.Domain;
using Iis.DataModel.Materials;
using Iis.Interfaces.Ontology.Data;
namespace Iis.DbLayer.Repositories.Helpers
{
    public static class MaterialDocumentHelper
    {
        private const string TitlePropertyName = "__title";
        private const string ValuePropertyName = "value";
        private const string NoValueFound = "значення відсутне";
        private static readonly Func<INode, string> getTitleFunc = (node) => node.GetComputedValue(TitlePropertyName) ?? NoValueFound;
        private static readonly Func<INode, string> getValueFunc = (node) => node.GetSingleProperty(ValuePropertyName)?.Value ?? NoValueFound;
        public static IDictionary<Guid, (INode Node, MaterialNodeLinkType NodeLinkType, string RelationCreatingType)> MapFeatureCollectionToNodeDictionary(
            IReadOnlyCollection<MaterialFeatureEntity> collection,
            IOntologyNodesData ontologyData)
        {
            var result = new Dictionary<Guid, (INode Node, MaterialNodeLinkType NodeLinkType, string RelationCreatingType)>();

            foreach (var feature in collection)
            {
                var node = ontologyData.GetNode(feature.NodeId);

                if (node is null) continue;

                result.TryAdd(node.Id, (node, feature.NodeLinkType, EntityMaterialRelation.Direct));
            }
            return result;
        }

        public static IDictionary<Guid, (INode Node, MaterialNodeLinkType NodeLinkType, string RelationCreatingType)> GetObjectsLinkedBySign(
            IDictionary<Guid, (INode Node, MaterialNodeLinkType NodeLinkType, string RelationCreatingType)> collection,
            IOntologyNodesData ontologyData)
        {
            var result = new Dictionary<Guid, (INode Node, MaterialNodeLinkType NodeLinkType, string RelaRelationCreatingTypetionCreated)>();

            var signIdCollection = collection
                .Where(e => e.Value.Node.NodeType.IsObjectSign)
                .Select(e => e.Key)
                .ToArray();

            var nodeIdCollection = GetNodeIdCollectionBySignIdCollection(signIdCollection, ontologyData);

            foreach (var nodeId in nodeIdCollection)
            {
                var node = ontologyData.GetNode(nodeId);

                if (node is null) continue;

                result.TryAdd(node.Id, (node, MaterialNodeLinkType.None, EntityMaterialRelation.Feature));
            }

            return result;
        }

        public static IReadOnlyCollection<RelatedObject> MapObjectOfStudyCollection(IDictionary<Guid, (INode Node, MaterialNodeLinkType NodeLinkType, string RelationCreatingType)> collection)
        {
            return MapNodeCollection(collection, (node) => node.NodeType.IsObjectOfStudy, getTitleFunc);
        }

        public static IReadOnlyCollection<RelatedObject> MapSingCollection(IDictionary<Guid, (INode Node, MaterialNodeLinkType NodeLinkType, string RelationCreatingType)> collection)
        {
            return MapNodeCollection(collection, (node) => node.NodeType.IsObjectSign, getValueFunc);
        }

        public static IReadOnlyCollection<RelatedObject> MapEventCollection(IDictionary<Guid, (INode Node, MaterialNodeLinkType NodeLinkType, string RelationCreatingType)> collection)
        {
            return MapNodeCollection(collection, (node) => node.NodeType.IsEvent, getTitleFunc);
        }

        private static IReadOnlyCollection<RelatedObject> MapNodeCollection(IDictionary<Guid, (INode Node, MaterialNodeLinkType NodeLinkType, string RelationCreatingType)> collection, Func<INode, bool> nodePredicate, Func<INode,string> getTitleProperyFunc)
        {
            var result = new List<RelatedObject>(collection.Count);

            foreach (var element in collection)
            {
                if (!nodePredicate(element.Value.Node)) continue;

                var @object = new RelatedObject(
                    element.Key,
                    getTitleProperyFunc(element.Value.Node),
                    element.Value.Node.NodeType.Name,
                    element.Value.NodeLinkType.ToString(),
                    element.Value.RelationCreatingType);

                result.Add(@object);
            }

            return result;
        }

        private static IReadOnlyCollection<Guid> GetNodeIdCollectionBySignIdCollection(IReadOnlyCollection<Guid> signIdCollection, IOntologyNodesData ontologyData)
        {
            return ontologyData.Relations
                .Where(r => signIdCollection.Contains(r.TargetNodeId))
                .Select(r => r.SourceNodeId)
                .Distinct()
                .ToArray();
        }
    }
}