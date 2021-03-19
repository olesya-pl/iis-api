using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DbLayer.Common
{
    public class CommonData : ICommonData
    {
        Func<IAccessLevels> _getAccessLevelsFunc;
        private IAccessLevels _accessLevels;
        public IAccessLevels AccessLevels =>
            _accessLevels ?? (_accessLevels = _getAccessLevelsFunc());

        public CommonData(Func<IAccessLevels> getAccessLevelsFunc)
        {
            _getAccessLevelsFunc = getAccessLevelsFunc;
        }
    }
}
