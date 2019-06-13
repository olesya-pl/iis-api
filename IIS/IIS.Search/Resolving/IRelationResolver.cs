using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.Search.Resolving
{
    public interface IRelationResolver
    {
        Task<object> ResolveAsync(ResolveContext context);
    }

    public class ResolveContext
    {
        public string RelationName { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public object Source { get; set; }
    }
}
