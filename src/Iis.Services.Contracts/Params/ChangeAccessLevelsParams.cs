using Iis.Interfaces.AccessLevels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Params
{
    public class ChangeAccessLevelsParams
    {
        public List<AccessLevel> AccessLevelList { get; set; }
        public Dictionary<Guid, Guid> DeletedMappings { get; set; }
    }
}
