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
            [GraphQLNonNullType] PaginationInput pagination,
            [GraphQLType(typeof(IdType))] Guid? parentId = null,
            IEnumerable<Guid> nodeIds = null,
            IEnumerable<string> types = null)
        {
            var materials = await materialProvider.GetMaterialsAsync(pagination.PageSize,
                pagination.Offset(), parentId, nodeIds, types);
            return materials.Select(m => m.ToView());
        }

        public async Task<Material> GetMaterial([Service] IMaterialProvider materialProvider, [GraphQLType(typeof(NonNullType<IdType>))] Guid materialId)
        {
            var material = await materialProvider.GetMaterialAsync(materialId);
            return material?.ToView();
        }

        public async Task<IEnumerable<MaterialSignFull>> GetMaterialSigns([Service] IMaterialProvider materialProvider, [Service] IMapper mapper)
        {
            await Task.Yield();
            return materialProvider.MaterialSigns.Select(ms => mapper.Map<MaterialSignFull>(ms));
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
