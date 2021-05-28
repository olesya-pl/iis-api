using System;
using System.IO;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Iis.Api.Configuration;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Interfaces;
using IIS.Core;
using IIS.Core.GraphQL.Files;
using IIS.Core.Materials;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.Api.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class UploadController : Controller
    {
        const string DataFileExtension = ".dat";
        const string AccessLevelPropertyName = "AccessLevel";
        const string LoadedByPropertyName = "LoadedBy";
        private static readonly UploadResult DuplicatedUploadResult = new UploadResult()
        {
            Success = false,
            Message = "Даний файл вже завантажений до системи"
        };

        private readonly IFileService _fileService;
        private readonly IMaterialService _materialService;
        private readonly UploadConfiguration _uploadConfiguration;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UploadController(IFileService fileService,
            IMaterialService materialService,
            UploadConfiguration uploadConfiguration,
            IUserService userService,
            IConfiguration configuration)
        {
            _fileService = fileService;
            _materialService = materialService;
            _uploadConfiguration = uploadConfiguration;
            _userService = userService;
            _configuration = configuration;
        }
        
        [HttpPost]
        [DisableRequestSizeLimit]
        public Task<UploadResult> Post([FromForm] IFormFile file, [FromForm]string fileInfo,  CancellationToken ct)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var token))
                throw new AuthenticationException("Requires \"Authorization\" header to contain a token");
            var tokenPayload = TokenHelper.ValidateToken(token, _configuration, _userService);

            var input = JsonConvert.DeserializeObject<UploadInput>(fileInfo);

            return UploadSingleFile(file.OpenReadStream(), input, tokenPayload.User);
        }

        private async Task<UploadResult> UploadSingleFile(Stream fileStream, UploadInput input, User user)
        {
            try
            {
                if (input.Name.EndsWith(".docx"))
                {
                    return await UploadFileAsync(_uploadConfiguration.DocxDirectory, fileStream, input, user);
                }
                else if (input.Name.EndsWith(".pdf"))
                {
                    return await UploadFileAsync(_uploadConfiguration.PdfDirectory, fileStream, input, user);
                }
                else if (input.Name.EndsWith(".mp4"))
                {
                    return await UploadFileAsync(_uploadConfiguration.VideoDirectory, fileStream, input, user);
                }
                else if (input.Name.EndsWith(".mp3"))
                {
                    return await UploadFileAsync(_uploadConfiguration.AudioDirectory, fileStream, input, user);
                }
                else if (input.Name.EndsWith(".png"))
                {
                    return await UploadPng(input, fileStream, user);
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

        private async Task<UploadResult> UploadPng(UploadInput input, Stream fileStream, User user)
        {
            var fileSaveResult = await _fileService
                .SaveFileAsync(fileStream, input.Name, "image/png", CancellationToken.None);
            if (fileSaveResult.IsDuplicate)
            {
                return DuplicatedUploadResult;
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
                File = new Iis.Domain.Materials.File(fileSaveResult.Id),
                CreatedDate = DateTime.UtcNow,
                AccessLevel = input.AccessLevel,
            };

            await _materialService.SaveAsync(material);

            return new UploadResult { Success = true };
        }

        private async Task<UploadResult> UploadFileAsync(string directory, Stream fileStream, UploadInput input, User user)
        {
            var result = await _fileService.IsDuplicatedAsync(fileStream);
            if (result.IsDuplicate)
                return DuplicatedUploadResult;

            var fileName = Path.Combine(directory, input.Name);
            var dataFileName = $"{Path.GetFileNameWithoutExtension(input.Name)}{DataFileExtension}";
            var fullDataName = Path.Combine(directory, dataFileName);
            var userName = user is null ? string.Empty : $"{user.LastName} {user.FirstName} {user.Patronymic}";
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fs);
            }
            using (var sw = System.IO.File.CreateText(fullDataName))
            {
                await sw.WriteLineAsync($"{AccessLevelPropertyName}: {input.AccessLevel}");
                sw.WriteLine($"{LoadedByPropertyName}: {userName}");
            }
            return new UploadResult
            {
                Success = true
            };
        }
    }
}
