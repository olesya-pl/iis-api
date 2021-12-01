using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Dtos.RadioElectronicSituation;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IRadioElectronicSituationService
    {
        Task<IReadOnlyCollection<ResSourceItemDto>> GetSituationNodesAsync();
    }
}