using System;
using System.IO;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Interfaces;
using IIS.Core;
using IIS.Core.GraphQL.Files;
using IIS.Core.Materials;

namespace Iis.Api.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class UploadController : Controller
    {
        private const string ImageTypePropertyName = "image";
        private const string ImageSourcePropertyName = "iis.api";
        const string DataFileExtension = ".dat";
        const string AccessLevelPropertyName = "AccessLevel";
        const string LoadedByPropertyName = "LoadedBy";
        private readonly IFileService _fileService;
        private readonly IMaterialService _materialService;
        private readonly UploadConfiguration _uploadConfiguration;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UploadController> _logger;
        private readonly IAuthTokenService _authTokenService;

        public UploadController(IFileService fileService,
            IMaterialService materialService,
            UploadConfiguration uploadConfiguration,
            IUserService userService,
            IConfiguration configuration,
            ILogger<UploadController> logger,
            IAuthTokenService authTokenService)
        {
            _fileService = fileService;
            _materialService = materialService;
            _uploadConfiguration = uploadConfiguration;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
            _authTokenService = authTokenService;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<UploadResult> Post([FromForm] IFormFile file, [FromForm]string fileInfo,  CancellationToken ct)
        {
            var user = await _authTokenService.GetHttpRequestUserAsync(Request);

            var input = JsonConvert.DeserializeObject<UploadInput>(fileInfo);

            return await UploadSingleFile(file.OpenReadStream(), input, user);
        }

        private async Task<UploadResult> UploadSingleFile(Stream fileStream, UploadInput input, User user)
        {
            try
            {
                return Path.GetExtension(input.Name) switch
                {
                    var extension when extension.Equals(".docx", StringComparison.OrdinalIgnoreCase) => await UploadFileAsync(_uploadConfiguration.DocxDirectory, fileStream, input, user),
                    var extension when extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)  => await UploadFileAsync(_uploadConfiguration.PdfDirectory, fileStream, input, user),
                    var extension when extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)  => await UploadFileAsync(_uploadConfiguration.VideoDirectory, fileStream, input, user),
                    var extension when extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase)  => await UploadFileAsync(_uploadConfiguration.AudioDirectory, fileStream, input, user),
                    var extension when extension.Equals(".png", StringComparison.OrdinalIgnoreCase)  => await UploadPngAsync(input, fileStream, user),
                    _ => UploadResult.Unsupported
                };
            }
            catch (Exception e)
            {
                return UploadResult.Error(e.Message);
            }
        }

        private async Task<UploadResult> UploadPngAsync(UploadInput input, Stream fileStream, User user)
        {
            var fileSaveResult = await _fileService.SaveFileAsync(fileStream, input.Name, "image/png", CancellationToken.None);

            if (fileSaveResult.IsDuplicate)
            {
                return UploadResult.Duplicated;
            }

            var loadData = new MaterialLoadData
            {
                LoadedBy = user.FullUserName
            };

            var material = new Material
            {
                Id = Guid.NewGuid(),
                Title = input.Name,
                Type = ImageTypePropertyName,
                Source = ImageSourcePropertyName,
                Metadata = JObject.FromObject(new
                {
                    source = ImageSourcePropertyName,
                    type = ImageTypePropertyName
                }),
                LoadData = loadData,
                File = new Iis.Domain.Materials.File(fileSaveResult.Id),
                CreatedDate = DateTime.UtcNow,
                AccessLevel = input.AccessLevel,
            };

            await _materialService.SaveAsync(material);

            return UploadResult.Ok;
        }

        private async Task<UploadResult> UploadFileAsync(string directory, Stream fileStream, UploadInput input, User user)
        {
            _logger.LogInformation("UploadController. Upload started. File {fileName}", input.Name);

            var result = await _fileService.IsDuplicatedAsync(fileStream);

            if (result.IsDuplicate) return UploadResult.Duplicated;

            var fileName = Path.Combine(directory, input.Name);
            var dataFileName = $"{Path.GetFileNameWithoutExtension(input.Name)}{DataFileExtension}";
            var fullDataName = Path.Combine(directory, dataFileName);
            var userName = user?.FullUserName ?? string.Empty;

            using (var sw = System.IO.File.CreateText(fullDataName))
            {
                var accessLine = $"{AccessLevelPropertyName}: {input.AccessLevel}";
                var loadedByLine = $"{LoadedByPropertyName}: {userName}";
                await sw.WriteLineAsync(accessLine);
                await sw.WriteLineAsync(loadedByLine);
                _logger.LogInformation("UploadController. Generating data file for {fileName}. Access level {accessLine} Loaded by {loadedByLine}", input.Name, accessLine, loadedByLine);
            }

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                _logger.LogInformation("UploadController. Copying file {fileName} to {directory}", input.Name, directory);
                await fileStream.CopyToAsync(fs);
            }

            return UploadResult.Ok;
        }
    }
}