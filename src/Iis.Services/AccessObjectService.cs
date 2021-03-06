using System.Collections.Generic;
using System.Linq;
using Iis.DataModel;
using Iis.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Iis.Services
{
    public class AccessObjectService : IAccessObjectService
    {
        OntologyContext _context;

        public AccessObjectService(OntologyContext context)
        {
            _context = context;
        }

        public IReadOnlyCollection<AccessGranted> GetAccesses()
        {
            return _context.AccessObjects
                .AsNoTracking()
                .Select(ag => new AccessGranted
                {
                    Id = ag.Id,
                    Kind = ag.Kind,
                    Category = ag.Category,
                    Title = ag.Title
                })
                .ToList();
        }
    }
}
