namespace IIS.Core.GraphQL.Reports
{
    public class DeleteEntityReportResponse
    {
        // as requested by client side devs
        public string Type => "Ok";
        public Report Details { get; set; }
    }
}
