using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.Access
{
    public class GraphQLAccessList: List<GraphQLAccessItem>
    {
        public GraphQLAccessList()
        {
            AddRange(new List<GraphQLAccessItem>
            {
                new GraphQLAccessItem(AccessKind.FreeForAll, AccessOperation.None, @"getEntityTypes"),
                new GraphQLAccessItem(AccessKind.Dor, AccessOperation.Create, @"createEntity.*"),
                new GraphQLAccessItem(AccessKind.Dor, AccessOperation.Read, @"getEntity.*"),
                new GraphQLAccessItem(AccessKind.Dor, AccessOperation.Update, @"updateEntity.*"),
                new GraphQLAccessItem(AccessKind.Dor, AccessOperation.Delete, @"deleteEntity.*"),
                
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Create, @"createMaterial.*"),
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Read, @"getMaterial.*"),
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Update, @"updateMaterial.*"),
                new GraphQLAccessItem(AccessKind.Material, AccessOperation.Delete, @"deleteMaterial.*"),

                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Create, @"createEvent.*"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Read, @"getEvent.*"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Update, @"updateEvent.*"),
                new GraphQLAccessItem(AccessKind.Event, AccessOperation.Delete, @"deleteEvent.*"),
            });
        }

        public GraphQLAccessItem GetAccessItem(string graphQLItem)
        {
            return this.FirstOrDefault(ai => ai.IsMatch(graphQLItem));
        }
    }
}
