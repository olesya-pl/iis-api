using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DbLayer.ModifyDataScripts
{
    public delegate void ModifyDataAction(OntologyContext context, IOntologyNodesData ontologyData);
    public class ModifyDataItem
    {
        public string Name { get; }
        public ModifyDataAction Action { get; }
        public bool HostRestartNeeded { get; }

        public ModifyDataItem(string name, ModifyDataAction action, bool hostRestartNeeded = false)
        {
            Name = name;
            Action = action;
            HostRestartNeeded = hostRestartNeeded;
        }
    }
}
