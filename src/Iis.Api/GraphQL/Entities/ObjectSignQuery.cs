using HotChocolate;
using Iis.Api.GraphQL.Entities.ObjectTypes;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Entities
{
    public class ObjectSignQuery
    {
        [GraphQLNonNullType]
        public Task<GraphQLCollection<OntologyValue>> GetValuesByEntityType(string entityType, int limit)
        {
            var list = new List<OntologyValue> 
            { 
                new OntologyValue { Id = new Guid("499ed2b1f5f844368be5089bd093a808"), Value = "1234567890"},
                new OntologyValue { Id = new Guid("cea763b57fef4f349fd4961e32634896"), Value = "0987654321"}
            };
            return Task.FromResult(new GraphQLCollection<OntologyValue>(list, list.Count));
        }
    }
}
