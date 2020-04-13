using Iis.DataModel;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.OntologyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.DbLayer.OntologyEnum
{
    public class NodeEnumService : INodeEnumService
    {
        OntologyContext _ontologyContext;
        IExtNodeService _extNodeService;
        public NodeEnumService(OntologyContext ontologyContext, IExtNodeService extNodeService)
        {
            _ontologyContext = ontologyContext;
        }

        public async Task<INodeEnumValues> GetEnumValues(string typeName, CancellationToken cancellationToken = default)
        {
            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(new[] { typeName }, cancellationToken);
            return new NodeEnumValues(extNodes.Select(extNode => MapToEnumValue(extNode)));
        }

        private INodeEnumValue MapToEnumValue(IExtNode extNode)
        {
            var enumValue = new NodeEnumValue();
            foreach (var child in extNode.Children)
            {
                enumValue.AddProperty(child.NodeTypeName, child.AttributeValue);
            }
            return enumValue;
        }
    }
}
