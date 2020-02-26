﻿using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Iis.OntologyManager.Ontology
{
    public class OntologySchemaService
    {
        public IOntologySchema LoadFromFile(string fileName)
        {
            var json = File.ReadAllText(fileName);
            var rawData = JsonConvert.DeserializeObject<OntologyRawDataDeserializable>(json);
            var ontologyRawData = new OntologyRawData(rawData.NodeTypes, rawData.RelationTypes, rawData.AttributeTypes);
            var ontologySchema = OntologySchema.OntologySchema.GetInstance(ontologyRawData);
            return ontologySchema;
        }

        public void SaveToFile(IOntologySchema ontologySchema, string fileName)
        {
            var ontologyRawData = ontologySchema.GetRawData();
            var json = JsonConvert.SerializeObject(ontologyRawData, Formatting.Indented);
            File.WriteAllText(fileName, json);
        }

        public IOntologySchema LoadFromDatabase(string connectionString)
        {
            using var context = OntologyContext.GetContext(connectionString);
            var ontologyRawData = new OntologyRawData(context.NodeTypes, context.RelationTypes, context.AttributeTypes);
            var ontologySchema = OntologySchema.OntologySchema.GetInstance(ontologyRawData);
            return ontologySchema;
        }

        public IOntologySchema GetOntologySchema(OntologySchemaSource schemaSource)
        {
            switch (schemaSource.SourceKind)
            {
                case SchemaSourceKind.File:
                    return LoadFromFile(schemaSource.Data);
                case SchemaSourceKind.Database:
                    return LoadFromDatabase(schemaSource.Data);
            }
            throw new ArgumentException($"Invalid argument sourceKind = {schemaSource.SourceKind}");
        }

    }
}
