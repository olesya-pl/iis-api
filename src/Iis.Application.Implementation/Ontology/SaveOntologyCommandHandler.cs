using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Iis.Ontology.DataRead;
using Iis.Ontology.DataRead.Concept;
using Iis.Ontology.DataRead.Raw;
using MediatR;

namespace Iis.Application.Ontology
{
    public sealed class SaveOntologyCommandHandler : AsyncRequestHandler<SaveOntologyCommand>
    {
        private readonly IMediator _mediator;

        public SaveOntologyCommandHandler(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override async Task Handle(SaveOntologyCommand request, CancellationToken cancellationToken)
        {
            List<OntologyItem> ontologyItems = await _mediator.Send(new GetRawOntologyQuery(), cancellationToken);

            List<EntityConcept> entityConcepts = new List<EntityConcept>();
            foreach (OntologyItem entityItem in ontologyItems.Where(x => x.Kind == ItemKind.Entity))
            {
                EntityConcept entityConcept = await _mediator.Send(new GetEntityConceptQuery(entityItem.Id), cancellationToken);
                entityConcepts.Add(entityConcept);
            }

            string folder = Path.GetFullPath(Path.Combine("Seeding", request.Profile, "ontology"));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                foreach (string file in Directory.EnumerateFiles(folder, "*.json"))
                {
                    File.Delete(file);
                }
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            options.Converters.Add(new JsonStringEnumConverter());
            
            foreach (EntityConcept entityConcept in entityConcepts)
            {
                string fileName = Path.Combine(folder, entityConcept.Name);
                fileName = Path.ChangeExtension(fileName, ".json");

                await using FileStream fs = File.Create(fileName);
                await JsonSerializer.SerializeAsync(fs, entityConcept, options, cancellationToken);
            }
        }
    }
}
