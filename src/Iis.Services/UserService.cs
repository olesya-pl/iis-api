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
using System.Security.Authentication;
using Iis.Interfaces.Users;
using System.Security.Cryptography;
using System.Text;
using Iis.Services.Contracts.Materials.Distribution;
using Iis.Services.Contracts.Extensions;
using Microsoft.Extensions.Logging;
using Iis.Services.Contracts.ExternalUserServices;

namespace Iis.Services
{
    public class UserService<TUnitOfWork> : BaseService<TUnitOfWork>, IUserService where TUnitOfWork : IIISUnitOfWork
    {
        private const string DefaultRoleName = "Користувач";

        private readonly ILogger<UserService<TUnitOfWork>> _logger;
        private readonly OntologyContext _context;
        private readonly MaxMaterialsPerOperatorConfig _maxMaterialsConfig;
        private readonly IMapper _mapper;
        private readonly IUserElasticService _userElasticService;
        private IConfiguration _configuration;
        private IExternalUserService _externalUserService;
        private IMatrixService _matrixService;

        public UserService(
            ILogger<UserService<TUnitOfWork>> logger,
            OntologyContext context,
            MaxMaterialsPerOperatorConfig maxMaterialsConfig,
            IUserElasticService userElasticService,
            IMapper mapper,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IConfiguration configuration,
            IExternalUserService externalUserService,
            IMatrixService matrixService) : base(unitOfWorkFactory)
        {
            _logger = logger;
            _context = context;
            _maxMaterialsConfig = maxMaterialsConfig;
            _mapper = mapper;
            _userElasticService = userElasticService;
            _configuration = configuration;
            _externalUserService = externalUserService;
            _matrixService = matrixService;
        }

        public async Task<Guid> CreateUserAsync(User newUser)
        {
            var entityExists = await _context.Users.AnyAsync(u => u.Username == newUser.UserName);
            if (entityExists)
            {
                throw new InvalidOperationException($"User with Username:'{newUser.UserName}' already exists");
            }

            var userEntity = _mapper.Map<UserEntity>(newUser);

            //TODO: temporaly solution
            userEntity.Name = $"{userEntity.LastName} {userEntity.FirstName} {userEntity.Patronymic}";

            var userRolesEntitiesList = newUser.Roles.Select(role => UserRoleEntity.CreateFrom(userEntity.Id, role.Id));

            _context.Add(userEntity);
            _context.AddRange(userRolesEntitiesList);

            await _context.SaveChangesAsync();

            var elasticUser = _mapper.Map<ElasticUserDto>(userEntity);

            await Task.WhenAll(_userElasticService.SaveUserAsync(elasticUser, CancellationToken.None),
                _matrixService.CreateUserAsync(userEntity.Username, userEntity.Id.ToString("N")));

            return userEntity.Id;
        }

        public async Task<List<User>> GetOperatorsAsync(CancellationToken ct = default)
        {
            var userEntityList = await RunWithoutCommitAsync(_ => _.UserRepository.GetOperatorsAsync(ct));

            return userEntityList
                .Select(e => _mapper.Map<User>(e))
                .ToList();
        }

        public async Task<UserDistributionList> GetOperatorsForMaterialsAsync()
        {
            var maxMaterialsCount = _maxMaterialsConfig.Value;
            var chargedInfo = await _context.MaterialAssignees
                .AsNoTracking()
                .Include(_ => _.Material)
                .Where(p => (p.Material.ProcessedStatusSignId == null
                    || p.Material.ProcessedStatusSignId == MaterialEntity.ProcessingStatusNotProcessedSignId)
                    && p.Material.ParentId == null)
                .GroupBy(p => p.AssigneeId)
                .Select(group => new
                {
                    UserId = group.Key,
                    FreeSlots = maxMaterialsCount - group.Count()
                })
                .ToArrayAsync();
            var mapping = await _context.MaterialChannelMappings.ToArrayAsync();
            var allOperators = await RunWithoutCommitAsync(_ => _.UserRepository.GetOperatorsAsync());
            var distributions = from op in allOperators
                                join ci in chargedInfo
                                    on op.Id equals ci.UserId
                                where ci.FreeSlots > 0
                                select new UserDistributionItem(op.Id, ci.FreeSlots, GetRoles(op, mapping), GetChannels(op, mapping), op.AccessLevel);
            var allDistributions = allOperators
                .Where(op => !chargedInfo.Any(ci => ci.UserId == op.Id))
                .Select(op => new UserDistributionItem(op.Id, maxMaterialsCount, GetRoles(op, mapping), GetChannels(op, mapping), op.AccessLevel))
                .Concat(distributions);

            return new UserDistributionList(allDistributions);
        }

