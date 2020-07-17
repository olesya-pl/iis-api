using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using IIS.Core.Files;
using IIS.Core.GraphQL.Materials;
using IIS.Core.Materials;
using IIS.Core.Materials.FeatureProcessors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Controllers
{
    // TODO: protect files with authentication check
    [Route("api/{controller}")]
    [ApiController]
    public class FilesController : Controller
    {
        private IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly IMaterialProvider _materialProvider;
        private readonly IMaterialService _materialService;
        private readonly IFeatureProcessorFactory _featureProcessorFactory;

        public FilesController(IFileService fileService,
            IMapper mapper,
            IMaterialProvider materialProvider,
            IMaterialService materialService,
            IFeatureProcessorFactory featureProcessorFactory)
        {
            _fileService = fileService;
            _mapper = mapper;
            _materialProvider = materialProvider;
            _materialService = materialService;
            _featureProcessorFactory = featureProcessorFactory;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<CreateMaterialResponse> Post([FromForm] string input,
            [FromForm] IFormFile file,
            CancellationToken token)
        {
            var material = JsonConvert.DeserializeObject<MaterialInput>(input);
            FileId fileSaveResult = await _fileService
                .SaveFileAsync(file.OpenReadStream(), file.FileName, file.ContentType, token);
            material.FileId = fileSaveResult.Id;
            if (fileSaveResult.IsDuplicate)
            {
                return new CreateMaterialResponse() { IsDublicate = false };
            }

            Iis.Domain.Materials.Material inputMaterial = _mapper.Map<Iis.Domain.Materials.Material>(material);
            inputMaterial.Reliability = _materialProvider.GetMaterialSign(material.ReliabilityText);
            inputMaterial.SourceReliability = _materialProvider.GetMaterialSign(material.SourceReliabilityText);
            inputMaterial.LoadData = _mapper.Map<Iis.Domain.Materials.MaterialLoadData>(material);
            inputMaterial.Metadata = await _featureProcessorFactory
                .GetInstance(inputMaterial.Source, inputMaterial.Type).ProcessMetadata(inputMaterial.Metadata);
            await _materialService.SaveAsync(inputMaterial);

            return new CreateMaterialResponse { Id = inputMaterial.Id, IsDublicate = true };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken token)
        {
            var fi = await _fileService.GetFileAsync(id);
            if (fi == null) return NotFound();
            var cd = new System.Net.Mime.ContentDisposition // Always inline file
            {
                FileName = Uri.EscapeDataString(fi.Name),
                Inline = true
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(fi.Contents, fi.ContentType);
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
