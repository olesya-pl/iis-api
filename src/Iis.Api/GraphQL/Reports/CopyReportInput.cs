using HotChocolate;
using System;

namespace IIS.Core.GraphQL.Reports
{
    public class CopyReportInput
    {
        public string Title     { get; set; }
        public string Recipient { get; set; }
    }
}
