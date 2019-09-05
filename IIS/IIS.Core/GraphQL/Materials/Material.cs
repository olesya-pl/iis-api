using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.Files;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.Materials;
using FileInfo = IIS.Core.GraphQL.Files.FileInfo;

namespace IIS.Core.GraphQL.Materials
{
    public class Material
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid Id { get; set; }
        [GraphQLIgnore] public Guid? FileId { get; set; }
        [GraphQLNonNullType] public Metadata Metadata { get; set; }
        public IEnumerable<Data> Data { get; set; }

        [GraphQLNonNullType]
        public async Task<IEnumerable<Material>> GetChildren([Service] IMaterialService materialService,
            [GraphQLNonNullType] PaginationInput pagination)
        {
            var materials = await materialService.GetMaterialsAsync(pagination.PageSize,
                pagination.Offset());
            return materials.Select(m => m.ToView()).ToList();
        }

        public async Task<FileInfo> GetFile([Service] IFileService fileService)
        {
            if (FileId == null) return null;
            var f = await fileService.GetFileAsync(FileId.Value);
            return f.ToView();
        }
    }
}
