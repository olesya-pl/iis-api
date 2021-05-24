using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iis.DbLayer.Repositories;
using Iis.Domain.Materials;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Newtonsoft.Json.Linq;
using MaterialLoadData = Iis.Domain.Materials.MaterialLoadData;

namespace Iis.Services
{
    public enum UploadResult
    {
        Success,
        Duplicate
    }
    
    public class UploadService
    {
        private readonly IFileService _fileService;

        public UploadService(IFileService fileService, IMaterialService materialService)
        {
            _fileService = fileService;
        }
        
        public async Task<UploadResult> UploadPng(UploadInput input, User user)
        {
            using (var stream = new MemoryStream(input.Content))
            {
                var fileSaveResult = await _fileService
                .SaveFileAsync(stream, input.Name, "image/png", CancellationToken.None);
                if (fileSaveResult.IsDuplicate)
                {
                    return UploadResult.Duplicate;
                }

                var loadData = new MaterialLoadData();

                if (user != null)
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

                return UploadResult.Success;
            }
        }
    }
}
