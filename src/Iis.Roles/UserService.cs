using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Iis.DataModel;
using Iis.DataModel.Roles;

namespace Iis.Roles
{
    public class UserService
    {
        private OntologyContext _context;
        private IMapper _mapper;

        public UserService(OntologyContext context, IMapper mapper)
        {
            _context = context;
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

        public async Task<Guid> UpdateUserAsync(User updatedUser)
        {
            var userEntity = GetUsersQuery()
                                        .FirstOrDefaultAsync(e => e.Id == updatedUser.Id)
                                        .GetAwaiter().GetResult();

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
            
            return userEntity == null ? null : Map(userEntity);
        }
        public User GetUser(Guid userId, string passwordHash)
        {
            var userEntity = GetUsersQuery()
                                    .SingleOrDefault(x => x.Id == userId && x.PasswordHash == passwordHash);

            return userEntity == null ? null : Map(userEntity);
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
            var roleEntityList = entity.UserRoles
                                    .Select(ur => ur.Role);

            var user = _mapper.Map<User>(entity);

            foreach (var roleEntity in roleEntityList)
            {
                if (roleEntity.IsAdmin)
                {
                    user.IsAdmin = true;
                }
                var accessGrantedList = roleEntity.RoleAccessEntities
                                            .Select(r => _mapper.Map<AccessGranted>(r));
                
                user.AccessGrantedItems.Merge(accessGrantedList);
            }

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
    }
}