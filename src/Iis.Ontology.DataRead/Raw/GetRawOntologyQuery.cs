using System.Collections.Generic;
using MediatR;

namespace Iis.Ontology.DataRead.Raw
{
    public sealed class GetRawOntologyQuery : IRequest<List<OntologyItem>>
    {
    }
}
