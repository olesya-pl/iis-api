namespace Iis.Desktop.Common.Requests
{
    public static class ApiRouteList
    {
        public const string OntologyReIndex = "admin/ReInitializeOntologyIndexes/all";
        public const string SignsReIndex = "admin/ReInitializeSignIndexes/all";
        public const string EventsReIndex = "admin/ReInitializeEventIndexes";
        public const string ReportsReIndex = "admin/RecreateElasticReportIndex";
        public const string MaterialsReIndex = "admin/RecreateElasticMaterialIndexes";
        public const string WikiReIndex = "admin/ReInitializeWikiIndexes/all";
        public const string UsersReIndex = "admin/RecreateElasticUserIndexes";
        public const string ApplicationRestart = "admin/RestartApplication";
        public const string OntologyReloadData = "admin/ReloadOntologyData";
        public const string AccessLevelChange = "admin/ChangeAccessLevels";
        public const string GetSecurityLevels = "securityLevel/getSecurityLevels";
        public const string GetUserSecurityDtos = "securityLevel/getUserSecurityDtos";
        public const string SaveUserSecurityDto = "securityLevel/saveUserSecurityDto";
        public const string GetObjectSecurityDtos = "securityLevel/getObjectSecurityDtos";
        public const string SaveObjectSecurityDto = "securityLevel/saveObjectSecurityDto";
        public const string SaveSecurityLevel = "securityLevel/saveSecurityLevel";
        public const string RemoveSecurityLevel = "securityLevel/removeSecurityLevel";
    }
}