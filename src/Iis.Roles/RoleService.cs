using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Roles
{
    public class RoleService
    {
        OntologyContext _context;
        IMapper _mapper;
        private List<AccessGranted> _defaultAccessGrantedList;
        public RoleService(OntologyContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _defaultAccessGrantedList = _context.AccessObjects
                .Select(ag => new AccessGranted { 
                    Id = ag.Id, 
                    Kind = ag.Kind, 
                    Category = ag.Category, 
                    Title = ag.Title })
                .ToList();
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
                role.AccessGrantedItems.Merge(_defaultAccessGrantedList);
            }
            return roles;
        }

        public async Task<Role> GetRoleAsync(Guid id)
        {
            var roleEntity = await _context.Roles
                .Where(r => r.Id == id)
                .Include(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .SingleOrDefaultAsync();
            var role = _mapper.Map<Role>(roleEntity);
            role.AccessGrantedItems.Merge(_defaultAccessGrantedList);
            return role;
        }

        public User GetUser(Guid id)
        {
            var userEntity = _context.Users.Where(u => u.Id == id)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .SingleOrDefault();

            if (userEntity == null)
            {
                throw new ArgumentException($"User does not exist for id = {id}");
            }

            var roleEntityList = userEntity.UserRoles.Select(ur => ur.Role).ToList();
            var user = _mapper.Map<User>(userEntity);
            foreach (var roleEntity in roleEntityList)
            {
                if (roleEntity.IsAdmin)
                {
                    user.IsAdmin = true;
                }
                var accessGrantedList = roleEntity.RoleAccessEntities.Select(r => _mapper.Map<AccessGranted>(r));
                user.AccessGrantedItems.Merge(accessGrantedList);
            }

            return user;
        }

        public User GetUser(string userName, string passwordHash)
        {
            var userEntity = _context.Users.SingleOrDefault(x => x.Username == userName && x.PasswordHash == passwordHash);
            return userEntity == null ? null : GetUser(userEntity.Id);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            var roleEntity = _mapper.Map<RoleEntity>(role);
            _context.Roles.Add(roleEntity);
            await SaveRoleAccesses(role.Id, role.AccessGrantedItems);
            return await GetRoleAsync(roleEntity.Id);
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            var roleEntity = _mapper.Map<RoleEntity>(role);
            var roleAccessEntities = _mapper.Map<List<RoleAccessEntity>>(role.AccessGrantedItems);

            _context.Roles.Update(roleEntity);
            _context.RoleAccess.RemoveRange(_context.RoleAccess.Where(p => p.RoleId == roleEntity.Id));
            await _context.SaveChangesAsync();
            await SaveRoleAccesses(role.Id, role.AccessGrantedItems);
            return await GetRoleAsync(roleEntity.Id);
        }

        private async Task SaveRoleAccesses(Guid roleId, AccessGrantedList accesses)
        {
            //have to manage nested entities manually here
            //to avoid extra inserts
            var roleAccessEntities = _mapper.Map<List<RoleAccessEntity>>(accesses);
            foreach (var entity in roleAccessEntities)
            {
                entity.RoleId = roleId;
            }
            _context.RoleAccess.AddRange(roleAccessEntities);
            await _context.SaveChangesAsync();
        }
    }
}