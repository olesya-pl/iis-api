namespace Iis.Services.Contracts.Params
{
    public class ReportSearchParams
    {
        public int PageSize { get; set; } 
        public int Offset { get; set; } 
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public string Suggestion { get; set; }
    }
}
