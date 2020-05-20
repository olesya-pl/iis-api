using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.ElasticConfig
{
    public class ElasticFieldsInput
    {
        [GraphQLNonNullType]
        public string TypeName { get; set; }
        [GraphQLNonNullType]
        public List<ElasticField> Fields { get; set; }
    }
}
