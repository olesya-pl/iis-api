using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.Mocks
{
    public class OntologyServiceMock : IOntologyService
    {
        private readonly Dictionary<Guid, Node> _nodes = new Dictionary<Guid, Node>();
        private IOntologyTypesService _typesService;

        public OntologyServiceMock(IOntologyTypesService typesService)
        {
            _typesService = typesService;

            AddNode("ObjectImportance", "ae1560f24d3e4ed49ced155bdea89cd7",
                new Dictionary<string, object> {["code"] = "important", ["name"] = "IMPORTANT"});
            AddNode("ObjectAffiliation", "8434d67e27eb4ae18d66e4623fa399ef",
                new Dictionary<string, object> {["code"] = "enemy", ["name"] = "ENEMY"});
            AddNode("Person", "ab4176d839114cdd83dab452584e4316", new Dictionary<string, object>
            {
                ["importance"] = "ae1560f24d3e4ed49ced155bdea89cd7",
                ["affiliation"] = "8434d67e27eb4ae18d66e4623fa399ef",
                ["firstName"] = "John"
            });
        }

        private void AddNode(string typeName, string guid, Dictionary<string, object> props)
        {
            var node = CreateNode(typeName, Guid.Parse(guid), props);
            _nodes.Add(node.Id, node);
        }

        private Node CreateNode(string typeName, Guid id, Dictionary<string, object> props)
        {
            var type = _typesService.GetEntityType(typeName);
            var entity = new Entity(id, type);
            foreach (var (key, value) in props)
                AddRelation(entity, key, value);
            return entity;
        }

        private void AddRelation(Node source, string targetProperty, object targetValue)
        {
            var targetType = source.Type.GetProperty(targetProperty) ?? throw new ArgumentException($"No property {targetProperty} on {source.Type.Name}");
            var rel = new Relation(Guid.NewGuid(), targetType);
            Node target;
            if (targetValue is string s && Guid.TryParse(s, out var guid))
                target = _nodes[guid];
            else
                target = new Ontology.Attribute(Guid.NewGuid(), targetType.AttributeType, targetValue);
            rel.AddNode(target);
            source.AddNode(rel);
        }

        // -----------

        public async Task<IEnumerable<Node>> GetNodesByTypeAsync(Type type, CancellationToken cancellationToken = default)
        {
            return _nodes.Values.Where(n => n.Type.Name == type.Name);
        }

        public Task<IDictionary<string, IEnumerable<Node>>> GetNodesByTypesAsync(IEnumerable<string> typeNames, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default)
        {
            return _nodes[nodeId];
        }

        public Task<IDictionary<Guid, Node>> LoadNodesAsync(IEnumerable<Guid> sourceIds, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SaveTypeAsync(Type type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveTypeAsync(string typeName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task SaveNodeAsync(Node node, CancellationToken cancellationToken = default)
        {
            foreach (var n in node.Nodes)
                await SaveNodeAsync(n, cancellationToken);
            _nodes[node.Id] = node;
        }

        public async Task RemoveNodeAsync(Guid nodeId, CancellationToken cancellationToken = default)
        {
            _nodes.Remove(nodeId);
        }
    }
}
