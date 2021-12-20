using System;
using System.Collections.Generic;
using System.Text;
using Iis.Interfaces.AccessLevels;

namespace Iis.Services.Contracts.Params
{
    public class ChangeAccessLevelsParams
    {
        public List<AccessLevel> AccessLevelList { get; set; }
        public Dictionary<Guid, Guid> DeletedMappings { get; set; }
    }
}
