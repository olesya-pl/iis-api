using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Desktop.Common.GraphQL
{
    public class GraphQLRequest<TParam>
    {
        [JsonProperty("operationName")]
        public string OperationName { get; set; }
        public TParam Variables { get; set; }
        public string Query { get; set; }
    }
}
