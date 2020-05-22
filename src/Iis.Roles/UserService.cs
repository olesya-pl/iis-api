using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.DataModel.Materials;
using Microsoft.Extensions.Configuration;

namespace Iis.Roles
{
    public class UserService
    {
        private readonly OntologyContext _context;
        private readonly MaxMaterialsPerOperatorConfig _maxMaterialsConfig;
        private readonly IMapper _mapper;

        public UserService(
            OntologyContext context,
            MaxMaterialsPerOperatorConfig maxMaterialsConfig,
            IMapper mapper)
        {
            _context = context;
            _maxMaterialsConfig = maxMaterialsConfig;
            _mapper = mapper;
        }
        public async Task<Guid> CreateUserAsync(User newUser)
        {
            var entityExists= await _context.Users
                                            .AnyAsync(u => u.Username == newUser.UserName);
            if (entityExists)
            {
                throw new InvalidOperationException($"User with Username:'{newUser.UserName}' already exists");
            }

            var userEntity = _mapper.Map<UserEntity>(newUser);

            //TODO: temporaly solution
            userEntity.Name = $"{userEntity.LastName} {userEntity.FirstName} {userEntity.Patronymic}";

            var userRolesEntitiesList = newUser.Roles
                                    .Select(role => CreateUserRole(userEntity.Id, role.Id))
                                    .ToList();

            await _context.Semaphore.WaitAsync();
            try
            {
                _context.Add(userEntity);

                _context.AddRange(userRolesEntitiesList);

                 await _context.SaveChangesAsync();

                return userEntity.Id;
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        private IQueryable<UserEntity> GetOperatorsQuery()
        {
            return _context.Users.AsNoTracking();
        }

        public Task<List<User>> GetOperatorsAsync()
        {
            return GetOperatorsQuery()
                .Select(p => _mapper.Map<User>(p))
                .ToListAsync();
        }

        public Task<List<Guid>> GetAvailableOperatorIdsAsync()
        {
            var maxMaterialsCount = _maxMaterialsConfig.Value;

            var unavailableOperators = _context.Materials
                .AsNoTracking()
                .Where(p => (p.ProcessedStatusSignId == null
                    || p.ProcessedStatusSignId == MaterialEntity.ProcessingStatusNotProcessedSignId)
                    && p.AssigneeId != null)
                .GroupBy(p => p.AssigneeId)
                .Where(group => group.Count() >= maxMaterialsCount)
                .Select(group => group.Key);

            return GetOperatorsQuery()
                .Select(p => p.Id)
                .Where(p => !unavailableOperators.Contains(p))
                .ToListAsync();
        }

        public async Task<Guid> UpdateUserAsync(User updatedUser)
        {
            var userEntity = await GetUsersQuery()
                                        .FirstOrDefaultAsync(e => e.Id == updatedUser.Id);

            if (userEntity is null)
            {
                throw new InvalidOperationException($"Cannot find User with id:'{updatedUser.Id}'.");
            }

            if (userEntity.PasswordHash == updatedUser.PasswordHash)
            {
                throw new InvalidOperationException($"New password must not match old.");
            }

            var updatedEntity = _mapper.Map<UserEntity>(updatedUser);

            var newUserRolesEntitiesList = updatedUser.Roles
                                            .Select(role => CreateUserRole(updatedUser.Id, role.Id))
                                            .ToList();

            _mapper.Map(updatedEntity, userEntity);

            //TODO: temporaly solution
            userEntity.Name = $"{userEntity.LastName} {userEntity.FirstName} {userEntity.Patronymic}";

            await _context.Semaphore.WaitAsync();
            try
            {
                _context.RemoveRange(_context.UserRoles.Where(ur => ur.UserId == userEntity.Id));

                _context.Update(userEntity);

                _context.UserRoles.AddRange(newUserRolesEntitiesList);

                await _context.SaveChangesAsync();

                return userEntity.Id;
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }
        public async Task<User> GetUserAsync(Guid userId)
        {
            var userEntity = await GetUsersQuery()
                                    .SingleOrDefaultAsync(u => u.Id == userId);

            if (userEntity == null)
            {
                throw new ArgumentException($"User does not exist for id = {userId}");
            }

            return Map(userEntity);
        }
        public User GetUser(Guid userId)
        {
            return GetUserAsync(userId).GetAwaiter().GetResult();
        }
        public User GetUser(string userName, string passwordHash)
        {
            var userEntity = GetUsersQuery()
                                    .SingleOrDefault(x => x.Username == userName && x.PasswordHash == passwordHash);

            return Map(userEntity);
        }
        public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(int offset, int pageSize)
        {
            var userEntities = await GetUsersQuery()
                                    .Skip(offset)
                                    .Take(pageSize)
                                    .AsNoTracking()
                                    .ToListAsync();

            var userEntitiesCount = await _context.Users
                                        .CountAsync();

            return (userEntities.Select(e => Map(e)).ToList(), userEntitiesCount);
        }
        private IQueryable<UserEntity> GetUsersQuery()
        {
            return _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .AsNoTracking();
        }
        private User Map(UserEntity entity)
        {
            if(entity is null) return null;

            var roleEntityList = entity.UserRoles
                                    .Select(ur => ur.Role);

            var user = _mapper.Map<User>(entity);

            foreach (var roleEntity in roleEntityList)
            {
                var accessGrantedList = roleEntity.RoleAccessEntities
                                            .Select(r => _mapper.Map<AccessGranted>(r));

                user.AccessGrantedItems.Merge(accessGrantedList);
            }

            user.IsAdmin = roleEntityList.Any(re => re.IsAdmin);

            return user;
        }
        private UserRoleEntity CreateUserRole(Guid userId, Guid roleId)
        {
            return new UserRoleEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId
            };
        }

        public async Task<User> AssignRole(Guid userId, Guid roleId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(user => user.Id == userId);
            if (user == null)
            {
                throw new Exception($"User is not found for id = {userId.ToString()}");
            }
            var role = await _context.Roles.SingleOrDefaultAsync(role => role.Id == roleId);
            if (role == null)
            {
                throw new Exception($"Role is not found for id = {roleId.ToString()}");
            }

            var existing = _context.UserRoles?.SingleOrDefault(ur => ur.RoleId == roleId);
            if (existing == null)
            {
                _context.UserRoles.Add(CreateUserRole(userId, roleId));
                await _context.SaveChangesAsync();
            }

            return GetUser(userId);
        }

        public async Task<User> RejectRole(Guid userId, Guid roleId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(user => user.Id == userId);
            if (user == null)
            {
                throw new Exception($"User is not found for id = {userId.ToString()}");
            }
            var role = await _context.Roles.SingleOrDefaultAsync(role => role.Id == roleId);
            if (role == null)
            {
                throw new Exception($"Role is not found for id = {roleId.ToString()}");
            }

            var existing = _context.UserRoles?.SingleOrDefault(ur => ur.RoleId == roleId);

            if (existing != null)
            {
                _context.UserRoles.Remove(existing);
                await _context.SaveChangesAsync();
            }

            return GetUser(userId);
        }
    }
}