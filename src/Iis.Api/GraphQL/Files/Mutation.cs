using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Api.Configuration;
using Iis.Domain.Materials;
using IIS.Core.Files;
using IIS.Core.Materials;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Files
{
    public class Mutation
    {
        public async Task<UploadResult> Upload([Service] UploadConfiguration uploadConfiguration,
            [Service] IFileService fileService,
            [Service] IMaterialService materialService,
            UploadInput input)
        {
            if (input.Name.EndsWith(".docx"))
            {
                return UploadDocx(uploadConfiguration, input);
            }
            else if (input.Name.EndsWith(".png"))
            {
                return await UploadPng(fileService, materialService, input);
            }
            else
            {
                return new UploadResult
                {
                    Success = false,
                    Message = "File not supported"
                };
            }
        }

        private static async Task<UploadResult> UploadPng(IFileService fileService, IMaterialService materialService, UploadInput input)
        {
            using (var stream = new MemoryStream(input.Content))
            {
                FileId fileSaveResult = await fileService
                .SaveFileAsync(stream, input.Name, "image/png", CancellationToken.None);
                if (fileSaveResult.IsDuplicate)
                {
                    return new UploadResult
                    {
                        Success = false,
                        Message = "File is duplicate"
                    };
                }

                var material = new Material
                {
                    Id = Guid.NewGuid(),
                    Title = input.Name,
                    Type = "image",
                    Source = "iis.api",
                    Metadata = JObject.FromObject(new
                    {
                        source = "iis.api",
                        type = "image"
                    }),
                    LoadData = new MaterialLoadData(),
                    File = new Iis.Domain.Materials.FileInfo(fileSaveResult.Id)
                };

                await materialService.SaveAsync(material);

                return new UploadResult { Success = true };
            }
        }

        private static UploadResult UploadDocx(UploadConfiguration uploadConfiguration, UploadInput input)
        {
            var byteArray = input.Content;
            var fileName = System.IO.Path.Combine(uploadConfiguration.DocxDirectory, input.Name);
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
                return new UploadResult
                {
                    Success = true
                };

            }
        }
    }
}
