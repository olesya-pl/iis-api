using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoJSON.Net.Converters;
using GeoJSON.Net.Geometry;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using IIS.Core.Files;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.Ontology;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.Entities
{
    public static class InputExtensions
    {
        public static (string, Dictionary<string, object>) ParseInputUnion(object obj)
        {
            if (obj == null) throw new ArgumentNullException();
            var target = (Dictionary<string, object>) obj;
            if (target.Count != 1)
                throw new ArgumentException("Input union should contain exactly one element");
            var (key, v) = target.Single();
            var value = (Dictionary<string, object>) v;
            return (key, value);
        }

        public static Guid ParseGuid(object obj)
        {
            if (obj == null) throw new ArgumentNullException();
            return Guid.Parse((string) obj);
        }

        public static bool IsAssignableFrom(this Type target, Type source)
        {
            return source.Name == target.Name || source.AllParents.Any(t => t.Name == target.Name);
        }

        public static async Task<Node> LoadNodeOfType(this IOntologyService service, Guid targetId, Type targetType)
        {
            var existingNode = await service.LoadNodesAsync(targetId, null); // no fields needed, only type
            if (existingNode == null)
                throw new ArgumentException($"Node with id {targetId} not found");
            if (!targetType.IsAssignableFrom(existingNode.Type))
                throw new ArgumentException($"Received node is not of type {targetType.Name}");
            return existingNode;
        }

        public static NodeFilter CreateNodeFilter(this IResolverContext ctx)
        {
            var pagination = ctx.Argument<PaginationInput>("pagination");
            var filter = ctx.Argument<FilterInput>("filter");
            return new NodeFilter {Limit = pagination.PageSize, Offset = pagination.Offset(),
                Suggestion = filter?.Suggestion ?? filter?.SearchQuery};
        }

        public static NodeFilter CreateNodeFilter(this IResolverContext ctx, EntityType criteriaType)
        {
            var criteria = ctx.Argument<IDictionary<string, object>>("criteria");
            var result = ctx.CreateNodeFilter();

            if (criteria == null)
                return result;

            result.AnyOfCriteria = TryRemoveFromDictionary<bool>(criteria, CriteriaInputType.ANY_OF_CRITERIA_FIELD);
            result.ExactMatch = TryRemoveFromDictionary<bool>(criteria, CriteriaInputType.EXACT_MATCH_CRITERIA_FIELD);
            foreach (var (key, value) in criteria)
            {
                var relationType = criteriaType.GetProperty(key)
                                   ?? throw new ArgumentException($"Property {key} was not found on type {criteriaType.Name}");
                if (value == null)
                    throw new QueryException($"Can not accept null as value for relation {criteriaType.Name}.{relationType.Name}");
                var strVal = value.ToString();
                if (relationType.IsEntityType && !Guid.TryParse(strVal, out _))
                    throw new QueryException($"Could not parse value '{strVal}' as GUID for relation {criteriaType.Name}.{relationType.Name}");
                result.SearchCriteria.Add(Tuple.Create(relationType, strVal));
            }
            return result;
        }

        private static T TryRemoveFromDictionary<T>(IDictionary<string, object> dict, string key)
        {
            if (!dict.ContainsKey(key))
                return default;
            var result = (T)dict[key];
            dict.Remove(key);
            return result;
        }

        public static async Task<Guid> ProcessFileInput(IFileService fileService, object value)
        {
            // todo: think about file service work, where to store file info (name, etc.) - in ontology or in files db table
            var fileId = ParseGuid(((Dictionary<string, object>) value)["fileId"]);
            var fi = await fileService.GetFileAsync(fileId);
            if (fi == null) throw new ArgumentException($"There is no file with id {fileId}");
            if (fi.IsTemporary)
                await fileService.MarkFilePermanentAsync(fileId);
            return fileId;
        }

        public static object ProcessGeoInput(object value)
        {
            try
            {
                var dict = (Dictionary<string, object>) value;
                var jo = JObject.FromObject(dict);
                JsonConvert.DeserializeObject<IGeometryObject>(jo.ToString(), new GeometryConverter());
                return value;
            }
            catch (Exception e)
            {
                throw new QueryException("Unable to convert input json to GeoJson: " + e.Message);
            }
        }
    }
}
