using System.Collections.Generic;

namespace IIS.Core.GraphQL.Materials
{
    public class SearchByImageInput
    {
        public string Name { get; set; }
        public IEnumerable<byte> Content { get; set; }
    }
}
