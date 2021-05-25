using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DataModel.Roles;
using Iis.DbLayer.Repositories;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.Services.Contracts.Params;
using Iis.Services.Contracts.Enums;
using Iis.Domain.Users;
using Microsoft.Extensions.Configuration;
using Iis.Utility;
using System.Security.Authentication;
using Iis.Interfaces.Users;
using System.Security.Cryptography;
using System.Text;

namespace Iis.Services
{
    public class UserService<TUnitOfWork> : BaseService<TUnitOfWork>, IUserService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly OntologyContext _context;
        private readonly MaxMaterialsPerOperatorConfig _maxMaterialsConfig;
        private readonly IMapper _mapper;
        private readonly IUserElasticService _userElasticService;
        private IConfiguration _configuration;
        private IExternalUserService _externalUserService;

        public UserService(
            OntologyContext context,
            MaxMaterialsPerOperatorConfig maxMaterialsConfig,
            IUserElasticService userElasticService,
            IMapper mapper,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IConfiguration configuration,
            IExternalUserService externalUserService) : base(unitOfWorkFactory)
        {
            _context = context;
            _maxMaterialsConfig = maxMaterialsConfig;
            _mapper = mapper;
            _userElasticService = userElasticService;
            _configuration = configuration;
            _externalUserService = externalUserService;
        }

        public async Task<Guid> CreateUserAsync(User newUser)
        {
            var entityExists = await _context.Users
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

            _context.Add(userEntity);

            _context.AddRange(userRolesEntitiesList);

            await _context.SaveChangesAsync();

            var elasticUser = _mapper.Map<ElasticUserDto>(userEntity);
            await _userElasticService.SaveUserAsync(elasticUser, CancellationToken.None);

            return userEntity.Id;
        }

        public async Task<List<User>> GetOperatorsAsync(CancellationToken ct = default)
        {
            var userEntityList = await RunWithoutCommitAsync(uow => uow.UserRepository.GetOperatorsAsync(ct));

            return userEntityList
                .Select(e => _mapper.Map<User>(e))
                .ToList();
        }

        public async Task<List<Guid>> GetAvailableOperatorIdsAsync()
        {
            var maxMaterialsCount = _maxMaterialsConfig.Value;

            var unavailableOperators = _context.Materials
                .AsNoTracking()
                .Where(p => (p.ProcessedStatusSignId == null
                    || p.ProcessedStatusSignId == MaterialEntity.ProcessingStatusNotProcessedSignId)
                    && p.ParentId == null
                    && p.AssigneeId != null)
                .GroupBy(p => p.AssigneeId)
                .Where(group => group.Count() >= maxMaterialsCount)
                .Select(group => group.Key)
                .Where(key => key.HasValue)
                .Select(key => key)
                .ToArray();

            var userList = await RunWithoutCommitAsync(uow => uow.UserRepository.GetOperatorsAsync(e => !unavailableOperators.Contains(e.Id), CancellationToken.None));

            return userList.Select(e => e.Id).ToList();
        }

        public async Task<Guid> UpdateUserAsync(User updatedUser, CancellationToken cancellation = default)
        {
            var userEntity = await RunWithoutCommitAsync(uow => uow.UserRepository.GetByIdAsync(updatedUser.Id, cancellation));

            if (userEntity is null)
            {
                throw new InvalidOperationException($"Cannot find User with id:'{updatedUser.Id}'.");
            }

            if (string.Equals(userEntity.PasswordHash, updatedUser.PasswordHash, StringComparison.Ordinal))
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

            _context.RemoveRange(_context.UserRoles.Where(ur => ur.UserId == userEntity.Id));
            _context.Update(userEntity);
            _context.UserRoles.AddRange(newUserRolesEntitiesList);
            await _context.SaveChangesAsync(cancellation);

            var elasticUser = _mapper.Map<ElasticUserDto>(userEntity);
            await _userElasticService.SaveUserAsync(elasticUser, cancellation);

            return userEntity.Id;
        }

        public async Task<User> GetUserAsync(Guid userId)
        {
            var userEntity = await RunWithoutCommitAsync(uow => uow.UserRepository.GetByIdAsync(userId, CancellationToken.None));

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
            var userEntity = RunWithoutCommit(uow => uow.UserRepository.GetByUserNameAndHash(userName, passwordHash));

            return Map(userEntity);
        }

        public User GetUserByUserName(string userName)
        {
            var userEntity = RunWithoutCommit(uow => uow.UserRepository.GetByUserName(userName));

            return Map(userEntity);
        }

