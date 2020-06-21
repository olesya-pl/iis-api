using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Domain.Materials;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Materials
{
    public static class Extensions
    {        
        public static Files.FileInfo ToView(this FileInfo fileInfo)
        {
            return new Files.FileInfo
            {
                Id = fileInfo.Id,
                Name = fileInfo.Name,
                ContentType = fileInfo.ContentType,
                IsTemporary = fileInfo.IsTemporary,
            };
        }
    }
}
