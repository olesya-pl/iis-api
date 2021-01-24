using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.OntologyModelWrapper
{
    public class OntologyWrapper : IOntologyModel
    {
        IOntologySchema _schema;
        public OntologyWrapper(IOntologySchema schema)
        {
            _schema = schema;
        }

        public IEnumerable<INodeTypeLinked> GetEntityTypes() => _schema.GetEntityTypes();

        public INodeTypeLinked GetEntityTypeByName(string name)
        {
            return _schema.GetEntityTypeByName(name);
        }
    }
}
