using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class AttributeTypeWrapper : NodeTypeWrapper, IAttributeTypeModel
    {
        public AttributeTypeWrapper(INodeTypeLinked source) : base(source) { }
        public ScalarType ScalarTypeEnum => throw new NotImplementedException();

        public bool AcceptsScalar(object value)
        {
            throw new NotImplementedException();
        }
    }
}
