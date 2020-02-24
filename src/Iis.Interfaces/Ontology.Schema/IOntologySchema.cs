using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IOntologySchema
    {
        void Initialize(IEnumerable<INodeType> nodeTypes, IEnumerable<IRelationType> relationTypes, IEnumerable<IAttributeType> attributeTypes);
        IEnumerable<INodeTypeLinked> GetTypes(IGetTypesFilter filter);
        INodeTypeLinked GetNodeTypeById(Guid id);
    }
}
