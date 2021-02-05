using System.Threading;
using System.Threading.Tasks;
using Iis.MaterialLoader.Models;
using Iis.MaterialLoader.Services;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Iis.MaterialLoader.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IMaterialService _materialService;

        public FilesController(IFileService fileService, IMaterialService materialService)
        {
            _fileService = fileService;
            _materialService = materialService;
        }

        [HttpPost(nameof(CreateMaterial))]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> CreateMaterial([FromForm] string input,
            [FromForm] IFormFile file,
            CancellationToken token)
        {
            var material = JsonConvert.DeserializeObject<MaterialInput>(input);
            var fileSaveResult = await _fileService.SaveFileAsync(file.OpenReadStream(), file.FileName, file.ContentType, token);
            material.FileId = fileSaveResult.Id;
            if (fileSaveResult.IsDuplicate)
            {
                return Ok(new {IsDublicate = true});
            }

            var result = await _materialService.SaveAsync(material);

            return Ok(new {Id = result.Id, IsDublicate = false});
        }
    }
}