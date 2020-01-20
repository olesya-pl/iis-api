using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Domain.Materials;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Materials
{
    public static class Extensions
    {
        public static Iis.Domain.Materials.Material ToDomain(this MaterialInput input)
        {
            Iis.Domain.Materials.Material result = new Iis.Domain.Materials.Material(
                Guid.NewGuid(),
                JObject.FromObject(input.Metadata),
                input.Data == null ? null : JArray.FromObject(input.Data),
                input.Metadata.Type,
                input.Metadata.Source);

            if (input.FileId != null)
            {
                result.File = new FileInfo((Guid)input.FileId);
            }

            return result;
        }

        public static Material ToView(this Iis.Domain.Materials.Material material)
        {
            return new Material
            {
                Id = material.Id,
                Metadata = material.Metadata.ToObject<Metadata>(),
                Data = material.Data?.ToObject<IEnumerable<Data>>(),
                FileId = material.File?.Id,
                Transcriptions = material.Infos.Select(e => e.Data)
            };
        }

        public static Files.FileInfo ToView(this FileInfo fileInfo)
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