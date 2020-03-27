using HotChocolate;
using Iis.DataModel;
using Iis.Interfaces.Roles;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Roles
{
    public class Query
    {
        public Task<GraphQLCollection<Role>> GetRoles([Service] OntologyContext context)
        {
            var roles = new List<Role>
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
                            AllowedOperations = new List<string>{"Update", "Read" }
                        },
                        new AccessEntity
                        {
                            Kind = AccessKind.Dor.ToString(),
                            Title = "Об'єкти розвідки",
                            AllowedOperations = new List<string>{"Read" }
                        },
                        new AccessEntity
                        {
                            Kind = AccessKind.Dor.ToString(),
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
                    IsAdmin = true
                },
            };
            return Task.FromResult(new GraphQLCollection<Role>(roles, roles.Count));
        }

        //public Task<GraphQLCollection<AccessTab>> GetAccessTabs([Service] OntologyContext context)
        //{
        //    var tabs = new List<AccessTab>
        //    {
        //        new AccessTab { Kind = AccessKind.}
        //    }
        //}
    }
}
