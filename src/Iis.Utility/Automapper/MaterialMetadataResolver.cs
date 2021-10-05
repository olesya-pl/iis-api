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
            int totalSeconds = default;
            var metadata = JObject.Parse(sourceMember);
            var hasDuration = metadata.TryGetValue(Duration, out var duration);
            var isDurationFieldInteger = hasDuration && int.TryParse(duration.Value<string>(), out totalSeconds);

            if (isDurationFieldInteger)
            {
                metadata[Duration] = totalSeconds;
                return metadata;
            }
            
            if (hasDuration)
            {
                metadata[Duration] = TimeSpan.TryParse(duration.Value<string>(), out var parsedDuration) 
                    ? parsedDuration.TotalSeconds 
                    : default;
            }
            else
            {
                metadata[Duration] = default;
            }
            
            return metadata;
        }
    }
}