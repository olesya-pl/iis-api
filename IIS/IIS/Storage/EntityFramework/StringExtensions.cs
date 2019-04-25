namespace IIS.Storage.EntityFramework
{
    internal static class StringExtensions
    {
        public static string Camelize(this string value) => value.Substring(0, 1).ToUpper() + value.Substring(1);
    }
}
