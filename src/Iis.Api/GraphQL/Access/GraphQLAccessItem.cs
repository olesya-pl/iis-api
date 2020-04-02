using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.Access
{
    public class GraphQLAccessItem
    {
        public AccessKind Kind { get; private set; }
        public AccessOperation Operation { get; private set; }
        public string[] GraphQLPatterns { get; private set; }
        public GraphQLAccessItem(AccessKind kind, AccessOperation operation, params string[] graphQLPatterns)
        {
            Kind = kind;
            Operation = operation;
            GraphQLPatterns = graphQLPatterns;
        }

        public bool IsMatch(string graphQLItem)
        {
            foreach (var pattern in GraphQLPatterns)
            {
                var regex = new Regex(pattern);
                if (regex.IsMatch(graphQLItem))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
