using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.Materials;
using IIS.Core.GraphQL.Files;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;

namespace IIS.Core.GraphQL.Materials
{
    public class Query
    {

        [GraphQLType(typeof(MaterialCollection))]
        public async Task<(IEnumerable<Material> materials, int totalCount)> GetMaterials(
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] PaginationInput pagination,
            FilterInput filter,
            IEnumerable<Guid> nodeIds = null,
            IEnumerable<string> types = null)
        {
            var filterQuery = filter?.Suggestion ?? filter?.SearchQuery;

            var materialsResult = await materialProvider.GetMaterialsAsync(pagination.PageSize, pagination.Offset(), filterQuery, nodeIds, types);

            var materials = materialsResult.Materials.Select(m => mapper.Map<Material>(m)).ToList();

            return (materials, materialsResult.Count);
        }

        public async Task<Material> GetMaterial(
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid materialId)
        {
            var material = await materialProvider.GetMaterialAsync(materialId);
            var res = mapper.Map<Material>(material);
            return res;
        }

        public Task<IEnumerable<MaterialSignFull>> GetImportanceSigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("Importance").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }
        
        public Task<IEnumerable<MaterialSignFull>> GetReliabilitySigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("Reliability").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }
        
        public Task<IEnumerable<MaterialSignFull>> GetRelevanceSigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("Relevance").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }
        
        public Task<IEnumerable<MaterialSignFull>> GetCompletenessSigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("Completeness").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }
        
        public Task<IEnumerable<MaterialSignFull>> GetSourceReliabilitySigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            return Task.FromResult(materialProvider.GetMaterialSigns("SourceReliability").Select(ms => mapper.Map<MaterialSignFull>(ms)));
        }
    }
}