        public async Task<Guid> UpdateUserAsync(User updatedUser, CancellationToken cancellationToken = default)
        {
            var userEntity = await RunWithoutCommitAsync(uow => uow.UserRepository.GetByIdAsync(updatedUser.Id, cancellationToken));
            if (userEntity is null)
            {
                throw new InvalidOperationException($"Cannot find User with id:'{updatedUser.Id}'.");
            }

            if (userEntity.Source == UserSource.Internal &&
                string.Equals(userEntity.PasswordHash, updatedUser.PasswordHash, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"New password must not match old.");
            }

            var updatedEntity = _mapper.Map<UserEntity>(updatedUser);
            updatedEntity.Source = userEntity.Source;

            var newUserRolesEntitiesList = updatedUser.Roles.Select(role => UserRoleEntity.CreateFrom(updatedUser.Id, role.Id));
            var removed = await _context.UserRoles.Where(ur => ur.UserId == userEntity.Id).ToArrayAsync(cancellationToken);

            _mapper.Map(updatedEntity, userEntity);

            //TODO: temporaly solution
            userEntity.Name = $"{userEntity.LastName} {userEntity.FirstName} {userEntity.Patronymic}";

            _context.RemoveRange(removed);
            _context.Update(userEntity);
            _context.UserRoles.AddRange(newUserRolesEntitiesList);

            await _context.SaveChangesAsync(cancellationToken);

            var elasticUser = _mapper.Map<ElasticUserDto>(userEntity);
            await _userElasticService.SaveUserAsync(elasticUser, cancellationToken);

            return userEntity.Id;
        }

        public async Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var userEntity = await RunWithoutCommitAsync(uow => uow.UserRepository.GetByIdAsync(userId, cancellationToken));
            if (userEntity == null)
            {
                throw new ArgumentException($"User does not exist for id = {userId}");
            }
            return Map(userEntity);
        }

        public async Task<User> ValidateAndGetUserAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var userSource = _externalUserService?.GetUserSource();
            if (userSource == UserSource.ActiveDirectory)
            {
                await SynchronizeActiveDirectoryUserAsync(username, cancellationToken);
            }

            var userEntity = await RunWithoutCommitAsync(uow => uow.UserRepository.GetByUserNameAsync(username, cancellationToken));
            var user = Map(userEntity);

            if (user == null)
            {
                throw new InvalidCredentialException($"User {username} does not exists");
            }

            if (user.IsBlocked)
            {
                throw new InvalidCredentialException($"User {username} is blocked");
            }

            if (user.Source != UserSource.Internal && user.Source != _externalUserService.GetUserSource())
            {
                throw new InvalidCredentialException($"User {username} has wrong source");
            }

            if (user.Source == UserSource.Internal && !ValidateCredentials(user, password) ||
                user.Source == _externalUserService?.GetUserSource() && _externalUserService?.ValidateCredentials(username, password) == false)
            {
                throw new InvalidCredentialException($"Wrong password");
            }

