using System;
using System.Collections.Generic;

using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;

namespace IIS.Core.Materials
{
    public class MaterialsDto
    {
        public IReadOnlyCollection<Material> Materials { get; private set;}
        public int Count { get; private set; }
        public Dictionary<Guid, SearchResultItem> Highlights { get; private set; }
        private MaterialsDto(){}
        public static MaterialsDto Create(IReadOnlyCollection<Material> materials, int count, Dictionary<Guid, SearchResultItem> highlights)
        {
            return new MaterialsDto
            {
                Materials = materials ?? Array.Empty<Material>(),
                Count = count,
                Highlights = highlights
            };
        }
    }
}