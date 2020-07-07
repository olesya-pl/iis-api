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
        public IEnumerable<IEntityTypeModel> EntityTypes => _schema.GetEntityTypes().Select(et => new EntityTypeWrapper(et));

        public IEnumerable<INodeTypeModel> Types => throw new NotImplementedException();

        public IEnumerable<INodeTypeModel> GetChildTypes(INodeTypeModel type)
        {
            throw new NotImplementedException();
        }

        public IEntityTypeModel GetEntityType(string name)
        {
            throw new NotImplementedException();
        }

        public INodeTypeModel GetType(Guid id)
        {
            throw new NotImplementedException();
        }

        public new T GetType<T>(string name) where T : INodeTypeModel
        {
            throw new NotImplementedException();
        }

        public T GetTypeOrNull<T>(string name) where T : INodeTypeModel
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetTypes<T>(string name) where T : INodeTypeModel
        {
            throw new NotImplementedException();
        }
    }
}
