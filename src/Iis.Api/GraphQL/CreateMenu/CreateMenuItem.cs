using HotChocolate;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.CreateMenu
{
    public class CreateMenuItem
    {
        [GraphQLType(typeof(IdType))]
        public Guid NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public string Title { get; set; }
        public List<CreateMenuItem> Children { get; set; }
    }
}
