namespace Iis.OntologyManager.Dictionaries
{
    public static class IndexRelativePaths
    {
        public const string Ontology = "ReInitializeOntologyIndexes/all";

        public const string OntologyHistorical = "ReInitializeHistoricalOntologyIndexes/all";

        public const string Signs = "ReInitializeSignIndexes/all";

        public const string Events = "ReInitializeEventIndexes";

        public const string Reports = "RecreateElasticReportIndex";

        public const string Materials = "RecreateElasticMaterialIndexes";
        public const string Wiki = "ReInitializeWikiIndexes/all";
        public const string WikiHistorical = "ReInitializeHistoricalWikiIndexes/all";
    }
}
