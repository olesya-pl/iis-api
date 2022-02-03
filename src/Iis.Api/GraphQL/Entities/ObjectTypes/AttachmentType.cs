using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Iis.Services.Contracts.Interfaces;
using Iis.Utility;
using Microsoft.AspNetCore.Http;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    // This type represents Ontology.ScalarType.File output type
    public class AttachmentType : ObjectType<Guid>
    {
        protected override void Configure(IObjectTypeDescriptor<Guid> descriptor)
        {
            descriptor.Name("Attachment");
            descriptor.Field("FileId").Resolve(ctx => ctx.Parent<Guid>()).Type(typeof(NonNullType<IdType>));
            descriptor.Field("Title").Resolve(async ctx => {
                var fileService = ctx.Service<IFileService>();
                var fileId = ctx.Parent<Guid>();
                return (await fileService.GetFileAsync(fileId))?.Name ?? "File is not found";
            }).Type(typeof(NonNullType));
            descriptor.Field("Type").Resolve(async ctx => {
                var fileService = ctx.Service<IFileService>();
                var fileId = ctx.Parent<Guid>();
                return (await fileService.GetFileAsync(fileId))?.ContentType ?? "File is not found";
            }).Type(typeof(NonNullType));
            descriptor.Field("Url").Resolve(ctx => {
                var fileId = ctx.Parent<Guid>();
                return FileUrlGetter.GetFileUrl(fileId);
            }).Type(typeof(NonNullType));
        }

        public class Resolvers
        {
            [GraphQLType(typeof(NonNullType<IdType>))]
            public Guid GetFileId([Parent] Guid fileId)
            {
                return fileId;
            }

            [GraphQLNonNullType]
            public async Task<string> GetTitle([Parent] Guid fileId, [Service] IFileService fileService)
            {
                return (await fileService.GetFileAsync(fileId))?.Name ?? "File is not found";
            }

            public async Task<string> GetType([Parent] Guid fileId, [Service] IFileService fileService)
            {
                return (await fileService.GetFileAsync(fileId))?.ContentType ?? "File is not found";
            }

            [GraphQLNonNullType]
            public string GetUrl([Parent] Guid fileId)
            {
                return FileUrlGetter.GetFileUrl(fileId);
            }
        }
    }
}
