using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.Report.EntityFramework
{
    public class Report
    {
        public Guid Id            { get; set; }
        public string Title       { get; set; }
        public string Recipient   { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
