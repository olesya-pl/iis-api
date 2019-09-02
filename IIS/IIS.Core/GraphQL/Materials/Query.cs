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

        [GraphQLType(typeof(NonNullType<CollectionType<Material>>))]
        public async Task<IEnumerable<Material>> GetMaterials([Service] IMaterialService materialService)
        {
            var materials = await materialService.GetMaterialsAsync();
            return materials.Select(m => m.ToView());
        }

        public async Task<Material> GetMaterial([Service] IMaterialService materialService, [GraphQLType(typeof(NonNullType<IdType>))] Guid materialId)
        {
            var material = await materialService.GetMaterialAsync(materialId);
            return material?.ToView();
        }


    }
}