            return user;
        }

        public async Task<(IReadOnlyCollection<User> Users, int TotalCount)> GetUsersByStatusAsync(
            PaginationParams page,
            SortingParams sorting,
            string suggestion,
            UserStatusType userStatusFilter,
            CancellationToken ct = default)
        {
            var (skip, take) = page.ToEFPage();
            bool? isBlocked = userStatusFilter switch
            {
                UserStatusType.Active => false,
                UserStatusType.Blocked => true,
                _ => null
            };

            Expression<Func<UserEntity, bool>> predicate = null;

            if (string.IsNullOrWhiteSpace(suggestion) && isBlocked.HasValue)
                predicate = user => user.IsBlocked == isBlocked;
            else if (!string.IsNullOrWhiteSpace(suggestion) && isBlocked.HasValue)
                predicate = user => user.IsBlocked == isBlocked && (EF.Functions.ILike(user.Username, $"%{suggestion}%") || EF.Functions.ILike(user.Name, $"%{suggestion}%"));
            else if (!string.IsNullOrWhiteSpace(suggestion) && !isBlocked.HasValue)
                predicate = user => EF.Functions.ILike(user.Username, $"%{suggestion}%") || EF.Functions.ILike(user.Name, $"%{suggestion}%");

            var getUserListTask = RunWithoutCommitAsync(uow => uow.UserRepository.GetUsersAsync(skip, take, sorting.ColumnName, sorting.AsSortDirection(), predicate, ct));
            var getUserCountTask = RunWithoutCommitAsync(uow => uow.UserRepository.GetUserCountAsync(predicate, ct));

            await Task.WhenAll(getUserListTask, getUserCountTask);

            var userList = await getUserListTask;
            var userCount = await getUserCountTask;

            var mappedUser = userList
                            .Select(Map)
                            .ToArray();

            return (mappedUser, userCount);
        }

        public async Task<User> AssignRoleAsync(Guid userId, Guid roleId)
        {
            var userExists = await _context.Users
                .AnyAsync(user => user.Id == userId);
            if (!userExists)
            {
                throw new Exception($"User is not found for id = {userId}");
            }

            var roleExists = await _context.Roles.AnyAsync(role => role.Id == roleId);
            if (!roleExists)
            {
                throw new Exception($"Role is not found for id = {roleId}");
            }

            var existing = await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId);
            if (!existing)
            {
                var userRole = UserRoleEntity.CreateFrom(userId, roleId);
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }

            return await GetUserAsync(userId);
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

            return await GetUserAsync(userId);
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
        
        public string GetPasswordHashAsBase64String(string password)
        {
            var salt = _configuration.GetValue("salt", string.Empty);
            return ComputeHash(password + salt);
        }

        public async Task<string> ImportUsersFromExternalSourceAsync(IEnumerable<string> userNames = null, CancellationToken cancellationToken = default)
        {
            if (_externalUserService == null) return null;

            var externalUsers = _externalUserService.GetUsers()
                .Where(eu => userNames == null || userNames.Contains(eu.UserName)
                    && !eu.UserName.Contains('$'));
            var users = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToArrayAsync(cancellationToken);
            var roles = await _context.Roles
                .AsNoTracking()
                .ToArrayAsync(cancellationToken);
            var defaultRole = GetDefaultRole(roles);
            var sb = new StringBuilder();

            foreach (var externalUser in externalUsers)
            {
                var user = users.FirstOrDefault(u => u.Username == externalUser.UserName);

                if (user == null)
                {
                    user = new UserEntity
                    {
                        Id = Guid.NewGuid(),
                        Username = externalUser.UserName,
                        FirstName = externalUser.FirstName,
                        Patronymic = externalUser.SecondName,
                        LastName = externalUser.LastName,
                        Source = _externalUserService.GetUserSource(),
                        UserRoles = new List<UserRoleEntity>()
                    };
                    _context.Users.Add(user);

                    if (_matrixService?.AutoCreateUsers == true)
                    {
                        try
                        {
                            var msg = await _matrixService.CreateUserAsync(user.Username, user.Id.ToString("N"));
                            if (msg == null)
                                sb.AppendLine("... matrix user is successfully created");
                            else
                                sb.AppendLine($"... matrix returns error: {msg}");
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"... matrix returns error: { ex.Message }");
                        }
                    }

                    sb.AppendLine($"User {user.Username} is successfully created");
                }
                else
                {
                    sb.AppendLine($"User {user.Username} already exists");
                }

                if (!user.UserRoles.Any(_ => _.RoleId == defaultRole.Id))
                {
                    var userRole = UserRoleEntity.CreateFrom(user.Id, defaultRole.Id);
                    _context.Add(userRole);
                }

                foreach (var externalRole in externalUser.Roles)
                {
                    if (!user.UserRoles.Any(ur => (ur.Role?.Name ?? defaultRole.Name) == externalRole.Name))
                    {
                        var role = roles.FirstOrDefault(r => r.Name == externalRole.Name);

                        if (role != null)
                        {
                            var userRole = UserRoleEntity.CreateFrom(user.Id, role.Id);
                            _context.Add(userRole);
                            sb.AppendLine($"... {externalRole.Name} is successfully assigned");
                        }
                        else
                        {
                            sb.AppendLine($"... {externalRole.Name} does not exists and cannot be assigned");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"... {externalRole.Name} is already assigned");
                    }
                }
            }

            await _context.SaveChangesAsync();

            return sb.ToString();
        }

        public async Task<string> GetUserMatrixInfoAsync()
        {
            var msg = await _matrixService.CheckMatrixAvailableAsync();
            if (msg != null) return msg;

            var users = GetActiveUsers();
            var sb = new StringBuilder();

            foreach (var user in users)
            {
                var userExists = await _matrixService.UserExistsAsync(user.UserName, user.Id.ToString("N"));
                sb.AppendLine($"{user.UserName}\t\t\t{userExists}");
            }
            return sb.ToString();
        }

        public async Task<string> CreateMatrixUsersAsync(List<string> userNames = null)
        {
            var serverAvailability = await _matrixService.CheckMatrixAvailableAsync();
            if (serverAvailability != null) return serverAvailability;

            var users = GetActiveUsers()
                .Where(u => userNames == null || userNames.Contains(u.UserName));

            var sb = new StringBuilder();

            foreach (var user in users)
            {
                var userExists = await _matrixService.UserExistsAsync(user.UserName, user.Id.ToString("N"));
                if (userExists)
                {
                    sb.AppendLine($"{user.UserName}\t\t\t already exists");
                    continue;
                }
                var msg = await _matrixService.CreateUserAsync(user.UserName, user.Id.ToString("N"));
                if (msg == null)
                {
                    sb.AppendLine($"{user.UserName}\t\t\tsuccessfully created");
                }
                else
                {
                    sb.AppendLine($"{user.UserName}\t\t\t{msg}");
                }

            }
            return sb.ToString();
        }

        private RoleEntity GetDefaultRole(IEnumerable<RoleEntity> roles)
        {
            var defaultRole = roles.SingleOrDefault(_ => _.Name == DefaultRoleName);
            if (defaultRole is null)
            {
                defaultRole = new RoleEntity { Id = Guid.NewGuid(), Name = DefaultRoleName, Description = DefaultRoleName };
                _context.Add(defaultRole);
            }

            return defaultRole;
        }

        private async Task SynchronizeActiveDirectoryUserAsync(string userName, CancellationToken cancellationToken = default)
        {
            var userEntity = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(_ => _.Username == userName, cancellationToken);
            if (userEntity != null && userEntity.Source != UserSource.ActiveDirectory) return;

            var externalUser = _externalUserService.GetUser(userName);
            if (externalUser is null)
            {
                OnExternalUserNotFound(userEntity);
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            userEntity = userEntity is null
                ? await CreateUserAsync(externalUser)
                : UpdateUser(userEntity, externalUser);

            await UpdateUserRolesAsync(externalUser, userEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await PutUserToElasticSearchAsync(userEntity.Id, cancellationToken);
        }

        private async Task PutUserToElasticSearchAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await RunWithoutCommitAsync(uowfactory => uowfactory.UserRepository.GetByIdAsync(id, cancellationToken));
            var elasticUser = _mapper.Map<ElasticUserDto>(user);

            await _userElasticService.SaveUserAsync(elasticUser, cancellationToken);
        }

        private void OnExternalUserNotFound(UserEntity userEntity)
        {
            if (userEntity is null) return;

            userEntity.IsBlocked = true;

            if (_matrixService?.AutoCreateUsers == true)
            {
                //TODO: remove from matrix service
            }
        }

        private UserEntity UpdateUser(UserEntity userEntity, ExternalUser externalUser)
        {
            userEntity.FirstName = externalUser.FirstName;
            userEntity.Patronymic = externalUser.SecondName;
            userEntity.LastName = externalUser.LastName;

            return userEntity;
        }

        private async Task<UserEntity> CreateUserAsync(ExternalUser externalUser)
        {
            var userEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = externalUser.UserName,
                FirstName = externalUser.FirstName,
                Patronymic = externalUser.SecondName,
                LastName = externalUser.LastName,
                Source = _externalUserService.GetUserSource(),
                UserRoles = new List<UserRoleEntity>()
            };
            _context.Users.Add(userEntity);

            if (_matrixService?.AutoCreateUsers == true)
            {
                try
                {
                    await _matrixService.CreateUserAsync(userEntity.Username, userEntity.Id.ToString("N"));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Create matrix user failed");
                }
            }

            return userEntity;
        }

        private async Task UpdateUserRolesAsync(ExternalUser externalUser, UserEntity userEntity, CancellationToken cancellationToken)
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .ToDictionaryAsync(_ => _.Name, cancellationToken);
            var defaultRole = GetDefaultRole(roles.Values);
            if (!userEntity.UserRoles.Any(_ => _.RoleId == defaultRole.Id))
            {
                var userRole = UserRoleEntity.CreateFrom(userEntity.Id, defaultRole.Id);
                _context.Add(userRole);
            }

            foreach (var externalRole in externalUser.Roles)
            {
                if (userEntity.UserRoles.Any(_ => (_.Role?.Name ?? defaultRole.Name) == externalRole.Name)
                    || !roles.TryGetValue(externalRole.Name, out var role))
                {
                    continue;
                }

                var userRole = UserRoleEntity.CreateFrom(userEntity.Id, role.Id);
                _context.Add(userRole);
            }
        }

        private List<User> GetActiveUsers() =>
            _context.Users.Where(u => !u.IsBlocked).Select(u => _mapper.Map<User>(u)).ToList();

        private bool ValidateCredentials(User user, string password)
        {
            var hash = GetPasswordHashAsBase64String(password);

            return user.PasswordHash == hash;
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

        private List<string> GetRoles(UserEntity userEntity, IEnumerable<MaterialChannelMappingEntity> mapping) =>
           userEntity.UserRoles
               .Where(ur => mapping.Any(mp => mp.RoleId == ur.RoleId))
               .Select(ur => ur.Role.Id.ToString("N")).ToList();

        private IReadOnlyList<string> GetChannels(UserEntity userEntity, IEnumerable<MaterialChannelMappingEntity> mapping)
        {
            var result =
                (from ur in userEntity.UserRoles
                 join mp in mapping on ur.RoleId equals mp.RoleId
                 select mp.ChannelName).ToList();

            return result;
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
    }
}