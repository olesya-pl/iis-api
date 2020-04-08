using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Roles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Roles
{
    public class RoleSaver
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private readonly RoleLoader _roleLoader;

        public RoleSaver(OntologyContext ontologyContext,
            RoleLoader roleLoader,
            IMapper mapper)
        {
            _context = ontologyContext;
            _mapper = mapper;
            _roleLoader = roleLoader;
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            var roleEntity = _mapper.Map<RoleEntity>(role);
            _context.Roles.Add(roleEntity);
            //have to manage nested entities manually here
            //to avoid extra inserts
            var roleAccessEntities = _mapper.Map<List<RoleAccessEntity>>(role.AccessGrantedItems);
            foreach (var entity in roleAccessEntities)
            {
                entity.RoleId = role.Id;
            }
            _context.AddRange(roleAccessEntities);
            await _context.SaveChangesAsync();
            return await _roleLoader.GetRoleAsync(roleEntity.Id);
        }
    }
}
