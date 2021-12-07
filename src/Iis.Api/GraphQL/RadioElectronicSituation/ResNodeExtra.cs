using System;
using System.Collections.Generic;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class ResNodeExtra
    {
        public DateTime RegisteredAt { get; set; }
        public ResNodeExtraSign Sign { get; set; }
        public IReadOnlyList<ResNodeExtraObject> Objects { get; set; }
        public IReadOnlyList<ResMaterial> Materials { get; set; }
    }
}
