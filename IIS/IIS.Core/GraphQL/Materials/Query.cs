using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
            [Service] IMaterialService materialService,
            [GraphQLNonNullType] PaginationInput pagination,
            [GraphQLType(typeof(IdType))] Guid? parentId = null,
            IEnumerable<Guid> nodeIds = null,
            IEnumerable<string> types = null)
        {
            var materials = await materialService.GetMaterialsAsync(pagination.PageSize,
                pagination.Offset(), parentId, nodeIds, types);
            return materials.Select(m => m.ToView());
        }

        public async Task<Material> GetMaterial([Service] IMaterialService materialService, [GraphQLType(typeof(NonNullType<IdType>))] Guid materialId)
        {
            var material = await materialService.GetMaterialAsync(materialId);
            return material?.ToView();
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
