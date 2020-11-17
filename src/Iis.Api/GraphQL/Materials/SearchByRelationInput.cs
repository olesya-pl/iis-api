using System;
using System.Linq;
using System.Collections.Generic;
using HotChocolate;

namespace IIS.Core.GraphQL.Materials
{
    public class SearchByRelationInput
    {
        public IEnumerable<Guid> NodeIdentityList { get; set; }
        public bool IncludeDescendants { get; set; }
        [GraphQLIgnore]
        public bool ShoudBeExecuted => NodeIdentityList != null && NodeIdentityList.Any();
    }
}
