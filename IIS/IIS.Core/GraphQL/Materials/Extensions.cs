using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Materials
{
    public static class Extensions
    {
        public static Core.Materials.Material ToDomain(this MaterialInput input)
        {
            var result = new Core.Materials.Material(Guid.NewGuid(), JObject.Parse(input.Data), input.Type, input.Source);
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
                Data = material.Data?.ToString(),
                File = material.File == null ? null : ToView(material.File),
                Source = material.Source,
                Type = material.Type,
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
