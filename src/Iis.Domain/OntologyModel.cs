using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Domain
{
    public class OntologyModel: IOntologyModel
    {
        public IEnumerable<INodeTypeModel> Types { get; }

        public IEnumerable<IEntityTypeModel> EntityTypes => Types.OfType<IEntityTypeModel>();

        public OntologyModel(IEnumerable<INodeTypeModel> types)
        {
            Types = types;
        }

        public IEntityTypeModel GetEntityType(string name)
        {
            // TODO: this method is redundant and can be removed
            return GetType<IEntityTypeModel>(name);
        }

        public T GetType<T>(string name) where T : INodeTypeModel
        {
            var type = GetTypeOrNull<T>(name);

            if (type == null)
            {
                throw new ArgumentException($"Type '{name}' does not exist");
            }

            return type;
        }

        public T GetTypeOrNull<T>(string name) where T : INodeTypeModel
        {
            return Types.OfType<T>().SingleOrDefault(type => type.Name == name);
        }

        public IEnumerable<T> GetTypes<T>(string name) where T : INodeTypeModel
        {
            // TODO: remove this method. There should not be types with the same name
            //       this is a temporary hack while we have relations with the same name but different Source/Target
            return Types.OfType<T>().Where(type => type.Name == name);
        }

        public INodeTypeModel GetType(Guid id) => Types.SingleOrDefault(e => e.Id == id);

        public IEnumerable<INodeTypeModel> GetChildTypes(INodeTypeModel type) => EntityTypes.Where(etype => etype.IsSubtypeOf(type));
    }
}
