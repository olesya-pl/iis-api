using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Api.Configuration;
using Iis.Domain.Materials;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.Materials;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Files
{
    public class Mutation
    {
        public async Task<IEnumerable<UploadResult>> Upload([Service] UploadConfiguration uploadConfiguration,
            [Service] IFileService fileService,
            [Service] IMaterialService materialService,
            IEnumerable<UploadInput> inputs)
        {
            var uploadTasks = new List<Task<UploadResult>>();
            foreach (var file in inputs)
            {
                uploadTasks.Add(UploadSingleFile(uploadConfiguration, fileService, materialService, file));
            }
            return await Task.WhenAll(uploadTasks);
        }

        private static async Task<UploadResult> UploadSingleFile(UploadConfiguration uploadConfiguration, IFileService fileService, IMaterialService materialService, UploadInput input)
        {
            try
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
                        Message = "Формат не підтримується"
                    };
                }
            }
            catch (Exception e)
            {
                return new UploadResult
                {
                    Success = false,
                    Message = e.Message
                };
            }

        }

        private static async Task<UploadResult> UploadPng(IFileService fileService, IMaterialService materialService, UploadInput input)
        {
            using (var stream = new MemoryStream(input.Content))
            {
                FileIdDto fileSaveResult = await fileService
                .SaveFileAsync(stream, input.Name, "image/png", CancellationToken.None);
                if (fileSaveResult.IsDuplicate)
                {
                    return new UploadResult
                    {
                        Success = false,
                        Message = "Даний файл вже завантажений до системи"
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
                    File = new FileDto(fileSaveResult.Id)
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
