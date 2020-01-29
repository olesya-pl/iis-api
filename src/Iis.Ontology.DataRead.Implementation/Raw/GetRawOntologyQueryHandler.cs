using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Iis.DataModel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Iis.Ontology.DataRead.Raw
{
    public class GetRawOntologyQueryHandler : IRequestHandler<GetRawOntologyQuery, List<OntologyItem>>
    {
        private readonly IMapper _mapper;
        private readonly OntologyContext _context;

        public GetRawOntologyQueryHandler(
            IMapper mapper,
            OntologyContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<List<OntologyItem>> Handle(GetRawOntologyQuery request, CancellationToken cancellationToken)
        {
            var query = from nt in _context.NodeTypes.AsNoTracking()
                select nt;

            return await query
                .ProjectTo<OntologyItem>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
