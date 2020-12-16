using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.OntologyManager.Configurations
{
    public class SchemaDataSource : IOntologySchemaSource
    {
        public string Data { get; private set; }
        public SchemaSourceKind SourceKind { get; private set; }
        public string Title { get; private set; }
        public string EnvironmentCode { get; private set; }
        public string ApiAddress { get; private set; }
        private SchemaDataSource() { }
        public static SchemaDataSource CreateForFile(string fileName)
        {
            return new SchemaDataSource
            {
                Title = $"(FILE): {Path.GetFileNameWithoutExtension(fileName)}",
                SourceKind = SchemaSourceKind.File,
                Data = fileName
            };
        }
        public static SchemaDataSource CreateForDb(string environmentCode, string connectionString, string apiAddress)
        {
            return new SchemaDataSource
            {
                SourceKind = SchemaSourceKind.Database,
                Title = $"(DB): {environmentCode}",
                EnvironmentCode = environmentCode,
                Data = connectionString,
                ApiAddress = apiAddress
            };
        }
    }
}
