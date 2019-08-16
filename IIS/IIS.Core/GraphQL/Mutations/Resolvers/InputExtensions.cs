using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.Mutations.Resolvers
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

        public static bool IsAssignableFrom(this Type target, Type source) =>
            source.Name == target.Name || source.AllParents.Any(t => t.Name == target.Name);

        public static async Task<Node> LoadNodeOfType(this IOntologyService service, Guid targetId, Type targetType)
        {
            var existingNode = await service.LoadNodesAsync(targetId, null); // no fields needed, only type
            if (existingNode == null)
                throw new ArgumentException($"Node with id {targetId} not found");
            if (!targetType.IsAssignableFrom(existingNode.Type))
                throw new ArgumentException($"Received node is not of type {targetType.Name}");
            return existingNode;
        }

        public static Guid ProcessFileInput(FileValueInput value)
        {
            // todo: mark file as permanent through IFileService
            return value.FileId;
        }
    }
}
