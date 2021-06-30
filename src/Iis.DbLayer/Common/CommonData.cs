using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Common;
using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DbLayer.Common
{
    public class CommonData : ICommonData
    {
        private IOntologyNodesData _ontologyData;
        private IAccessLevels _accessLevels;
        public IAccessLevels AccessLevels =>
            _accessLevels ?? (_accessLevels = _ontologyData.GetAccessLevels());

        public CommonData(IOntologyNodesData ontologyData)
        {
            _ontologyData = ontologyData;
            _ontologyData.OnAccessLevelsChanged += () => { _accessLevels = null; };
        }
    }
}
