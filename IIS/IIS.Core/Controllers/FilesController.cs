using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IIS.Core.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class FilesController : Controller
    {
        private IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost]
        public async Task<object> Post([Required] IFormFile file, CancellationToken token)
        {
            var id = await _fileService.SaveFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);
            var url = Url.Action("Get", "Files", new {Id = id}, Request.Scheme);
            return new {Id = id, Url = url};
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken token)
        {
            var fi = await _fileService.GetFileAsync(id);
            if (fi == null) return NotFound();
            return File(fi.Contents, fi.ContentType, fi.Name);
        }

        [HttpGet(nameof(FlushTemporary))]
        public async Task<IActionResult> FlushTemporary(CancellationToken token)
        {
            // TODO: Change solution to use either datetime provider or take time from argument
            await _fileService.FlushTemporaryFilesAsync(d => d < DateTime.Now);
            return Ok();
        }
    }
}
