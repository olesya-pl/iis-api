using System;
using System.Threading.Tasks;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts;
using System.Linq;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class MutationDeleteResolver
    {
        private readonly IOntologyService _ontologyService;
        private readonly IOntologySchema _ontologySchema;

        public MutationDeleteResolver(IOntologyService ontologyService, IOntologySchema ontologySchema)
        {
            _ontologyService = ontologyService;
            _ontologySchema = ontologySchema;
        }

        public MutationDeleteResolver(IResolverContext ctx)
        {
            _ontologySchema = ctx.Service<IOntologySchema>();
            _ontologyService = ctx.Service<IOntologyService>();
        }

        public Entity DeleteEntity(IResolverContext ctx, string typeName)
        {
            var id = ctx.Argument<Guid>("id");
            var tokenPayload = ctx.GetToken();
            return DeleteEntity(id, typeName, tokenPayload.User);
        }

        private Entity DeleteEntity(Guid id, string typeName, User user)
        {
            var node = (Entity)_ontologyService.GetNode(id); // load only type
            if (node == null)
                throw new QueryException($"Entity with id {id} was not found");
            var type = _ontologySchema.GetEntityTypeByName(typeName);
            if (!node.Type.IsSubtypeOf(type))
                throw new QueryException($"Entity with id {id} is of type {node.Type.Name}, not of type {type.Name}");
            VerifyAccess(node, user);
            _ontologyService.RemoveNodeAndRelations(node);
            return node;
        }

        private void VerifyAccess(Entity node, User user)
        {
            var existingAccessLevel = node.OriginalNode.GetAccessLevelIndex();
            if (existingAccessLevel > user.AccessLevel)
            {
                throw new QueryException($"Entity with was not found");
            }
        }
    }
}
