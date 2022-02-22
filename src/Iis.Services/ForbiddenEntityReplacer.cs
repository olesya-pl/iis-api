using System;
using System.Collections.Generic;
using Iis.Domain;
using Iis.Domain.Users;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Services
{
    public class ForbiddenEntityReplacer
    {
        private readonly ISecurityLevelChecker _securityLevelChecker;
        private readonly IOntologyService _ontologyService;

        public ForbiddenEntityReplacer(ISecurityLevelChecker securityLevelChecker, IOntologyService ontologyService)
        {
            _securityLevelChecker = securityLevelChecker;
            _ontologyService = ontologyService;
        }

        public void Replace(IEnumerable<Iis.Domain.Materials.RelatedObject> relatedEntityCollection, User user)
        {
            foreach (var entity in relatedEntityCollection)
            {
                entity.AccessAllowed = IsAllowedEntityForUser(entity.Id, user);

                if (entity.AccessAllowed) continue;

                entity.Id = Guid.Empty;
                entity.Title = string.Empty;
                entity.NodeType = string.Empty;
                entity.RelationType = string.Empty;
                entity.RelationCreatingType = string.Empty;
            }
        }

        private bool IsAllowedEntityForUser(Guid id, User user)
        {
            return _securityLevelChecker.AccessGranted(
                user.SecurityLevelsIndexes,
                _ontologyService.GetNode(id).OriginalNode.GetSecurityLevelIndexes());
        }
    }
}