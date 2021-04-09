using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iis.DataModel;
using Iis.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Iis.Services
{
    public class AccessObjectService : IAccessObjectService
    {
        private readonly IMapper _mapper;
        OntologyContext _context;

        public AccessObjectService(
            IMapper mapper,
            OntologyContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public IReadOnlyCollection<AccessGranted> GetAccesses()
        {
            return _context.AccessObjects
                .AsNoTracking()
                .Select(ag => _mapper.Map<AccessGranted>(ag))
                .ToList();
        }
    }
}
