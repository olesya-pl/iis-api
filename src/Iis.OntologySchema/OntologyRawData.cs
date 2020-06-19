using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema
{
    public class OntologyRawData : IOntologyRawData
    {
        public IReadOnlyList<INodeType> NodeTypes { get; private set; }

        public IReadOnlyList<IRelationType> RelationTypes { get; private set; }

        public IReadOnlyList<IAttributeType> AttributeTypes { get; private set; }

        public IReadOnlyList<IAlias> Aliases { get; private set; }

        public OntologyRawData(
            IEnumerable<INodeType> nodeTypes, 
            IEnumerable<IRelationType> relationTypes, 
            IEnumerable<IAttributeType> attributeTypes,
            IEnumerable<IAlias> aliases)
        {
            NodeTypes = new List<INodeType>(nodeTypes);
            RelationTypes = new List<IRelationType>(relationTypes);
            AttributeTypes = new List<IAttributeType>(attributeTypes);
            Aliases = new List<IAlias>(aliases);
        }
    }
}
