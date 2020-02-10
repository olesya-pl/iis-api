namespace Iis.Utility
{
    public class ScalarType : ObjectEnum
    {
        public static ScalarType Keyword { get; } = new ScalarType("keyword");
        public static ScalarType String { get; } = new ScalarType("string");
        public static ScalarType Int { get; } = new ScalarType("int");
        public static ScalarType Decimal { get; } = new ScalarType("decimal");
        public static ScalarType Date { get; } = new ScalarType("date");
        public static ScalarType Boolean { get; } = new ScalarType("boolean");
        public static ScalarType Geo { get; } = new ScalarType("geo");
        public static ScalarType File { get; } = new ScalarType("file");
        public static ScalarType Json { get; } = new ScalarType("json");

        protected ScalarType(string value) : base(value) { }

        public static explicit operator ScalarType(string value) => new ScalarType(value);
    }
}
