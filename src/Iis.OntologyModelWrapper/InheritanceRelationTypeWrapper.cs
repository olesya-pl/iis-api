using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class InheritanceRelationTypeWrapper : RelationTypeWrapper, IInheritanceRelationTypeModel
    {
        public InheritanceRelationTypeWrapper(INodeTypeLinked source) : base(source) { }
        public IEntityTypeModel ParentType => throw new NotImplementedException();
    }
}
