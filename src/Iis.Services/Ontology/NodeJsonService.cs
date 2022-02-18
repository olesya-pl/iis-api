using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Ontology;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Ontology
{
    public class NodeJsonService : INodeJsonService
    {
        public JObject GetJObject(INode node, GetEntityOptions options)
        {
            return null;
        }
    }
}
