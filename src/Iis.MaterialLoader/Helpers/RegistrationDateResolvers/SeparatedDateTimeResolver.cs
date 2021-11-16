using Iis.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public abstract class SeparatedDateTimeResolver : IMaterialRegistrationDateResolver
    {
        private readonly IReadOnlyCollection<string> _dateFormats;
        private readonly string _timePropertyName;
        private readonly string _datePropertyName;

        protected SeparatedDateTimeResolver(
            string dateFormat = "dd.MM.yyyy",
            string timePropertyName = "regTime",
            string datePropertyName = "regDate")
        {
            _dateFormats = dateFormat.AsArray();
            _timePropertyName = timePropertyName;
            _datePropertyName = datePropertyName;
        }

        protected SeparatedDateTimeResolver(
            IReadOnlyCollection<string> dateFormats,
            string timePropertyName = "regTime",
            string datePropertyName = "regDate")
        {
            _dateFormats = dateFormats;
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
                || !TryParseDate(registrationDateString, out var registrationDatePart))
                return false;

            registrationDate = registrationDatePart.Add(registrationTime);

            return true;
        }

        private bool TryParseDate(string registrationDateString, out DateTime date)
        {
            date = default;

            foreach (var dateFormat in _dateFormats)
            {
                if (DateTime.TryParseExact(
                    registrationDateString,
                    dateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out date))
                {
                    return true;
                }
            }

            return false;
        }
    }
}