namespace IIS.Core.GraphQL.Common
{
    public class PaginationInput
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int Offset() => Page * PageSize - PageSize;
    }
}
