using System;
using System.Collections.Generic;
namespace Iis.Services.Contracts.Params
{
    public class ChangeHistoryParams
    {
        public ISet<Guid> EntityIdentityList { get; set; }
        public string PropertyName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool ApplyAliases { get; set; }
    }
}
