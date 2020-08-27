using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Api.Configuration;

namespace IIS.Core.GraphQL.Files
{
    public class Mutation
    {
        public async Task<UploadResult> Upload([Service] UploadConfiguration uploadConfiguration,
            UploadInput input)
        {
            string destinationDir;
            if (input.Name.EndsWith(".docx"))
            {
                destinationDir = uploadConfiguration.DocxDirectory;
            }
            else
            {
                return new UploadResult
                {
                    Success = false,
                    Message = "File not supported"
                };
            }

            try
            {
                var byteArray = input.Content;
                var fileName = System.IO.Path.Combine(destinationDir, input.Name);
                using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                fs.Write(byteArray, 0, byteArray.Length);
                return new UploadResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new UploadResult()
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
