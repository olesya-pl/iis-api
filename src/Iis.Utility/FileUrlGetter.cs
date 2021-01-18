using System;
using Microsoft.AspNetCore.Http;

namespace Iis.Utility
{
    public class FileUrlGetter
    {
        public static string GetFileUrl(Guid fileId)
        {
            return $"api/files/{fileId}";
        }
    }
}