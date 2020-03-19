using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Files;
using IIS.Core.Materials;

namespace IIS.Core.GraphQL.Materials
{
    public class Query
    {

        [GraphQLType(typeof(MaterialWrapperType))]
        public async Task<IEnumerable<Material>> GetMaterials(
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper,
            [GraphQLNonNullType] PaginationInput pagination,
            [GraphQLType(typeof(IdType))] Guid? parentId = null,
            IEnumerable<Guid> nodeIds = null,
            IEnumerable<string> types = null)
        {
            var materials = await materialProvider.GetMaterialsAsync(pagination.PageSize,
                pagination.Offset(), parentId, nodeIds, types);
            var result = materials.Select(m => mapper.Map<Material>(m)).ToList();
            return result;
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

    public class MaterialWrapperType : ObjectType<IEnumerable<Material>>
    {
        protected override void Configure(IObjectTypeDescriptor<IEnumerable<Material>> descriptor)
        {
            descriptor.Name("MaterialWrapper");
            descriptor.BindFieldsExplicitly().Include<Resolvers>();
        }

        public class Resolvers
        {
            public int Count => 0;
            [GraphQLType(typeof(NonNullType<ListType<NonNullType<ObjectType<Material>>>>))]
            public IEnumerable<Material> Items([Parent] IEnumerable<Material> parent) => parent;
        }
    }
}
