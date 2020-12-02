namespace Iis.Services.Contracts.Params
{
    public class SearchParams
    {
        public string Suggestion { get; set; }
        public PaginationParams Page {get;set;}
        public SortingParams Sorting { get; set; }
    }
}