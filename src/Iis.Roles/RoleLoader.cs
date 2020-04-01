using AutoMapper;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Roles
{
    public class RoleLoader
    {
        OntologyContext _context;
        IMapper _mapper;
        private List<AccessGranted> _defaultAccessGrantedList;
        public RoleLoader(OntologyContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _defaultAccessGrantedList = _context.AccessObjects
                .Select(ag => new AccessGranted { Kind = ag.Kind, Category = ag.Category, Title = ag.Title }).ToList();
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            var roleEntities = await _context.Roles
                .Include(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .ToListAsync();

            var roles = roleEntities.Select(r => _mapper.Map<Role>(r)).ToList();
            foreach (var role in roles)
            {
                role.AccessGrantedItems.Add(_defaultAccessGrantedList);
            }
            return roles;
        }
    }
}
