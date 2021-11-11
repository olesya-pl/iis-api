namespace Iis.Elastic.SearchQueryExtensions.CompositeBuilders
{
    internal static class QueryTerms
    {
        public static class Common
        {
            public const string Lenient = "lenient";
            public const string Field = "field";
        }

        public static class Conditions
        {
            public const string Query = "query";
            public const string Bool = "bool";
            public const string QueryString = "query_string";
            public const string Exists = "exists";
            public const string Match = "match";
        }
    }
}