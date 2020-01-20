using HotChocolate;
using System;
using System.Collections.ObjectModel;

namespace IIS.Core.GraphQL.Reports
{
    public class UpdateReportData
    {
        public Collection<Guid> AddEvents    { get; set; } = new Collection<Guid>();
        public Collection<Guid> RemoveEvents { get; set; } = new Collection<Guid>();
    }
}
