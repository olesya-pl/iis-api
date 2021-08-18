using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public abstract class SeparatedDateTimeResolver : IMaterialRegistrationDateResolver
    {
        private readonly string _dateFormat;
        private readonly string _timePropertyName;
        private readonly string _datePropertyName;

        protected SeparatedDateTimeResolver(
            string dateFormat = "dd.MM.yyyy",
            string timePropertyName = "regTime",
            string datePropertyName = "regDate")
        {
            _dateFormat = dateFormat;
            _timePropertyName = timePropertyName;
            _datePropertyName = datePropertyName;
        }

        public bool TryResolve(JObject metadata, out DateTime? registrationDate)
        {
            registrationDate = default;
            if (!metadata.TryGetValue(_timePropertyName, StringComparison.InvariantCultureIgnoreCase, out var registrationTimeToken)
                || !metadata.TryGetValue(_datePropertyName, StringComparison.InvariantCultureIgnoreCase, out var registrationDateToken))
                return false;

            string registrationTimeString = registrationTimeToken.Value<string>();
            string registrationDateString = registrationDateToken.Value<string>();
            if (string.IsNullOrWhiteSpace(registrationTimeString)
                || string.IsNullOrWhiteSpace(registrationDateString)
                || !TimeSpan.TryParse(registrationTimeString, out TimeSpan registrationTime)
                || !DateTime.TryParseExact(registrationDateString, _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime registrationDatePart))
                return false;

            registrationDate = registrationDatePart.Add(registrationTime);

            return true;
        }
    }
}