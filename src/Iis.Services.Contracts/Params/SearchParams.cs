namespace Iis.Services.Contracts.Params
{
    public class SearchParams
    {
        public int Offset { get; set; } 
        public int Limit { get; set; } 
        public string Suggestion { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
    }
}