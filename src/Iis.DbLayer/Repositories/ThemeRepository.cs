using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Themes;
using IIS.Repository;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Repositories
{
    internal class ThemeRepository : RepositoryBase<OntologyContext>, IThemeRepository
    {
        public async Task<ThemeEntity> GetByIdAsync(Guid themeId)
        {
            return await Context.Themes.FirstOrDefaultAsync(p => p.Id == themeId);
        }

        public void Update(ThemeEntity entity)
        {
            Context.Themes.Update(entity);
        }

        public async Task<IReadOnlyCollection<ThemeTypeEntity>> GetThemeTypesByEntityTypeNamesAsync(IReadOnlyCollection<string> entityTypeNames)
        {
            var entities = await Context.ThemeTypes
                .AsNoTracking()
                .Where(_ => entityTypeNames.Contains(_.EntityTypeName))
                .ToListAsync();
            
            return entities;
        }
    }
}
