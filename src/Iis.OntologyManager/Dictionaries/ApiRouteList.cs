﻿namespace Iis.OntologyManager.Dictionaries
{
    public static class ApiRouteList
    {
        public const string OntologyReIndex = "admin/ReInitializeOntologyIndexes/all";

        public const string OntologyHistoricalReIndex = "admin/ReInitializeHistoricalOntologyIndexes/all";

        public const string SignsReIndex = "admin/ReInitializeSignIndexes/all";

        public const string EventsReIndex = "admin/ReInitializeEventIndexes";

        public const string ReportsReIndex = "admin/RecreateElasticReportIndex";

        public const string MaterialsReIndex = "admin/RecreateElasticMaterialIndexes";
        public const string WikiReIndex = "admin/ReInitializeWikiIndexes/all";
        public const string WikiHistoricalReIndex = "admin/ReInitializeHistoricalWikiIndexes/all";
        public const string UsersReIndex = "admin/RecreateElasticUserIndexes";
        public const string ApplicationRestart = "admin/RestartApplication";
        public const string OntologyReloadData = "admin/ReloadOntologyData";
        public const string AccessLevelChange = "admin/ChangeAccessLevels";
    }
}
