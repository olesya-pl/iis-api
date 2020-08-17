using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Iis.Services
{
    public class RoleService : IRoleService
    {
        OntologyContext _context;
        IMapper _mapper;
        private List<AccessGranted> _defaultAccessGrantedList;
        public RoleService(OntologyContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            //TODO: should we get AccessObjects from db each time we use RoleService(RoleService is registered in DI as transient)
            _defaultAccessGrantedList = _context.AccessObjects
                .Select(ag => new AccessGranted
                {
                    Id = ag.Id,
                    Kind = ag.Kind,
                    Category = ag.Category,
                    Title = ag.Title
                })
                .ToList();
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            var roleEntities = await _context.Roles
                .Include(r => r.RoleGroups)
                .Include(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .ToListAsync();

            return roleEntities.Select(ToRole).ToList();
        }

        public async Task<Role> GetRoleAsync(Guid id)
        {
            var roleEntity = await _context.Roles
                .Include(r => r.RoleGroups)
                .Include(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .SingleOrDefaultAsync(r => r.Id == id);

           return ToRole(roleEntity);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            var roleEntity = _mapper.Map<RoleEntity>(role);
            roleEntity.RoleGroups = role.ActiveDirectoryGroupIds.Select(g => new RoleActiveDirectoryGroupEntity()
            {
                GroupId = g,
                RoleId = role.Id
            }).ToList();

            _context.Roles.Add(roleEntity);
            await SaveRoleAccesses(role.Id, role.AccessGrantedItems);
            return ToRole(roleEntity);
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            var updatedRoleEntity = _mapper.Map<RoleEntity>(role);
            var roleEntity = await _context.Roles
                .Include(r => r.RoleGroups)
                .Include(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .SingleOrDefaultAsync(x => x.Id == role.Id);
            
            _context.Entry(roleEntity).CurrentValues.SetValues(updatedRoleEntity);
            UpdateActiveDirectoryGroups(roleEntity, role.ActiveDirectoryGroupIds);
            UpdateRoleAccesses(roleEntity, role.AccessGrantedItems);

            await _context.SaveChangesAsync();
            return ToRole(roleEntity);
        }

        private void UpdateRoleAccesses(RoleEntity entity, AccessGrantedList accesses)
        {
            if (accesses == null || !accesses.Any())
                return;

            var updatedAccessEntities = accesses.Select(a =>
            {
                var access = _mapper.Map<RoleAccessEntity>(a);
                access.RoleId = entity.Id;
                return access;
            });

            foreach (var accessEntity in entity.RoleAccessEntities)
            {
                var updatedAccessEntity = updatedAccessEntities.Single(a => a.AccessObjectId == accessEntity.AccessObjectId);
                updatedAccessEntity.Id = accessEntity.Id;

                _context.Entry(accessEntity).CurrentValues.SetValues(updatedAccessEntity);
            }
        }

        private void UpdateActiveDirectoryGroups(RoleEntity roleEntity, List<Guid> activeDirectoryGroupIds)
        {
            if (activeDirectoryGroupIds == null)
                return;

            foreach (var groupId in activeDirectoryGroupIds.Where(groupId => roleEntity.RoleGroups.All(x => x.GroupId != groupId)))
            {
                roleEntity.RoleGroups.Add(new RoleActiveDirectoryGroupEntity()
                {
                    GroupId = groupId
                });
            }

            var groups = roleEntity.RoleGroups.ToArray();
            foreach (var roleGroup in groups.Where(roleGroup => activeDirectoryGroupIds.All(x => x != roleGroup.GroupId)))
            {
                roleEntity.RoleGroups.Remove(roleGroup);
            }
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

        private Role ToRole(RoleEntity entity)
        {
            var role = _mapper.Map<Role>(entity);
            role.AccessGrantedItems.Merge(_defaultAccessGrantedList);

            return role;
        }
    }
}