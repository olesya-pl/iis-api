using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Iis.DataModel;
using IIS.Domain;
using Iis.Interfaces.Ontology;
using Microsoft.Extensions.Logging;

namespace Iis.Api.Export
{
    public class ExportService
    {
        private readonly ILogger<ExportService> _logger;
        private readonly IExtNodeService _nodeService;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly OntologyContext _context;

        public ExportService(
            ILogger<ExportService> logger,
            IExtNodeService nodeService,
            IOntologyProvider ontologyProvider,
            OntologyContext context)
        {
            _logger = logger;
            _nodeService = nodeService;
            _ontologyProvider = ontologyProvider;
            _context = context;
        }

        public async Task<byte[]> ExportPersonAsync(Guid personId)
        {
            //OntologyModel ontologyModel = await _ontologyProvider.GetOntologyAsync();
            //EntityType entityType = ontologyModel.EntityTypes.First(x => x.Name == "Person");
            //List<Guid> ids = _context.Nodes.Where(x => x.NodeTypeId == entityType.Id).Select(x => x.Id).ToList();
            //personId = ids.First();
            //_logger.LogDebug("Found {n} persons.", ids.Count);

            IExtNode node = await _nodeService.GetExtNodeByIdAsync(personId);
            List<string> list = node.Children.Select(x => x.NodeTypeTitle).ToList();

            await using var memoryStream = new MemoryStream();
            using (WordprocessingDocument doc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                mainPart.Document.Body = new Body();
                Body body = mainPart.Document.Body;

                foreach (string s1 in list)
                {
                    Paragraph para = body.AppendChild(new Paragraph());
                    Run run = para.AppendChild(new Run());
                    run.AppendChild(new Text(s1));
                }
            }

            return memoryStream.ToArray();
        }
    }
}