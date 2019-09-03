using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Materials
{
    public static class Extensions
    {
        public static Core.Materials.Material ToDomain(this MaterialInput input)
        {
            var result = new Core.Materials.Material(Guid.NewGuid(), JObject.FromObject(input.Metadata),
                JArray.FromObject(input.Data), input.Metadata.Type, input.Metadata.Source);
            if (input.FileId.HasValue)
                result.File = new IIS.Core.Files.FileInfo(input.FileId.Value);
            return result;
        }

        public static Material ToView(this IIS.Core.Materials.Material material)
        {
            return new Material
            {
                Id = material.Id,
                Children = material.Children.Select(ToView).ToList(),
                Metadata = material.Metadata.ToObject<Metadata>(),
                Data = material.Data.ToObject<IEnumerable<Data>>(),
                File = material.File == null ? null : ToView(material.File),
            };
        }

        private static Files.FileInfo ToView(this IIS.Core.Files.FileInfo fileInfo)
        {
            return new Files.FileInfo
            {
                Id = fileInfo.Id,
                Name = fileInfo.Name,
                ContentType = fileInfo.ContentType,
                IsTemporary = fileInfo.IsTemporary,
            };
        }
    }
}
