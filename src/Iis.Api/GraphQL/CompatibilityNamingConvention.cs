using System;
using HotChocolate;
using HotChocolate.Types.Descriptors;

namespace Iis.Api.GraphQL
{
    public class CompatibilityNamingConvention : DefaultNamingConventions
    {
        public override NameString GetEnumValueName(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.ToString().ToUpperInvariant();
        }
    }
}