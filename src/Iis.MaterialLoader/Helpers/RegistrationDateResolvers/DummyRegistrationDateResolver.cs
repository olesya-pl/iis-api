using Newtonsoft.Json.Linq;
using System;

namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public class DummyRegistrationDateResolver : IMaterialRegistrationDateResolver
    {
        public bool TryResolve(JObject metadata, out DateTime? registrationDate)
        {
            registrationDate = default;

            return true;
        }
    }
}