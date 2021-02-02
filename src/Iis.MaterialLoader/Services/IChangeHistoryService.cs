using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.Services.Contracts.Dtos;

namespace Iis.MaterialLoader.Services
{
    public interface IChangeHistoryService
    {
        Task SaveMaterialChanges(IReadOnlyCollection<ChangeHistoryDto> changes);
    }
}