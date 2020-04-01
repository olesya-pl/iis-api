using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.DataModel;
using Iis.Interfaces.Roles;
using Iis.Roles;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Roles
{
    public class Query
    {
        List<Role> _roles = new List<Role>
        {
            new Role
            {
                Id = "9444caa8-a883-4e3d-96e4-533cd6e52f3b",
                Name = "Оператор",
                Description = "Может редактировать материалы. Может привязывать материалы к ДОР, может просматривать ДОР.",
                IsAdmin = false,
                Entities = new List<AccessEntity>
                {
                    new AccessEntity
                    {
                        Kind = AccessKind.Material.ToString(),
                        Title = "Матеріали",
                        AllowedOperations = new List<string>{"update", "read" }
                    },
                    new AccessEntity
                    {
                        Kind = AccessKind.Dor.ToString(),
                        Title = "Об'єкти розвідки",
                        AllowedOperations = new List<string>{"read" }
                    },
                    new AccessEntity
                    {
                        Kind = AccessKind.Event.ToString(),
                        Title = "Події",
                        AllowedOperations = new List<string>()
                    }
                },
                Tabs = new List<AccessTab>
                {
                    new AccessTab
                    {
                        Kind = AccessKind.MapTab.ToString(),
                        Title = "Мапа",
                        Visible = false
                    },
                    new AccessTab
                    {
                            Kind = AccessKind.AdminTab.ToString(),
                        Title = "Адміністрування",
                        Visible = false
                    },
                    new AccessTab
                    {
                        Kind = AccessKind.EventsTab.ToString(),
                        Title = "Події",
                        Visible = true
                    },
                    new AccessTab
                    {
                        Kind = AccessKind.DorTab.ToString(),
                        Title = "Об'єкти розвідки",
                        Visible = true
                    },
                    new AccessTab
                    {
                        Kind = AccessKind.MaterialsTab.ToString(),
                        Title = "Вхідний поток",
                        Visible = true
                    }
                }
            },
            new Role
            {
                Id = "99a1ac90-523d-4b95-bf61-0b04e1e35ba1",
                Name = "Администратор",
                Description = "Может все",
                IsAdmin = true,
                Entities = new List<AccessEntity>
                {
                    new AccessEntity
                    {
                        Kind = AccessKind.Material.ToString(),
                        Title = "Матеріали",
                        AllowedOperations = new List<string>()
                    },
                    new AccessEntity
                    {
                        Kind = AccessKind.Dor.ToString(),
                        Title = "Об'єкти розвідки",
                        AllowedOperations = new List<string>()
                    },
                    new AccessEntity
                    {
                        Kind = AccessKind.Event.ToString(),
                        Title = "Події",
                        AllowedOperations = new List<string>()
                    }
                },
                Tabs = new List<AccessTab>
                {
                    new AccessTab
                    {
                        Kind = AccessKind.MapTab.ToString(),
                        Title = "Мапа",
                        Visible = false
                    },
                    new AccessTab
                    {
                        Kind = AccessKind.AdminTab.ToString(),
                        Title = "Адміністрування",
                        Visible = false
                    },
                    new AccessTab
                    {
                        Kind = AccessKind.EventsTab.ToString(),
                        Title = "Події",
                        Visible = true
                    },
                    new AccessTab
                    {
                        Kind = AccessKind.DorTab.ToString(),
                        Title = "Об'єкти розвідки",
                        Visible = true
                    },
                    new AccessTab
                    {
                        Kind = AccessKind.MaterialsTab.ToString(),
                        Title = "Вхідний поток",
                        Visible = true
                    }
                }
            },
        };
        public async Task<GraphQLCollection<Role>> GetRoles([Service] RoleLoader roleLoader, [Service] IMapper mapper)
        {
            var roles = await roleLoader.GetRolesAsync();
            var rolesQl = roles.Select(r => mapper.Map<Role>(r)).ToList();
            return new GraphQLCollection<Role>(rolesQl, roles.Count);
            //return Task.FromResult(new GraphQLCollection<Role>(_roles, _roles.Count));
        }

        public Task<Role> GetRole([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var role = _roles.SingleOrDefault(r => new Guid(r.Id) == id);
            if (role == null)
                throw new InvalidOperationException($"Cannot find role with id \"{id}\"");

            return Task.FromResult(role);
        }
    }
}
