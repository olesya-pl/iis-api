using System.Linq;

namespace Iis.Utility
{
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
        public static string ShortFio(object firstName, object fatherName, object lastName)
        {
            if (string.IsNullOrEmpty(lastName?.ToString())) return FullTitle(firstName, fatherName);
            if (string.IsNullOrEmpty(firstName?.ToString())) return lastName.ToString();
            return $"{lastName} {firstName.ToString().Substring(0, 1)}." +
                (string.IsNullOrEmpty(fatherName?.ToString()) ? string.Empty : fatherName.ToString().Substring(0, 1));
        }
    }
}
