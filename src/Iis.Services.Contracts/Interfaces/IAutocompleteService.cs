using Iis.Services.Contracts.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IAutocompleteService
    {
        List<string> GetTips(string query, int count);

        Task<List<AutocompleteEntityDto>> GetEntitiesAsync(string query, int? size, CancellationToken ct = default);
    }
}