using System;
using System.Linq.Expressions;
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
    }
}
