using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Domain.Users;
using Iis.Interfaces.Roles;
using Microsoft.EntityFrameworkCore;

namespace Iis.Services
{
    public class RoleService : IRoleService
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        public RoleService(OntologyContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;            
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            var roleEntities = await _context.Roles
                .AsNoTracking()
                .Include(r => r.RoleGroups)
                .Include(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .ToListAsync();

            return roleEntities.Select(ToRole).ToList();
        }

        public async Task<Role> GetRoleAsync(Guid id)
        {
            RoleEntity roleEntity = await GetRoleEntityByIdAsync(id);
            var accessObjects = _context.AccessObjects.AsNoTracking().ToList();
            return ToRole(roleEntity, accessObjects);
        }        

        public async Task<(Role Role, bool AlreadyExists)> CreateRoleAsync(Role role)
        {
            var roleExists = await _context.Roles.AnyAsync(_ => _.Name == role.Name);
            if (roleExists) return (null, true);
            
            var roleEntity = _mapper.Map<RoleEntity>(role);
            roleEntity.RoleGroups = role.ActiveDirectoryGroupIds.Select(g => new RoleActiveDirectoryGroupEntity()
            {
                GroupId = g,
                RoleId = role.Id
            }).ToList();

            _context.Roles.Add(roleEntity);
            PrepareRoleAccesses(role.AccessGrantedItems);            

            SaveRoleAccesses(role.Id, role.AccessGrantedItems);            
            await SaveCorresponsingTabs(role);
            await _context.SaveChangesAsync();
            roleEntity = await GetRoleEntityByIdAsync(role.Id);
            return (ToRole(roleEntity), false);
        }

        private void PrepareRoleAccesses(AccessGrantedList accessGrantedItems)
        {
            var entities = accessGrantedItems.Where(p => p.Category == AccessCategory.Entity).ToList();
            VerifyAccessesAllowed(entities);
            VerifyAccessesDependencies(entities);
        }

        private static void VerifyAccessesAllowed(List<AccessGranted> entities)
        {
            foreach (var entity in entities)
            {
                entity.SearchGranted = entity.SearchGranted && entity.SearchAllowed;
                entity.CreateGranted = entity.CreateGranted && entity.CreateAllowed;
                entity.UpdateGranted = entity.UpdateGranted && entity.UpdateAllowed;
                entity.CommentingGranted = entity.CommentingGranted && entity.CommentingAllowed;
                entity.AccessLevelUpdateGranted = entity.AccessLevelUpdateGranted && entity.AccessLevelUpdateAllowed;
            }
        }

        private static void VerifyAccessesDependencies(List<AccessGranted> entities)
        {
            var entitiesWithIncorrectPermissions = entities.Where(p => p.Category == AccessCategory.Entity
                            && !p.ReadGranted
                            && (p.SearchGranted || p.CreateGranted || p.UpdateGranted
                                    || p.CommentingGranted || p.AccessLevelUpdateGranted));

            foreach (var entity in entitiesWithIncorrectPermissions)
            {
                entity.ReadGranted = true;
            }
        }        

        public async Task<(Role Role, bool AlreadyExists)> UpdateRoleAsync(Role role)
        {
            var updatedRoleEntity = _mapper.Map<RoleEntity>(role);
            var roleEntity = await _context.Roles
                .Include(r => r.RoleGroups)
                .Include(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .SingleOrDefaultAsync(x => x.Id == role.Id);
            if (roleEntity.Name != role.Name)
            {
                var roleWithNewNameAlreadyExists = await _context.Roles.AnyAsync(_ => _.Name == role.Name);

                if (roleWithNewNameAlreadyExists)
                {
                    return (ToRole(roleEntity), true);
                }
            }
            
            _context.Entry(roleEntity).CurrentValues.SetValues(updatedRoleEntity);
            UpdateActiveDirectoryGroups(roleEntity, role.ActiveDirectoryGroupIds);
            PrepareRoleAccesses(role.AccessGrantedItems);            
            
            UpdateRoleAccesses(roleEntity, role.AccessGrantedItems);
            await UpdateCorrespondingTabs(role);
            await InsertMissingAccessObjects(roleEntity, role.AccessGrantedItems);

            await _context.SaveChangesAsync();

            roleEntity = await GetRoleEntityByIdAsync(role.Id);
            return (ToRole(roleEntity), false);
        }

        private void UpdateRoleAccesses(RoleEntity entity, 
            AccessGrantedList accesses)
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
                var updatedAccessEntity = updatedAccessEntities.FirstOrDefault(a => a.AccessObjectId == accessEntity.AccessObjectId);
                if (updatedAccessEntity == null)
                {
                    continue;
                }
                updatedAccessEntity.Id = accessEntity.Id;

                _context.Entry(accessEntity).CurrentValues.SetValues(updatedAccessEntity);
            }            
        }        

