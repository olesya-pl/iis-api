using System;

using Microsoft.AspNetCore.Http;

namespace Iis.Utility
{
    public class FileUrlGetter
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public FileUrlGetter(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string GetFileUrl(Guid fileId)
        {
            return GetFileUrl(fileId, _contextAccessor);
        }

        public static string GetFileUrl(Guid fileId, IHttpContextAccessor contextAccessor)
        {
            var request = contextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host.Value}/api/files/{fileId}";
        }

    }
}
