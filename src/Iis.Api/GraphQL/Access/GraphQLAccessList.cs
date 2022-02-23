using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Language;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Roles;

namespace Iis.Api.GraphQL.Access
{
    public class GraphQLAccessList : List<GraphQLAccessItem>
    {
        private const string FilterPropertyName = "filter";
        private const string SuggestionPropertyName = "suggestion";
        private const string SearchQueryPropertyName = "searchQuery";
        private const string CherryPickedItemsPropertyName = "cherryPickedItems";
        private const string SearchByImageInputPropertyName = "searchByImageInput";
        private const string SearchByRelationInputPropertyName = "searchByRelationInput";
        private const string HasConditionsPropertyName = "hasConditions";
        private const string FilteredItemsPropertyName = "filteredItems";
        private const string NameProperty = "name";

        public GraphQLAccessList()
        {
            AddRange(new List<GraphQLAccessItem>
            {
                new GraphQLAccessItem(AccessKind.FreeForAll, AccessOperation.None, @"getEntityTypes", @"getEntityTypeIcons"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Create, @"createEntity(?!Event)"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Read, @"getEntity(?!Event)", @"getObjects"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Search, request => request.ContainsKey(FilterPropertyName)
                    && (HasBaseFilterParameter(request)
                        || HasCherryPickedItems(request)
                        || HasOtherFilteredItems(request)), @"getObjects"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Update, @"updateEntity(?!Event)"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Delete, @"deleteEntity.*"),

                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Read, @"getMaterial*"),
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Search, request => (request.ContainsKey(FilterPropertyName) && HasBaseFilterParameter(request))
                    || (request.ContainsKey(SearchByImageInputPropertyName) && HasConditions(request, SearchByImageInputPropertyName))
                    || (request.ContainsKey(SearchByRelationInputPropertyName) && HasConditions(request, SearchByRelationInputPropertyName)), @"getMaterials"),
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Update, @"updateMaterial*"),
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Delete, @"deleteMaterial.*"),

                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Create, @"createEntityEvent"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Read, @"getEvents", @"getEntityEvent"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Search, request => request.ContainsKey(FilterPropertyName) && HasBaseFilterParameter(request), @"getEvents"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Update, @"updateEntityEvent"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Delete, @"deleteEvent.*"),

                new GraphQLAccessItem(AccessKind.Report, AccessOperation.Create, @"createReport"),
                new GraphQLAccessItem(AccessKind.Report, AccessOperation.Read, @"getReports"),
                new GraphQLAccessItem(AccessKind.Report, AccessOperation.Search, request => request.ContainsKey(FilterPropertyName) && HasBaseFilterParameter(request), @"getReports"),
                new GraphQLAccessItem(AccessKind.Report, AccessOperation.Update, @"updateReport*"),

                new GraphQLAccessItem(AccessKind.Material, AccessOperation.AccessLevelUpdate, @"changeMaterialAccessLevel"),
            });
        }

        public IReadOnlyCollection<GraphQLAccessItem> GetAccessItem(string graphQLItem, IReadOnlyDictionary<string, object> variableValues)
        {
            return this.Where(ai => (ai.IsMatch(graphQLItem)) && ai.IsRequestConditionMatch(variableValues)).ToList();
        }

        private static bool HasBaseFilterParameter(IReadOnlyDictionary<string, object> variableValues)
        {
            var filterNode = variableValues[FilterPropertyName];
            if (TryGetChildNode(filterNode, SuggestionPropertyName, out var suggestionNode)
                && TryGetValue<string>(suggestionNode, out var suggestion)
                && !string.IsNullOrWhiteSpace(suggestion)) return true;
            if (TryGetChildNode(filterNode, SearchQueryPropertyName, out var searchQueryNode)
                && TryGetValue<string>(searchQueryNode, out var searchQuery)
                && !string.IsNullOrWhiteSpace(searchQuery)) return true;

            return false;
        }

        private static bool HasConditions(IReadOnlyDictionary<string, object> variableValues, string propertyName)
        {
            var filterNode = variableValues[FilterPropertyName];
            if (TryGetChildNode(filterNode, propertyName, out var node)
                && TryGetChildNode(node, HasConditionsPropertyName, out var conditionsNode)
                && TryGetValue<bool>(conditionsNode, out var hasConditions)
                && hasConditions) return true;

            return false;
        }

        private static bool HasOtherFilteredItems(IReadOnlyDictionary<string, object> variableValues)
        {
            var filterNode = variableValues[FilterPropertyName];
            if (TryGetChildNode(filterNode, FilteredItemsPropertyName, out var node)
                && node.Value is ListValueNode listValueNode
                && listValueNode
                    .Items
                    .Select(_ => (ObjectValueNode)_)
                    .Select(_ => _.Fields.FirstOrDefault(_ => _.Name.Value.Equals(NameProperty, StringComparison.OrdinalIgnoreCase)))
                    .Any(_ => _ != null
                        && TryGetValue<string>(_, out var name)
                        && name != ElasticConfigConstants.NodeTypeTitleAlias)) return true;

            return false;
        }

        private static bool HasCherryPickedItems(IReadOnlyDictionary<string, object> variableValues)
        {
            var filterNode = variableValues[FilterPropertyName];
            if (TryGetChildNode(filterNode, CherryPickedItemsPropertyName, out var cherryPickedItemsNode)
                && cherryPickedItemsNode.Value is ListValueNode listValueNode
                && listValueNode.Items.Count != 0) return true;

            return false;
        }

        private static bool TryGetChildNode(object node, string name, out ObjectFieldNode childNode)
        {
            if (node is List<ObjectFieldNode> objectFieldNode)
            {
                return TryGetChildNode(objectFieldNode, name, out childNode);
            }

            childNode = default;

            return false;
        }

        private static bool TryGetChildNode(List<ObjectFieldNode> node, string name, out ObjectFieldNode childNode)
        {
            childNode = node
                .FirstOrDefault(_ => _ is ObjectFieldNode objectFieldNode
                    && (objectFieldNode.Name.Value?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false));

            return childNode != null;
        }

        private static bool TryGetValue<TValue>(ObjectFieldNode objectFieldNode, out TValue value)
        {
            var pureValue = objectFieldNode.Value?.Value;

            if (pureValue is null
                || !(pureValue is TValue castedValue))
            {
                value = default(TValue);

                return false;
            }

            value = castedValue;

            return true;
        }
    }
}