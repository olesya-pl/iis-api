using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.Files;

namespace IIS.Core.GraphQL.Entities
{
    // This class represents Ontology.ScalarType.File output type
    public class Attachment
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid GetFileId([Parent] Guid fileId) => fileId;

        [GraphQLNonNullType] public async Task<string> GetTitle ([Parent] Guid fileId, [Service] IFileService fileService)
            => (await fileService.GetFileAsync(fileId)).Name;

        public async Task<string> GetType([Parent] Guid fileId, [Service] IFileService fileService)
            => (await fileService.GetFileAsync(fileId)).ContentType;

        [GraphQLNonNullType] public string GetUrl([Parent] Guid fileId)
        {
            // todo
            return $"http://dummy.net/{fileId}";
        }
    }

    public class AttachmentType : ObjectType<Guid>
    {
        protected override void Configure(IObjectTypeDescriptor<Guid> descriptor)
        {
            descriptor.Name("Attachment").BindFieldsExplicitly();
            descriptor.Include<Attachment>();
//            descriptor.Field("urlTest").Resolver(ctx =>
//            {
//                var httpContext = ctx.CustomProperty<HttpContext>("HttpContext");
//                var urlHelperFactory = ctx.Service<IUrlHelperFactory>();
//                var acc = ctx.Service<IActionContextAccessor>();
//                var helper = urlHelperFactory.GetUrlHelper(acc.ActionContext);
//                var url = helper.Action("Get", "Files", new {Id = Guid.NewGuid()}, acc.ActionContext.HttpContext.Request.Scheme);
//                return url;
//            });
        }
    }
}
