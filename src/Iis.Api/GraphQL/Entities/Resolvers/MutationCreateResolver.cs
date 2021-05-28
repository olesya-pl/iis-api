using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Common;
using Iis.Services;
using Iis.Domain.Users;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class MutationCreateResolver
    {
        private readonly IOntologySchema _ontologySchema;
        private readonly IResolverContext _resolverContext;
        private readonly IAccessLevels _accessLevels;
        private readonly CreateEntityService _createEntityService;

        public MutationCreateResolver(IOntologySchema ontologySchema, 
            ICommonData commonData,
            CreateEntityService createEntityService)
        {
            _ontologySchema = ontologySchema;
            _accessLevels = commonData.AccessLevels;
            _createEntityService = createEntityService;
        }

        public MutationCreateResolver(IResolverContext ctx)
        {
            _ontologySchema = ctx.Service<IOntologySchema>();
            _accessLevels = ctx.Service<IOntologyNodesData>().GetAccessLevels();
            _createEntityService = ctx.Service<CreateEntityService>();
            _resolverContext = ctx;
        }

        public async Task<Entity> CreateEntity(IResolverContext ctx, string typeName)
        {
            var data = ctx.Argument<Dictionary<string, object>>("data");

            var type = _ontologySchema.GetEntityTypeByName(typeName);
            var tokenPayload = ctx.GetToken();
            VerifyAccess(tokenPayload.User, data);
            var entityId = Guid.NewGuid();
            var entity = await _createEntityService.CreateEntity(entityId, type, data, string.Empty, entityId, Guid.NewGuid(), GetCurrentUser());
            return entity;
        }

        public Task<Entity> CreateEntity(
            Guid entityId,
            INodeTypeLinked type,
            string dotName,
            Dictionary<string, object> properties,
            Guid requestId)
        {
            return _createEntityService.CreateEntity(entityId, type, properties, dotName, Guid.NewGuid(), requestId, GetCurrentUser());
        }

        private void VerifyAccess(User user, Dictionary<string, object> properties)
        {
            if (properties.ContainsKey("accessLevel"))
            {
                var accessLevelProperty = properties["accessLevel"] as Dictionary<string, object>;
                var accessId = Guid.Parse(accessLevelProperty["targetId"].ToString());
                var accessLevel = _accessLevels.GetItemById(accessId);
                if (accessLevel.NumericIndex > user.AccessLevel)
                {
                    throw new AccessViolationException("Unable to create entitiy with given access level");
                }
            }
        }

        private User GetCurrentUser()
        {
            if (_resolverContext is null) return null;

            var tokenPayload = _resolverContext.GetToken();
            return tokenPayload?.User;
        }
    }
}
