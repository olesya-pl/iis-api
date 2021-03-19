using Iis.DataModel;
using Iis.DbLayer.OntologyData;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData;
using Iis.Services.Contracts.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services
{
    public class OntologyDataService: IOntologyDataService
    {
        IOntologyNodesData _ontologyData;

        public OntologyDataService(IOntologyNodesData ontologyData)
        {
            _ontologyData = ontologyData;
        }

        public void ReloadOntologyData(string connectionString)
        {
            using var context = OntologyContext.GetContext(connectionString);
            var rawData = new NodesRawData(context.Nodes, context.Relations, context.Attributes);
            _ontologyData.ReloadData(rawData);
        }
    }
}
