using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.SecurityLevels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface ISecurityLevelService
    {
        IReadOnlyList<SecurityLevelPlain> GetSecurityLevelsPlain();
        Task<ObjectSecurityDto> GetObjectSecurityDtosAsync(Guid id);
        Task SaveObjectSecurityDtoAsync(ObjectSecurityDto objectSecurityDto, CancellationToken cancellationToken);
        void SaveSecurityLevel(SecurityLevelPlain levelPlain);
        void RemoveSecurityLevel(Guid id);
        IReadOnlyList<INode> ChangeSecurityLevelsForLinkedNodes(INode node);
        IReadOnlyList<INode> ChangeSecurityLevelsForLinkedNodes(Guid nodeId);
    }
}
