using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DbLayer.Common
{
    public class CommonData : ICommonData
    {
        public IAccessLevels AccessLevels { get; }

        public CommonData(IAccessLevels accessLevels)
        {
            AccessLevels = accessLevels;
        }
    }
}
