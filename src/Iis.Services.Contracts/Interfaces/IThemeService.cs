using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IThemeService
    {
        Task<Guid> CreateThemeAsync(ThemeDto theme);
        Task<ThemeDto> DeleteThemeAsync(Guid themeId);
        Task<ThemeDto> GetThemeAsync(Guid themeId);
        Task<IEnumerable<ThemeDto>> GetThemesByUserIdAsync(Guid userId, PaginationParams paginationParams, SortingParams sorting);
        Task<IReadOnlyCollection<ThemeDto>> GetAllThemesByUserIdAsync(Guid userId);
        Task<IReadOnlyCollection<ThemeDto>> GetAllThemesByEntityTypeNamesAsync(Guid userId, IReadOnlyCollection<string> entityTypeNames);
        Task<ThemeTypeDto> GetThemeTypeByEntityTypeNameAsync(string entityTypeName);
        Task<IEnumerable<ThemeTypeDto>> GetThemeTypesAsync();
        Task<ThemeDto> SetReadCount(Guid themeId, int readCount);
        Task UpdateQueryResultsAsync(CancellationToken ct, params Guid[] themeTypes);
        Task<Guid> UpdateThemeAsync(ThemeDto theme);
    }
}