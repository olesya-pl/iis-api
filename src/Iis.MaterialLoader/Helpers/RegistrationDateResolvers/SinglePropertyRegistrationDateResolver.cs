using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public abstract class SinglePropertyRegistrationDateResolver : IMaterialRegistrationDateResolver
    {
        private readonly string propertyName;
        private readonly string dateFormat;

        protected SinglePropertyRegistrationDateResolver(
            string propertyName,
            string dateFormat)
        {
            this.propertyName = propertyName;
            this.dateFormat = dateFormat;
        }

        public virtual bool TryResolve(JObject metadata, out DateTime? registrationDate)
        {
            registrationDate = default;
            if (!metadata.TryGetValue(propertyName, StringComparison.InvariantCultureIgnoreCase, out var residtrationDateToken))
                return false;

            string registrationDateString = residtrationDateToken.Value<string>();
            if (string.IsNullOrWhiteSpace(registrationDateString)
                || !DateTime.TryParseExact(registrationDateString, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime result))
                return false;

            registrationDate = result;

            return true;
        }
    }
}