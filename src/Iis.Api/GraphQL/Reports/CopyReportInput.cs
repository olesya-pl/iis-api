namespace IIS.Core.GraphQL.Reports
{
    public class CopyReportInput
    {
        public string Title     { get; set; }
        public string Recipient { get; set; }
        public int AccessLevel { get; set; }
    }
}
