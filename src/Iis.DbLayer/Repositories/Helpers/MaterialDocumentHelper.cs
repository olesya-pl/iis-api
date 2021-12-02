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
        private const string ImportanceNamePropertyName = "importance.name";
        private const string ImportanceSortOrderPropertyName = "importance.sortOrder";
        private const string NoValueFound = "значення відсутне";
        private const int DefaultSortOrder = 99;
        private static readonly Func<INode, string> GetTitleFunc = node => node.GetComputedValue(TitlePropertyName) ?? NoValueFound;
        private static readonly Func<INode, string> GetValueFunc = node => node.GetSingleProperty(ValuePropertyName)?.Value ?? NoValueFound;
        private static readonly MaterialNodeLinkType[] CallerOrReveiver = { MaterialNodeLinkType.Caller, MaterialNodeLinkType.Receiver };
        public static IDictionary<Guid, NodeDataObject> MapFeatureCollectionToNodeDictionary(
            IReadOnlyCollection<MaterialFeatureEntity> collection,
            IOntologyNodesData ontologyData)
        {
            var result = new Dictionary<Guid, NodeDataObject>(collection.Count);

            var groupedCollection = collection.GroupBy(_ => _.NodeId);

            foreach (var group in groupedCollection)
            {
                var key = group.Key;

                var node = ontologyData.GetNode(key);

                if (node is null) continue;

                var feature = group.Count() == 1
                            ? GetAnyFeature(group)
                            : GetCallerOrReceiverFeature(group);

                var element = new NodeDataObject
                {
                    Node = node,
                    NodeLinkType = feature.NodeLinkType,
                    RelationCreatingType = EntityMaterialRelation.Direct
                };

                result.TryAdd(node.Id, element);
            }

            return result;
        }

        public static IReadOnlyDictionary<Guid, NodeDataObject> GetObjectsLinkedBySign(
            IDictionary<Guid, NodeDataObject> nodeDictionary,
            IOntologyNodesData ontologyData)
        {
            var signIdCollection = nodeDictionary
                .Where(e => e.Value.Node.NodeType.IsObjectSign)
                .Select(e => e.Key)
                .ToArray();

            var linkedNodeCollection = GetNodeIdCollectionBySignIdCollection(signIdCollection, ontologyData);

            var result = new Dictionary<Guid, NodeDataObject>(linkedNodeCollection.Count);

            foreach (var nodeElement in linkedNodeCollection)
            {
                var node = ontologyData.GetNode(nodeElement.NodeId);

                if (node is null) continue;

                var element = new NodeDataObject
                {
                    Node = node,
                    NodeLinkType = MaterialNodeLinkType.None,
                    RelatedSignId = nodeElement.FeatureId,
                    RelationCreatingType = EntityMaterialRelation.Feature
                };

                result.TryAdd(node.Id, element);
            }

            return result;
        }

        public static IReadOnlyCollection<RelatedObjectOfStudy> MapObjectOfStudyCollection(IDictionary<Guid, NodeDataObject> collection)
        {
            var result = new List<RelatedObjectOfStudy>(collection.Count);

            foreach (var element in collection)
            {
                if (!element.Value.Node.NodeType.IsObjectOfStudy) continue;

                if (!int.TryParse(element.Value.Node.GetSingleProperty(ImportanceSortOrderPropertyName)?.Value, out var sortOrder))
                {
                    sortOrder = DefaultSortOrder;
                }

                var @object = new RelatedObjectOfStudy(
                    element.Key,
                    GetTitleFunc(element.Value.Node),
                    element.Value.Node.NodeType.Name,
                    element.Value.NodeLinkType.ToString(),
                    element.Value.RelationCreatingType,
                    element.Value.Node.GetSingleProperty(ImportanceNamePropertyName)?.Value ?? NoValueFound,
                    sortOrder,
                    element.Value.RelatedSignId);

                result.Add(@object);
            }
            return result;
        }

        public static IReadOnlyCollection<RelatedObject> MapSingCollection(IDictionary<Guid, NodeDataObject> collection)
        {
            return MapNodeCollection(collection, (node) => node.NodeType.IsObjectSign, GetValueFunc);
        }

        public static IReadOnlyCollection<RelatedObject> MapEventCollection(IDictionary<Guid, NodeDataObject> collection)
        {
            return MapNodeCollection(collection, (node) => node.NodeType.IsEvent, GetTitleFunc);
        }

        private static MaterialFeatureEntity GetAnyFeature(IEnumerable<MaterialFeatureEntity> collection) => collection.FirstOrDefault();
        private static MaterialFeatureEntity GetCallerOrReceiverFeature(IEnumerable<MaterialFeatureEntity> collection) => collection.FirstOrDefault(_ => CallerOrReveiver.Contains(_.NodeLinkType)) ?? collection.FirstOrDefault();
        private static IReadOnlyCollection<RelatedObject> MapNodeCollection(IDictionary<Guid, NodeDataObject> collection, Func<INode, bool> nodePredicate, Func<INode, string> getTitleProperyFunc)
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

        private static IReadOnlyCollection<(Guid NodeId, Guid FeatureId)> GetNodeIdCollectionBySignIdCollection(IReadOnlyCollection<Guid> signIdCollection, IOntologyNodesData ontologyData)
        {
            return ontologyData.Relations
                .Where(_ => signIdCollection.Contains(_.TargetNodeId))
                .Select(_ => (NodeId: _.SourceNodeId, FeatureId: _.TargetNodeId))
                .GroupBy(_ => _.NodeId)
                .Select(_ => _.First())
                .ToArray();
        }
    }

    public class NodeDataObject
    {
        public INode Node { get; set; }
        public MaterialNodeLinkType NodeLinkType { get; set; }
        public Guid? RelatedSignId { get; set; }
        public string RelationCreatingType { get; set; }
    }
}