using System.Collections.Generic;

namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public class SatIridiumRegistrationDateResolver : SeparatedDateTimeResolver, IMaterialRegistrationDateResolver
    {
        private static readonly IReadOnlyCollection<string> DateFormats = new[] { "yyyy.MM.dd", "dd.MM.yyyy" };

        public SatIridiumRegistrationDateResolver()
            : base(DateFormats)
        {
        }
    }
}