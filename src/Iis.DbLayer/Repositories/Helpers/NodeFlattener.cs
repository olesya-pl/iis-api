using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.DbLayer.Repositories.Helpers
{
    public class NodeFlattener
    {
        private readonly IElasticSerializer _elasticSerializer;
        private readonly IExtNodeService _extNodeService;

        public NodeFlattener(IElasticSerializer elasticSerializer,
            IExtNodeService extNodeService)
        {
            _elasticSerializer = elasticSerializer;
            _extNodeService = extNodeService;
        }
        
        public async Task<FlattenNodeResult> FlattenNode(Guid id, CancellationToken cancellationToken = default)
        {
            var extNode = await _extNodeService.GetExtNodeAsync(id, cancellationToken);

            return new FlattenNodeResult
            {
                SerializedNode = _elasticSerializer.GetJsonByExtNode(extNode),
                Id = extNode.Id,
                NodeTypeName = extNode.NodeTypeName
            };
        }

        internal List<FlattenNodeResult> FlattenNodes(IReadOnlyCollection<INode> itemsToUpdate)
        {
            return _extNodeService.GetExtNodes(itemsToUpdate)
                .Select(extNode => new FlattenNodeResult
                {
                    SerializedNode = _elasticSerializer.GetJsonByExtNode(extNode),
                    Id = extNode.Id,
                    NodeTypeName = extNode.NodeTypeName
                })
                .ToList();
        }
    }

    public class FlattenNodeResult
    {
        public string SerializedNode { get; set; }
        public string Id { get; internal set; }
        public string NodeTypeName { get; internal set; }
    }
}
