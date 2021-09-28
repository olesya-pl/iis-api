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
        public IReadOnlyCollection<string> GraphQLPatterns { get; private set; }
        public Predicate<IReadOnlyDictionary<string, object>> RequestCondition { get; private set; }
        public GraphQLAccessItem(AccessKind kind, AccessOperation operation, params string[] graphQLPatterns)
        {
            Kind = kind;
            Operation = operation;
            GraphQLPatterns = graphQLPatterns;
        }

        public GraphQLAccessItem(AccessKind kind, AccessOperation operation, Predicate<IReadOnlyDictionary<string, object>> requestCondition, params string[] graphQLPatterns)
        {
            Kind = kind;
            Operation = operation;
            RequestCondition = requestCondition;
            GraphQLPatterns = graphQLPatterns;
        }

        public bool IsMatch(string graphQLItem)
        {
            foreach (var pattern in GraphQLPatterns)
            {
                var regex = new Regex(pattern, RegexOptions.Compiled);
                if (string.Equals(pattern, graphQLItem, StringComparison.Ordinal) || regex.IsMatch(graphQLItem))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsRequestConditionMatch(IReadOnlyDictionary<string, object> request)
        {
            return RequestCondition == null || RequestCondition(request);
        }
    }
}
