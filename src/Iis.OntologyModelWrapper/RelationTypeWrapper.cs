using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class RelationTypeWrapper: NodeTypeWrapper, IRelationTypeModel
    {
        public RelationTypeWrapper(INodeTypeLinked source) : base(source) { }
    }
}
