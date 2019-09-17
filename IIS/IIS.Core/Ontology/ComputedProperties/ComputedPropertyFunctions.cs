using System.Linq;

namespace IIS.Core.Ontology.ComputedProperties
{
    // TODO: Through DI and into domain
    public static class ComputedPropertyFunctions
    {
        public static string Join(params object[] values)
        {
            return string.Join(' ', values.Where(v => v != null));
        }

        public static string FullTitle(object first, object second)
        {
            if (first != null && second != null)
                return $"{first} ({second})";
            return (first ?? second)?.ToString();
        }
    }
}
