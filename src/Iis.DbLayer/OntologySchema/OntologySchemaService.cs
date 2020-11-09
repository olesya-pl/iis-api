using Iis.DataModel;
using Iis.Interfaces.Enums;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema;
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
            var rawData = JsonConvert.DeserializeObject<OntologyRawDataDeserializable>(json);
            var ontologyRawData = new OntologyRawData(rawData.NodeTypes, rawData.RelationTypes, rawData.AttributeTypes, rawData.Aliases);
            var ontologySchema = Iis.OntologySchema.OntologySchema.GetInstance(ontologyRawData, schemaSource);
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
                var ontologySchema = Iis.OntologySchema.OntologySchema.GetInstance(ontologyRawData, schemaSource);
                return ontologySchema;
            }
            catch
            {
                return Iis.OntologySchema.OntologySchema.GetInstance(null, schemaSource);
            }
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

    }
}
