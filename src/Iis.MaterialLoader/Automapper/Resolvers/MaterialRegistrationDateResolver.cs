using AutoMapper;
using Iis.MaterialLoader.Models;
using System;
using Iis.Utility;
using Newtonsoft.Json.Linq;
using Iis.MaterialLoader.Helpers.RegistrationDateResolvers;

namespace Iis.MaterialLoader.Automapper.Resolvers
{
    public class MaterialRegistrationDateResolver : IMemberValueResolver<MaterialInput, Domain.Materials.Material, string, DateTime?>
    {
        private const string Source = "source";

        public DateTime? Resolve(MaterialInput source, Domain.Materials.Material destination, string sourceMember, DateTime? destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(sourceMember)
                || !sourceMember.TryParseToJObject(out var parsed)
                || !parsed.TryGetValue(Source, StringComparison.InvariantCultureIgnoreCase, out var sourceJObject))
                return default;

            string materialSource = sourceJObject.Value<string>();
            if (string.IsNullOrWhiteSpace(materialSource))
                return default;

            var resolver = RegistrationDateResolverFactory.Create(materialSource);

            resolver.TryResolve(parsed, out DateTime? registrationDate);

            return registrationDate;
        }
    }
}