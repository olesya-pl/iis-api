using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Reports
{
    public class ReportInput
    {
        [GraphQLNonNullType] public string Title      { get; set; }
        [GraphQLNonNullType] public string Recipient  { get; set; }
    }
}
