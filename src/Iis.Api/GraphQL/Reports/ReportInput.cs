using HotChocolate;

namespace IIS.Core.GraphQL.Reports
{
    public class ReportInput
    {
        [GraphQLNonNullType] public string Title      { get; set; }
        [GraphQLNonNullType] public string Recipient  { get; set; }
        [GraphQLNonNullType] public int AccessLevel  { get; set; }
        public string Annotation { get; set; }
    }
}
