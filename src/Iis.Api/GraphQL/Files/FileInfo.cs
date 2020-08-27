using System;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;

namespace IIS.Core.GraphQL.Files
{
    public class FileInfo
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public bool IsTemporary { get; set; }

        [GraphQLNonNullType]
        public string GetUrl([Service] IHttpContextAccessor contextAccessor)
        {
            var request = contextAccessor.HttpContext.Request;
            
            var scheme = request.IsHttps ? $"{request.Scheme}s" : request.Scheme; 

            return $"{scheme}://{request.Host.Value}/api/files/{Id}"; // todo: change hardcoded files api
        }
    }
}
