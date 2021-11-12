namespace Iis.Services.Contracts.Elastic
{
    public static class MaterialAliases
    {
        public static class Source
        {
            public const string Alias = "Джерело";
            public const string Path = "Source.keyword";
        }

        public static class Type
        {
            public const string Alias = "Тип";
            public const string Path = "Type.keyword";
        }

        public static class ProcessedStatus
        {
            public const string Alias = "Статус";
            public const string Path = "ProcessedStatus.Title";
        }

        public static class Completeness
        {
            public const string Alias = "Повнота";
            public const string Path = "Completeness.Title";
        }

        public static class Importance
        {
            public const string Alias = "Важливість";
            public const string Path = "Importance.Title";
        }

        public static class Reliability
        {
            public const string Alias = "Достовірність";
            public const string Path = "Reliability.Title";
        }

        public static class Relevance
        {
            public const string Alias = "Актуальність";
            public const string Path = "Relevance.Title";
        }

        public static class SourceReliability
        {
            public const string Alias = "Надійність_джерела";
            public const string Path = "SourceReliability.Title";
        }

        public static class SessionPriority
        {
            public const string Alias = "Пріоритет_сесії";
            public const string Path = "SessionPriority.Title";
        }

        public static class Assignees
        {
            public const string Alias = "Призначення";
            public const string Path = "Assignees.Id";
            public const string AliasForSingleItem = "Призначено мені";
        }
    }
}