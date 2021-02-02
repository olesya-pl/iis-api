using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoJSON.Net.Converters;
using GeoJSON.Net.Geometry;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;
using Iis.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Iis.Services.Contracts.Interfaces;
using Iis.OntologySchema.DataTypes;
using Iis.Interfaces.Ontology.Schema;

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

        public static bool IsAssignableFrom(this INodeTypeLinked target, INodeTypeLinked source)
        {
            return source.Name == target.Name || source.AllParents.Any(t => t.Name == target.Name);
        }

        public static Node LoadNodeOfType(this IOntologyService service, Guid targetId, INodeTypeLinked targetType)
        {
            var existingNode = service.GetNode(targetId); // no fields needed, only type
            if (existingNode == null)
                throw new ArgumentException($"Node with id {targetId} not found");
            if (!targetType.IsAssignableFrom(existingNode.Type))
                throw new ArgumentException($"Received node is not of type {targetType.Name}");
            return existingNode;
        }

        public static ElasticFilter CreateNodeFilter(this IResolverContext ctx)
        {
            var pagination = ctx.Argument<PaginationInput>("pagination");
            var filter = ctx.Argument<FilterInput>("filter");
            return new ElasticFilter {Limit = pagination.PageSize, Offset = pagination.Offset(),
                Suggestion = filter?.Suggestion ?? filter?.SearchQuery};
        }

        public static ElasticFilter CreateNodeFilter(this IResolverContext ctx, INodeTypeLinked criteriaType)
        {
            var result = ctx.CreateNodeFilter();
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
                var dict = AttributeType.ValueToDict(value.ToString());
                var jo = JObject.FromObject(dict);
                JsonConvert.DeserializeObject<IGeometryObject>(jo.ToString(), new GeometryConverter());
                return dict;
            }
            catch (Exception e)
            {
                throw new QueryException("Unable to convert input json to GeoJson: " + e.Message);
            }
        }
    }
}
