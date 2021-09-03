using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.DataModel.Themes;

namespace Iis.DbLayer.Repositories
{
    public interface IThemeRepository
    {
        Task<ThemeEntity> GetByIdAsync(Guid themeId);
        void Update(ThemeEntity entity);
        Task<IReadOnlyCollection<ThemeTypeEntity>> GetThemeTypesByEntityTypeNamesAsync(IReadOnlyCollection<string> entityTypeNames);
    }
}
