using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Services.Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Core.Tools
{
    public class ExternalUserSeeder
    {
        IExternalUserService _externalUserService;
        OntologyContext _context;
        public ExternalUserSeeder(IExternalUserService externalUserService, OntologyContext ontologyContext)
        {
            _externalUserService = externalUserService;
            _context = ontologyContext;
        }
        public void Seed()
        {
            var externalUsers = _externalUserService.GetUsers();

            var users = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToList();

            var roles = _context.Roles.ToList();

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
        }
    }
}
