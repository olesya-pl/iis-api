using Iis.Interfaces.SecurityLevels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface ISecurityLevelService
    {
        IReadOnlyList<SecurityLevelPlain> GetSecurityLevelsPlain();
        Task<ObjectSecurityDto> GetObjectSecurityDtosAsync(Guid id);
        Task SaveObjectSecurityDtoAsync(ObjectSecurityDto objectSecurityDto);
        void SaveSecurityLevel(SecurityLevelPlain levelPlain);
        void RemoveSecurityLevel(Guid id);
    }
}
