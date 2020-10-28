using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema;
using Iis.OntologySchema.Saver;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iis.DbLayer.OntologySchema
{
    public class OntologySchemaService
    {
        public IOntologySchema LoadFromFile(IOntologySchemaSource schemaSource)
        {
            var json = File.ReadAllText(schemaSource.Data);
            return LoadFromJson(json);
        }
        public IOntologySchema LoadFromJson(string json, IOntologySchemaSource schemaSource = null)
        {
            var rawData = JsonConvert.DeserializeObject<OntologyRawDataDeserializable>(json);
            var ontologyRawData = new OntologyRawData(rawData.NodeTypes, rawData.RelationTypes, rawData.AttributeTypes, rawData.Aliases);
            var ontologySchema = Iis.OntologySchema.OntologySchema.GetInstance(ontologyRawData, 
                schemaSource ?? new OntologySchemaSource { SourceKind = SchemaSourceKind.File });
            return ontologySchema;
        }

        public void SaveToFile(IOntologySchema ontologySchema, string fileName)
        {
            var ontologyRawData = ontologySchema.GetRawData();
            var json = JsonConvert.SerializeObject(ontologyRawData, Formatting.Indented);
            File.WriteAllText(fileName, json);
        }

        public IOntologySchema LoadFromDatabase(IOntologySchemaSource schemaSource)
        {
            using var context = OntologyContext.GetContext(schemaSource.Data);
            var ontologyRawData = new OntologyRawData(
                context.NodeTypes.AsNoTracking(),
                context.RelationTypes.AsNoTracking(),
                context.AttributeTypes.AsNoTracking(),
                context.Aliases.AsNoTracking());
            var ontologySchema = Iis.OntologySchema.OntologySchema.GetInstance(ontologyRawData, schemaSource);
            return ontologySchema;
        }

        public IOntologySchema GetOntologySchema(IOntologySchemaSource schemaSource)
        {
            if (schemaSource == null)
            {
                return new Iis.OntologySchema.OntologySchema(null);
            }
            switch (schemaSource.SourceKind)
            {
                case SchemaSourceKind.File:
                    return LoadFromFile(schemaSource);
                case SchemaSourceKind.Database:
                    return LoadFromDatabase(schemaSource);
                case SchemaSourceKind.New:
                    return new Iis.OntologySchema.OntologySchema(schemaSource);
            }
            throw new ArgumentException($"Invalid argument sourceKind = {schemaSource.SourceKind}");
        }
        public void UpdateOntologySchemaFromJson(OntologyRawDataDeserializable rawData, string connectionString)
        {
            var ontologyRawData = new OntologyRawData(rawData.NodeTypes, rawData.RelationTypes, rawData.AttributeTypes, rawData.Aliases);
            var schemaFrom = Iis.OntologySchema.OntologySchema.GetInstance(ontologyRawData, new OntologySchemaSource { SourceKind = SchemaSourceKind.File });

            var schemaTo = GetOntologySchema(
                new OntologySchemaSource 
                { 
                    SourceKind = SchemaSourceKind.Database, 
                    Data = connectionString 
                });

            var context = OntologyContext.GetContext(connectionString);
            var schemaSaver = new OntologySchemaSaver(context);
            var parameters = new SchemaSaveParameters
            {
                Create = true,
                Update = true,
                Delete = true,
                Aliases = true
            };
            var compareResult = schemaFrom.CompareTo(schemaTo);
            schemaSaver.SaveToDatabase(compareResult, schemaTo, parameters);
        }
    }
}
