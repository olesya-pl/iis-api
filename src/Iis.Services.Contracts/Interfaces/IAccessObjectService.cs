using Iis.Domain.Users;
using System.Collections.Generic;

namespace Iis.Services
{
    public interface IAccessObjectService
    {
        IReadOnlyCollection<AccessGranted> GetAccesses();
    }
}