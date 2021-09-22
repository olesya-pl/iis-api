using HotChocolate;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.CreateMenu
{
    public class CreateMenuItemsQuery
    {
        public CreateMenuResponse GetCreateMenuItems([Service] IOntologySchema schema)
        {
            var list = new List<CreateMenuItem>();
            AddCreateMenuItem(list, EntityTypeNames.ObjectOfStudy.ToString(), schema);
            AddCreateMenuItem(list, EntityTypeNames.Wiki.ToString(), schema);
            return new CreateMenuResponse { Items = list.Select(_ => JObject.FromObject(_)).ToList() };
        }

        private void AddCreateMenuItem(List<CreateMenuItem> list, string entityTypeName, IOntologySchema schema)
        {
            var nodeType = schema.GetEntityTypeByName(entityTypeName);
            var createMenuItem = GetCreateMenuItem(nodeType);

            if (createMenuItem != null)
                list.Add(createMenuItem);
        }

        private CreateMenuItem GetCreateMenuItem(INodeTypeLinked nodeType)
        {
            if (nodeType == null) return null;

            var children = nodeType.GetDirectDescendants().OrderBy(nt => nt.Title).ToList();

            if (nodeType.IsAbstract && children.Count == 0) return null;

            var result = new CreateMenuItem
            {
                NodeTypeId = nodeType.Id,
                NodeTypeName = nodeType.Name,
                Title = nodeType.Title
            };

            if (children.Count > 0)
            {
                result.Children = children
                    .Select(GetCreateMenuItem)
                    .Where(m => m != null)
                    .ToList();
            }

            return result;
        }
    }
}
