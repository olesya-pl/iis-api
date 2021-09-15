using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.CreateMenu
{
    public class CreateMenuResponse
    {
        [GraphQLType(typeof(ListType<JsonScalarType>))]
        public IReadOnlyList<JObject> Items { get; set; }
    }
}
