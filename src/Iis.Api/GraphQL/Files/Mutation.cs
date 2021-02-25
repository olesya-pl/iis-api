using System;
using System.Collections.Generic;
using System.IO;
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
        private static readonly UploadResult DuplicatedUploadResult = new UploadResult()
        {
            Success = false,
            Message = "Даний файл вже завантажений до системи"
        };
        
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
                    return await UploadFileAsync(fileService, uploadConfiguration.DocxDirectory, input);
                }
                else if (input.Name.EndsWith(".pdf"))
                {
                    return await UploadFileAsync(fileService, uploadConfiguration.PdfDirectory, input);
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
                    return DuplicatedUploadResult;
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
                    File = new FileDto(fileSaveResult.Id),
                    CreatedDate = DateTime.UtcNow,
                    AccessLevel = input.AccessLevel
                };

                await materialService.SaveAsync(material);

                return new UploadResult { Success = true };
            }
        }

        private static async Task<UploadResult> UploadFileAsync(IFileService fileService, string directory, UploadInput input)
        {
            const string dataFileExtension = ".dat";
            const string accessLevelLinePrefix = "Рівень доступу: ";

            var result = await fileService.IsDuplicatedAsync(input.Content);
            if (result.IsDuplicate)
                return DuplicatedUploadResult;
            
            var byteArray = input.Content;
            var fileName = System.IO.Path.Combine(directory, input.Name);
            var dataFileName = $"{System.IO.Path.GetFileNameWithoutExtension(input.Name)}{dataFileExtension}";
            var fullDataName = System.IO.Path.Combine(directory, dataFileName);
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(byteArray, 0, byteArray.Length);                
            }
            using (var sw = File.CreateText(fullDataName))
            {
                await sw.WriteLineAsync($"{accessLevelLinePrefix} {(int)input.AccessLevel}");                
            }
            return new UploadResult
            {
                Success = true
            };
        }
    }
}
