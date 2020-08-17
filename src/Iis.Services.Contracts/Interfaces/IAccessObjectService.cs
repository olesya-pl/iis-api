using Iis.Services.Contracts;
using System.Collections.Generic;

namespace Iis.Services
{
    public interface IAccessObjectService
    {
        IReadOnlyCollection<AccessGranted> GetAccesses();
    }
}