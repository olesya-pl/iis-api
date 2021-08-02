using System;
using System.Collections.Generic;

using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;

namespace IIS.Services.Contracts.Materials
{
    public class MaterialsDto
    {
        private static readonly MaterialsDto _emptyResult;
        public IReadOnlyCollection<Material> Materials { get; private set;}
        public int Count { get; private set; }
        public Dictionary<Guid, SearchResultItem> Highlights { get; private set; }
        public Dictionary<string, AggregationItem> Aggregations { get; private set; }
        private MaterialsDto(){}
        static MaterialsDto()
        {
            _emptyResult = new MaterialsDto
            {
                Materials = Array.Empty<Material>(),
                Count = 0,
                Highlights = new Dictionary<Guid, SearchResultItem>(),
                Aggregations = new Dictionary<string, AggregationItem>()
            };
        }
        public static MaterialsDto Create(IReadOnlyCollection<Material> materials, 
            int count, 
            Dictionary<Guid, SearchResultItem> highlights,
            Dictionary<string, AggregationItem> aggregations)
        {
            return new MaterialsDto
            {
                Materials = materials ?? Array.Empty<Material>(),
                Count = count,
                Highlights = highlights,
                Aggregations = aggregations
            };
        }
        public static MaterialsDto Empty => _emptyResult;
    }
}