        public bool ValidateCredentials(string userName, string password)
        {
            var hash = GetPasswordHashAsBase64String(password);
            var userEntity = GetUserByUserName(userName);
            return userEntity.PasswordHash == hash;
        }

        public User ValidateAndGetUser(string username, string password)
        {
            var user = GetUserByUserName(username);

            if (user == null)
                throw new InvalidCredentialException($"User {username} does not exists");

            if (user.IsBlocked)
                throw new InvalidCredentialException($"User {username} is blocked");

            if (user.Source != UserSource.Internal && user.Source != _externalUserService.GetUserSource())
                throw new InvalidCredentialException($"User {username} has wrong source");

            if (user.Source == UserSource.Internal && !ValidateCredentials(username, password) ||
                user.Source == _externalUserService.GetUserSource() && !_externalUserService.ValidateCredentials(username, password))
            {
                throw new InvalidCredentialException($"Wrong password");
            }

            return user;
        }

        public async Task<(IReadOnlyCollection<User> Users, int TotalCount)> GetUsersByStatusAsync(PaginationParams page, UserStatusType userStatusFilter, CancellationToken ct = default)
        {
            var (skip, take) = page.ToEFPage();

            Expression<Func<UserEntity, bool>> predicate = userStatusFilter switch
            {
                UserStatusType.Active  => (user) => !user.IsBlocked,
                UserStatusType.Blocked => (user) => user.IsBlocked,
                _ => (user) => true
            };

            var getUserListTask = RunWithoutCommitAsync(uow => uow.UserRepository.GetUsersAsync(skip, take, predicate, ct));
            var getUserCountTask = RunWithoutCommitAsync(uow => uow.UserRepository.GetUserCountAsync(predicate, ct));

            await Task.WhenAll(getUserListTask, getUserCountTask);

            var userList = await getUserListTask;
            var userCount = await getUserCountTask;

            var mappedUser = userList
                            .Select(Map)
                            .ToArray();

            return (mappedUser, userCount);
        }

        private User Map(UserEntity entity)
        {
            if (entity is null) return null;

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

        public bool IsAccessLevelAllowedForUser(int userAccessLevel, int newAccessLevel)
        {
            return userAccessLevel >= newAccessLevel;
        }

        public async Task PutAllUsersToElasticSearchAsync(CancellationToken cancellationToken)
        {
            var users = await RunWithoutCommitAsync(uowfactory => uowfactory.UserRepository.GetAllUsersAsync(cancellationToken));

            var elasticUsers = _mapper.Map<List<ElasticUserDto>>(users);

            await _userElasticService.SaveAllUsersAsync(elasticUsers, cancellationToken);
        }

        private string ComputeHash(string s)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hash = Encoding.UTF8.GetBytes(s);
                var generatedHash = sha1.ComputeHash(hash);
                var generatedHashString = Convert.ToBase64String(generatedHash);
                return generatedHashString;
            }
        }

        public string GetPasswordHashAsBase64String(string password)
        {
            var salt = _configuration.GetValue<string>("salt", string.Empty);
            return ComputeHash(password + salt);
        }

        public int ImportUsersFromExternalSource(IEnumerable<string> userNames = null)
        {
            var externalUsers = _externalUserService.GetUsers()
                .Where(eu => userNames == null || userNames.Contains(eu.UserName));

            var users = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToList();

            var roles = _context.Roles.ToList();

            int cnt = 0;

            foreach (var externalUser in externalUsers)
            {
                var user = users.FirstOrDefault(u => u.Username == externalUser.UserName);

                if (user == null)
                {
                    user = new UserEntity
                    {
                        Id = Guid.NewGuid(),
                        Username = externalUser.UserName,
                        Source = _externalUserService.GetUserSource(),
                        UserRoles = new List<UserRoleEntity>()
                    };
                    _context.Users.Add(user);
                    cnt++;
                }

                foreach (var externalRole in externalUser.Roles)
                {
                    if (!user.UserRoles.Any(ur => ur.Role.Name == externalRole.Name))
                    {
                        var role = roles.FirstOrDefault(r => r.Name == externalRole.Name);

                        if (role != null)
                        {
                            var userRole = new UserRoleEntity
                            {
                                UserId = user.Id,
                                RoleId = role.Id
                            };
                        }
                    }
                }
            }
            _context.SaveChanges();

            return cnt;
        }
    }
}