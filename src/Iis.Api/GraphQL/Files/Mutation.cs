using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using HotChocolate;
using HotChocolate.Resolvers;
using IIS.Core.Materials;
using Iis.Api.Configuration;
using Iis.Domain.Materials;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.Files
{
    public class Mutation
    {
        const string DataFileExtension = ".dat";
        const string AccessLevelPropertyName = "AccessLevel";
        const string LoadedByPropertyName = "LoadedBy";
        private static readonly UploadResult DuplicatedUploadResult = new UploadResult()
        {
            Success = false,
            Message = "Даний файл вже завантажений до системи"
        };

        public async Task<IEnumerable<UploadResult>> Upload([Service] UploadConfiguration uploadConfiguration,
            IResolverContext context,
            [Service] IFileService fileService,
            [Service] IMaterialService materialService,
            IEnumerable<UploadInput> inputs)
        {
            var uploadTasks = new List<Task<UploadResult>>();

            var tokenPayload = context.ContextData[TokenPayload.TokenPropertyName] as TokenPayload;

            foreach (var file in inputs)
            {
                uploadTasks.Add(UploadSingleFile(uploadConfiguration, fileService, materialService, file, tokenPayload.User));
            }
            return await Task.WhenAll(uploadTasks);
        }

        private static async Task<UploadResult> UploadSingleFile(UploadConfiguration uploadConfiguration, IFileService fileService, IMaterialService materialService, UploadInput input, User user)
        {
            try
            {
                if (input.Name.EndsWith(".docx"))
                {
                    return await UploadFileAsync(fileService, uploadConfiguration.DocxDirectory, input, user);
                }
                else if (input.Name.EndsWith(".pdf"))
                {
                    return await UploadFileAsync(fileService, uploadConfiguration.PdfDirectory, input, user);
                }
                else if (input.Name.EndsWith(".mp4"))
                {
                    return await UploadFileAsync(fileService, uploadConfiguration.VideoDirectory, input, user);
                }
                else if (input.Name.EndsWith(".png"))
                {
                    return await UploadPng(fileService, materialService, input, user);
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

        private static async Task<UploadResult> UploadPng(IFileService fileService, IMaterialService materialService, UploadInput input, User user)
        {
            using (var stream = new MemoryStream(input.Content))
            {
                FileIdDto fileSaveResult = await fileService
                .SaveFileAsync(stream, input.Name, "image/png", CancellationToken.None);
                if (fileSaveResult.IsDuplicate)
                {
                    return DuplicatedUploadResult;
                }

                var loadData = new MaterialLoadData();

                if(user != null)
                {
                    loadData.LoadedBy = $"{user.LastName} {user.FirstName} {user.Patronymic}";
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
                    LoadData = loadData,
                    File = new FileDto(fileSaveResult.Id),
                    CreatedDate = DateTime.UtcNow,
                    AccessLevel = input.AccessLevel,
                };

                await materialService.SaveAsync(material);

                return new UploadResult { Success = true };
            }
        }

        private static async Task<UploadResult> UploadFileAsync(IFileService fileService, string directory, UploadInput input, User user)
        {
            var result = await fileService.IsDuplicatedAsync(input.Content);
            if (result.IsDuplicate)
                return DuplicatedUploadResult;

            var byteArray = input.Content;
            var fileName = System.IO.Path.Combine(directory, input.Name);
            var dataFileName = $"{System.IO.Path.GetFileNameWithoutExtension(input.Name)}{DataFileExtension}";
            var fullDataName = System.IO.Path.Combine(directory, dataFileName);
            var userName = user is null ? string.Empty : $"{user.LastName} {user.FirstName} {user.Patronymic}";
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(byteArray, 0, byteArray.Length);
            }
            using (var sw = File.CreateText(fullDataName))
            {
                await sw.WriteLineAsync($"{AccessLevelPropertyName} {input.AccessLevel}");                
                sw.WriteLine($"{LoadedByPropertyName}: {userName}");
            }
            return new UploadResult
            {
                Success = true
            };
        }
    }
}
