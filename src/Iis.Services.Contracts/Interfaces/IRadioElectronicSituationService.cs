using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Services.Contracts.Dtos;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IRadioElectronicSituationService
    {
        Task<IReadOnlyCollection<SituationNodeDto>> GetSituationNodesAsync();
    }
}