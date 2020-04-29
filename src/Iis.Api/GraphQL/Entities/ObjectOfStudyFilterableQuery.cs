﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Iis.Domain;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Entities
{
    public class ObjectOfStudyFilterableQueryReponse
    {
        [GraphQLType(typeof(ListType<JsonScalarType>))]
        public IEnumerable<JObject> Items { get; set; }
    }

    public class ObjectOfStudyFilterableQuery
    {
        public async Task<ObjectOfStudyFilterableQueryReponse> EntityObjectOfStudyFilterableList(
            [Service] IOntologyService ontologyService,
            ElasticFilter filter
            )
        {
            var items = await ontologyService.FilterObjectsOfStudyAsync(filter);
            return new ObjectOfStudyFilterableQueryReponse
            {
                Items = items
            };
        }
    }
}
