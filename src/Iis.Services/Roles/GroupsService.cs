using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Domain.Users;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Dtos.Roles;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces.Roles;
using Microsoft.EntityFrameworkCore;

namespace Iis.Services.Roles
{
    public class GroupsService : IGroupsService
    {
        private readonly IMapper _mapper;
        private readonly OntologyContext _context;
        private readonly IActiveDirectoryClient _activeDirectoryClient;

        public GroupsService(
            IMapper mapper,
            OntologyContext context,
            IActiveDirectoryClient activeDirectoryClient)
        {
            _mapper = mapper;
            _context = context;
            _activeDirectoryClient = activeDirectoryClient;
        }

        public async Task<GroupDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var group = _activeDirectoryClient
                  .GetGroupsByIds(id)
                  .SingleOrDefault();
            var groupRoles = await _context.RoleGroups
                .AsNoTracking()
                .Include(_ => _.Role)
                .ThenInclude(_ => _.RoleAccessEntities)
                .ThenInclude(_ => _.AccessObject)
                .ToArrayAsync(cancellationToken);

            return Map(group, groupRoles);
        }

        public async Task<IReadOnlyCollection<GroupAccessDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var groupsDictionary = _activeDirectoryClient
                .GetAllGroups()
                .ToDictionary(_ => _.Id);
            var roleGroups = await _context.RoleGroups
                   .AsNoTracking()
                   .Include(_ => _.Role)
                   .Where(_ => groupsDictionary.Keys.Contains(_.GroupId))
                   .ToArrayAsync(cancellationToken);

            return roleGroups
                .GroupBy(_ => _.GroupId)
                .Select(_ => new GroupAccessDto
                {
                    Id = _.Key,
                    Name = groupsDictionary[_.Key].Name,
                    Roles = _.Select(_ => _.Role)
                        .Select(role => new RoleDto
                        {
                            Id = role.Id,
                            Description = role.Description,
                            Name = role.Name
                        })
                        .ToArray()
                })
                .ToArray();
        }

        private GroupDto Map(ActiveDirectoryGroupDto groupDto, IReadOnlyCollection<RoleActiveDirectoryGroupEntity> entities)
        {
            if (groupDto is null) return null;

            var roleEntities = entities.Select(_ => _.Role);
            var group = new GroupDto
            {
                Id = groupDto.Id,
                Name = groupDto.Name,
                Roles = _mapper.Map<Role[]>(roleEntities)
            };

            foreach (var roleEntity in roleEntities)
            {
                var accessGrantedList = roleEntity.RoleAccessEntities.Select(_ => _mapper.Map<AccessGranted>(_));

                group.AccessGrantedItems.Merge(accessGrantedList);
            }

            return group;
        }
    }
}