using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Iis.OntologyManager
{
    public class OntologySchemaService
    {
        public IOntologySchema LoadFromFile(string fileName)
        {
            var json = File.ReadAllText(fileName);
            var ontologyRawData = JsonConvert.DeserializeObject<IOntologyRawData>(json);
            var ontologySchema = new Iis.OntologySchema.OntologySchema();
            ontologySchema.Initialize(ontologyRawData);
            return ontologySchema;
        }

        public void SaveToFile(IOntologySchema ontologySchema, string fileName)
        {
            var ontologyRawData = ontologySchema.GetRawData();
            var json = JsonConvert.SerializeObject(ontologyRawData);
            File.WriteAllText(fileName, json);
        }
    }
}
