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
        public Task<User> CreateUserAsync(User user)
        {
            return Task.FromResult(user);
        }

        public Task<User> UpdateUserAsync(User user)
        {
            return Task.FromResult(user);
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
    }
}