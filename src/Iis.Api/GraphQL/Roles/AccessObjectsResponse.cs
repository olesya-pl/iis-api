using System.Collections.Generic;

namespace IIS.Core.GraphQL.Roles
{
    public class AccessObjectsResponse
    {
        public IEnumerable<AccessEntity> Entities { get; set; }
        public IEnumerable<AccessTab> Tabs { get; set; }
    }
}
