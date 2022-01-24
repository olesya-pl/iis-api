using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Elastic.Entities;
using Iis.Messages.Materials;
using Newtonsoft.Json;

namespace Iis.Services.Mappers.Materials
{
    internal static class MaterialCriteriasMessageMapper
    {
        public static MaterialProcessingCriteriasEventMessage ToProcessingCriteriasEvent(this IEnumerable<MaterialDocument> materials)
        {
            return new MaterialProcessingCriteriasEventMessage
            {
                Materials = materials.Select(_ => _.Map()).ToArray()
            };
        }

        private static Material Map(this MaterialDocument materialDocument)
        {
            return new Material
            {
                Id = materialDocument.Id,
                Metadata = materialDocument.Metadata.ToString(Formatting.None),
                RelatedObjectCollection = materialDocument.RelatedObjectCollection.Select(_ => new Messages.Materials.RelatedObjectOfStudy
                {
                    Id = _.Id,
                    Importance = Enum.TryParse<Importance>(_.Importance, true, out var importance)
                    ? importance
                    : default(Importance?)
                }).ToArray()
            };
        }
    }
}