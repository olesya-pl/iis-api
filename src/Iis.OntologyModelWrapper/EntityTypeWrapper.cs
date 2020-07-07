using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class EntityTypeWrapper : NodeTypeWrapper, IEntityTypeModel
    {
        public EntityTypeWrapper(INodeTypeLinked source) : base(source) { }
        public bool IsAbstract => _source.IsAbstract;
    }
}
