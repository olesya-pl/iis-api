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
            return materials.Select(Map);
        }

        public async Task<Material> GetMaterial([Service] IMaterialService materialService, [GraphQLType(typeof(NonNullType<IdType>))] Guid materialId)
        {
            var material = await materialService.GetMaterialAsync(materialId);
            return material == null ? null : Map(material);
        }

        private Material Map(IIS.Core.Materials.Material material)
        {
            return new Material
            {
                Id = material.Id,
                Children = material.Children.Select(Map).ToList(),
                Data = material.Data?.ToString(),
                File = material.File == null ? null : Map(material.File),
                Source = material.Source,
                Type = material.Type,
            };
        }

        private FileInfo Map(IIS.Core.Files.FileInfo fileInfo)
        {
            return new FileInfo
            {
                Id = fileInfo.Id,
                Name = fileInfo.Name,
                ContentType = fileInfo.ContentType,
                IsTemporary = fileInfo.IsTemporary,
            };
        }
    }
}
