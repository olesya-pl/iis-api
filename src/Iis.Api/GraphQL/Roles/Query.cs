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
                            Title = "Материалы",
                            AllowedOperations = new List<string>{"Update", "Read" }
                        },
                        new AccessEntity
                        {
                            Kind = AccessKind.Dor.ToString(),
                            Title = "Объекты разведки",
                            AllowedOperations = new List<string>{"Read" }
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
    }
}
