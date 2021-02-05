using HotChocolate;

namespace IIS.Core.GraphQL.Materials
{
    public class SearchByImageInput
    {
        public string Name { get; set; }
        public string Content { get; set; }
        [GraphQLIgnore]
        public bool HasConditions => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Content);
    }
}
