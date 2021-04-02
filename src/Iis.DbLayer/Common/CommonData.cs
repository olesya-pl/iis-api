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
        public IAccessLevels AccessLevels { get; private set; }

        public CommonData(IOntologyNodesData ontologyData)
        {
            _ontologyData = ontologyData;
            _ontologyData.OnAccessLevelsChanged += RefreshAccessLevels;
            RefreshAccessLevels();
        }

        private void RefreshAccessLevels() =>
            AccessLevels = _ontologyData.GetAccessLevels();
    }
}