        private async Task InsertMissingAccessObjects(RoleEntity entity, AccessGrantedList accesses)
        {
            var missingAccessObjects = await _context
                .AccessObjects
                .AsNoTracking()
                .Where(p => !entity.RoleAccessEntities.Select(p => p.AccessObjectId).Contains(p.Id))
                .ToListAsync();

            foreach (var missingAccess in missingAccessObjects)
            {
                var accessEntity = accesses.FirstOrDefault(p => p.Kind == missingAccess.Kind && p.Category == AccessCategory.Entity);
                if (missingAccess.Category == AccessCategory.Entity)
                {
                    var missingAccessEntity = new RoleAccessEntity()
                    {
                        AccessObjectId = missingAccess.Id,
                        RoleId = entity.Id,
                        ReadGranted = accessEntity?.ReadGranted ?? false,
                        UpdateGranted = accessEntity?.UpdateGranted ?? false,
                        CreateGranted = accessEntity?.CreateGranted ?? false,
                        CommentingGranted = accessEntity?.CommentingGranted ?? false,
                        SearchGranted = accessEntity?.SearchGranted ?? false,
                        AccessLevelUpdateGranted = accessEntity?.AccessLevelUpdateGranted ?? false
                    };
                    _context.Add(missingAccessEntity);
                }
                else
                {
                    var missingAccessEntity = new RoleAccessEntity()
                    {
                        AccessObjectId = missingAccess.Id,
                        RoleId = entity.Id,
                        ReadGranted = accessEntity?.ReadGranted ?? false
                    };
                    _context.Add(missingAccessEntity);
                }

            }
        }

        private async Task UpdateCorrespondingTabs(Role role)
        {
            var entityKinds = role.AccessGrantedItems.Where(p => p.Category == AccessCategory.Entity).Select(prop => prop.Kind).ToList();
            var correspondingTabs = await _context
                .RoleAccess
                .Include(p => p.AccessObject)
                .Where(p => p.RoleId == role.Id && p.AccessObject.Category == AccessCategory.Tab && entityKinds.Contains(p.AccessObject.Kind)).ToListAsync();

            foreach (var tab in correspondingTabs)
            {
                var correspondingEntity = role.AccessGrantedItems.FirstOrDefault(p => p.Category == AccessCategory.Entity && p.Kind == tab.AccessObject.Kind);
                if (correspondingEntity == null)
                {
                    continue;
                }
                tab.ReadGranted = correspondingEntity.ReadGranted;
                _context.Update(tab);
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

        private void SaveRoleAccesses(Guid roleId, AccessGrantedList accesses)
        {
            var roleAccessEntities = _mapper.Map<List<RoleAccessEntity>>(accesses);
            foreach (var entity in roleAccessEntities)
            {
                entity.RoleId = roleId;
            }
            _context.RoleAccess.AddRange(roleAccessEntities);                        
        }

        private async Task SaveCorresponsingTabs(Role role)
        {
            var entityKinds = role.AccessGrantedItems.Where(p => p.Category == AccessCategory.Entity).Select(prop => prop.Kind).ToList();
            var correspondingTabs = await _context.AccessObjects.Where(p => p.Category == AccessCategory.Tab && entityKinds.Contains(p.Kind)).ToListAsync();

            foreach (var tab in correspondingTabs)
            {
                var grantedEntity = role.AccessGrantedItems.FirstOrDefault(p => p.Kind == tab.Kind);
                if (grantedEntity == null)
                {
                    continue;
                }
                var entity = new RoleAccessEntity()
                {
                    AccessObjectId = tab.Id,
                    RoleId = role.Id,
                    ReadGranted = grantedEntity.ReadGranted
                };
                _context.Add(entity);
            }
        }

        private Task<RoleEntity> GetRoleEntityByIdAsync(Guid id)
        {
            return _context.Roles
                            .AsNoTracking()
                            .Include(r => r.RoleGroups)
                            .Include(r => r.RoleAccessEntities)
                            .ThenInclude(ra => ra.AccessObject)
                            .SingleOrDefaultAsync(r => r.Id == id);
        }

        private Role ToRole(RoleEntity entity)
        {
            var role = _mapper.Map<Role>(entity);
            return role;
        }

        private Role ToRole(RoleEntity entity, List<AccessObjectEntity> accessObjects)
        {
            var role = _mapper.Map<Role>(entity);
            var allAcesses = _mapper.Map<List<AccessGranted>>(accessObjects);
            role.AllowedItems = new AccessGrantedList(allAcesses);
            role.AccessGrantedItems = role.AccessGrantedItems.Merge(allAcesses);
            return role;
        }
    }
}