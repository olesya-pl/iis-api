using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Desktop.Common.GraphQL
{
    public class GraphQLResponse<TResult>
    {
        public TResult Result { get; private set; }
        public GraphQLError Error { get; private set; }
        public bool IsSuccess => Error == null;

        public GraphQLResponse(TResult result, GraphQLError error)
        {
            Result = result;
            Error = error;
        }
    }
}
