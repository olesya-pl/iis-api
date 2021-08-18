using Newtonsoft.Json.Linq;
using System;

namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public interface IMaterialRegistrationDateResolver
    {
        bool TryResolve(JObject metadata, out DateTime? registrationDate);
    }
}