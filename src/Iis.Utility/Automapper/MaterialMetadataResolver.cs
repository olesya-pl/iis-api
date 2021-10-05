using System;
using AutoMapper;
using Newtonsoft.Json.Linq;

namespace Iis.Utility.Automapper
{
    public class MaterialMetadataResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, string, JObject>
    {
        private const string Duration = "Duration";

        public JObject Resolve(TSource source, TDestination destination, string sourceMember, JObject destMember, ResolutionContext context)
        {
            if (sourceMember is null) return default;

            var metadata = JObject.Parse(sourceMember);
            if (!metadata.TryGetValue(Duration, out var duration)) return metadata;

            string durationString = duration.Value<string>();

            if (int.TryParse(durationString, out var totalSeconds))
            {
                metadata[Duration] = totalSeconds;
                return metadata;
            }

            if (TimeSpan.TryParse(durationString, out var parsedDuration))
            {
                metadata[Duration] = parsedDuration.TotalSeconds;
                return metadata;
            }

            return metadata;
        }
    }
}