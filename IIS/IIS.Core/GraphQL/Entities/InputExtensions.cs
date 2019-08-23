using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Files;
using IIS.Core.Ontology;
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
            // todo: implement validation - just json or use geo lib?
            // seems like validation is not happening if json scalar is inside other object
            return value;
        }
    }
}
