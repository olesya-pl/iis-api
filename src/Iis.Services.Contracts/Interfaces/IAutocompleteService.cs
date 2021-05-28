
using Iis.Domain.Users;
using Iis.Services.Contracts.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IAutocompleteService
    {
        IReadOnlyCollection<string> GetTips(string query, int count);

        Task<IReadOnlyCollection<AutocompleteEntityDto>> GetEntitiesAsync(string query, string[] types, int size, User user, CancellationToken ct = default);
    }
}