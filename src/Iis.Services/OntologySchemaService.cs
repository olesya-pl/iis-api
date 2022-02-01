using System;
using System.IO;
using System.Linq;
using Iis.DataModel;
using Iis.DbLayer.OntologySchema;
using Iis.Interfaces.Enums;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema;
using Iis.OntologySchema.Saver;
using Iis.Services.Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Iis.Services
{
    public class OntologySchemaService : IOntologySchemaService
    {
        public IOntologySchema LoadFromFile(IOntologySchemaSource schemaSource)
        {
            var json = File.ReadAllText(schemaSource.Data);
            var rawData = JsonConvert.DeserializeObject<OntologyRawDataDeserializable>(json);
            var ontologyRawData = new OntologyRawData(rawData.NodeTypes, rawData.RelationTypes, rawData.AttributeTypes, rawData.Aliases);
            var ontologySchema = OntologySchema.OntologySchema.GetInstance(ontologyRawData, schemaSource);
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
            try
            {
                var ontologyRawData = new OntologyRawData(
                    context.NodeTypes.AsNoTracking(),
                    context.RelationTypes.AsNoTracking(),
                    context.AttributeTypes.AsNoTracking(),
                    context.Aliases.Where(x => x.Type == AliasType.Ontology).AsNoTracking());
                var ontologySchema = OntologySchema.OntologySchema.GetInstance(ontologyRawData, schemaSource);
                return ontologySchema;
            }
            catch (Exception ex)
            {
                return OntologySchema.OntologySchema.GetInstance(null, schemaSource);
            }
        }

        public void SaveToDatabase(IOntologySchema schema, string connectionString)
        {
            using var context = OntologyContext.GetContext(connectionString);
            var schemaSource = new OntologySchemaSource { SourceKind = SchemaSourceKind.Database, Data = connectionString };
            var oldSchema = GetOntologySchema(schemaSource);
            var compareResult = schema.CompareTo(oldSchema);
            var schemaSaver = new OntologySchemaSaver(context, compareResult.SchemaTo);

            schemaSaver.SaveToDatabase(compareResult);
        }

        public IOntologySchema GetOntologySchema(IOntologySchemaSource schemaSource)
        {
            if (schemaSource == null)
            {
                return new OntologySchema.OntologySchema(null);
            }
            switch (schemaSource.SourceKind)
            {
                case SchemaSourceKind.File:
                    return LoadFromFile(schemaSource);
                case SchemaSourceKind.Database:
                    return LoadFromDatabase(schemaSource);
                case SchemaSourceKind.New:
                    return new OntologySchema.OntologySchema(schemaSource);
            }
            throw new ArgumentException($"Invalid argument sourceKind = {schemaSource.SourceKind}");
        }

    }
}
