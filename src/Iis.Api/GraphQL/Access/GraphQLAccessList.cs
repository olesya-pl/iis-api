using Iis.Interfaces.Roles;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Materials;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Api.GraphQL.Access
{
    public class GraphQLAccessList: List<GraphQLAccessItem>
    {
        public GraphQLAccessList()
        {
            AddRange(new List<GraphQLAccessItem>
            {
                new GraphQLAccessItem(AccessKind.FreeForAll, AccessOperation.None, @"getEntityTypes", @"getEntityTypeIcons"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Create, @"createEntity*"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Read, @"getEntity*", @"getObjects"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Search, request => {
                    if (!request.ContainsKey("filter"))
                    {
                        return false;
                    }
                    var filter = JsonConvert.DeserializeObject<AllEntitiesFilterInput>(JsonConvert.SerializeObject(request["filter"]));
                    if (filter == null)
                    {
                        return false;
                    }
                    return !string.IsNullOrEmpty(filter?.Suggestion ?? filter?.SearchQuery) || filter.CherryPickedItems.Any() || filter.FilteredItems.Any();
                }, @"getObjects"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Update, @"updateEntity*"),
                new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Delete, @"deleteEntity.*"),

                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Read, @"getMaterial*"),
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Search, request => {
                    var filterNotEmpty = false;
                    var imageNotEmpty = false;
                    var relationNotEmpty = false;
                    if (request.ContainsKey("filter"))
                    {
                        var filter = JsonConvert.DeserializeObject<FilterInput>(JsonConvert.SerializeObject(request["filter"]));
                        if (filter != null)
                        {
                            filterNotEmpty = !string.IsNullOrEmpty(filter?.Suggestion ?? filter?.SearchQuery);
                        }                        
                    }

                    if (request.ContainsKey("searchByImageInput"))
                    {
                        var searchByImageInput = JsonConvert.DeserializeObject<SearchByImageInput>(JsonConvert.SerializeObject(request["searchByImageInput"]));
                        if (searchByImageInput != null)
                        {
                            imageNotEmpty = searchByImageInput.HasConditions;
                        }
                    }

                    if (request.ContainsKey("SearchByRelationInput"))
                    {
                        var searchByRelation = JsonConvert.DeserializeObject<SearchByRelationInput>(JsonConvert.SerializeObject(request["searchByRelation"]));
                        if (searchByRelation != null)
                        {
                            relationNotEmpty = searchByRelation.HasConditions;
                        }
                    }

                    return filterNotEmpty || imageNotEmpty || relationNotEmpty; }, @"getMaterials"),
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Update, @"updateMaterial*"),
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Delete, @"deleteMaterial.*"),

                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Create, @"createEntityEvent"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Read, @"getEvents"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Search, request => {
                if (!request.ContainsKey("filter"))
                    {
                        return false;
                    }
                    var filter = JsonConvert.DeserializeObject<FilterInput>(JsonConvert.SerializeObject(request["filter"]));
                    if (filter == null)
                    {
                        return false;
                    }
                    return !string.IsNullOrEmpty(filter?.Suggestion ?? filter?.SearchQuery);
                }, @"getEvents"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Update, @"updateEntityEvent"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Delete, @"deleteEvent.*"),

                new GraphQLAccessItem(AccessKind.Report, AccessOperation.Create, @"createReport"),
                new GraphQLAccessItem(AccessKind.Report, AccessOperation.Read, @"getReports"),
                new GraphQLAccessItem(AccessKind.Report, AccessOperation.Search, request => {
                    if (!request.ContainsKey("filter"))
                    {
                        return false;
                    }
                    var filter = JsonConvert.DeserializeObject<FilterInput>(JsonConvert.SerializeObject(request["filter"]));
                    if (filter == null)
                    {
                        return false;
                    }
                    return !string.IsNullOrEmpty(filter?.Suggestion ?? filter?.SearchQuery);
                }, @"getReports"),
                new GraphQLAccessItem(AccessKind.Report, AccessOperation.Update, @"updateReport*"),

                new GraphQLAccessItem(AccessKind.Material, AccessOperation.AccessLevelUpdate, @"changeMaterialAccessLevel"),
            });
        }

        public IReadOnlyCollection<GraphQLAccessItem> GetAccessItem(string graphQLItem, IReadOnlyDictionary<string, object> variableValues)
        {
            return this.Where(ai => ai.IsMatch(graphQLItem) && ai.IsRequestConditionMatch(variableValues)).ToList();
        }
    }
}
