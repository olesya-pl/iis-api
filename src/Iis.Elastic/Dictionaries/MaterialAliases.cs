namespace Iis.Elastic.Dictionaries
{
    public static class MaterialAliases
    {
        public class Source
        {
            public const string Alias = "Джерело";
            public const string Path = "Source.keyword";
        }

        public class Type
        {
            public const string Alias = "Тип";
            public const string Path = "Type.keyword";
        }

        public class ProcessedStatus
        {
            public const string Alias = "Статус";
            public const string Path = "ProcessedStatus.Title";
        }

        public class Completeness
        {
            public const string Alias = "Повнота";
            public const string Path = "Completeness.Title";
        }

        public class Importance
        {
            public const string Alias = "Важливість";
            public const string Path = "Importance.Title";
        }

        public class Reliability
        {
            public const string Alias = "Достовірність";
            public const string Path = "Reliability.Title";
        }

        public class Relevance
        {
            public const string Alias = "Актуальність";
            public const string Path = "Relevance.Title";
        }

        public class SourceReliability
        {
            public const string Alias = "Надійність_джерела";
            public const string Path = "SourceReliability.Title";
        }

        public class SessionPriority
        {
            public const string Alias = "Пріоритет_сесії";
            public const string Path = "SessionPriority.Title";
        }
    }
}