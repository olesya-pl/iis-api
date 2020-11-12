using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;

using IIS.Core.GraphQL.Common;

using ThemeMng = Iis.ThemeManagement;
using Iis.DbLayer.Repositories;

namespace IIS.Core.GraphQL.Themes
{
    public class Query
    {
        public async Task<GraphQLCollection<Theme>> GetThemesForUser(
            [Service] ThemeMng.ThemeService<IIISUnitOfWork> themeService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid userId)
        {
            var themes = await themeService.GetThemesByUserIdAsync(userId);

            var graphThemes = mapper.Map<List<Theme>>(themes);

            return new GraphQLCollection<Theme>(graphThemes, graphThemes.Count);
        }

        public async Task<Theme> GetTheme(
            [Service] ThemeMng.ThemeService<IIISUnitOfWork> themeService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var theme = await themeService.GetThemeAsync(id);

            return mapper.Map<Theme>(theme);
        }

        public async Task<GraphQLCollection<ThemeType>> GetThemeTypes(
            [Service] ThemeMng.ThemeService<IIISUnitOfWork> themeService,
            [Service] IMapper mapper)
        {
            var themeTypes = await themeService.GetThemeTypesAsync();

            var graphThemeTypes = mapper.Map<List<ThemeType>>(themeTypes);

            return new GraphQLCollection<ThemeType>(graphThemeTypes, graphThemeTypes.Count);
        }
    }
}