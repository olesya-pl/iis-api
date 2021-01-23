using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Themes
{
    public class Query
    {
        public async Task<GraphQLCollection<Theme>> GetThemesForUser(
            [Service] IThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid userId)
        {
            var sortingParam = new SortingParams("updatedAt", "desc");

            var themes = await themeService.GetThemesByUserIdAsync(userId, sortingParam);

            var graphThemes = mapper.Map<List<Theme>>(themes);

            return new GraphQLCollection<Theme>(graphThemes, graphThemes.Count);
        }

        public async Task<Theme> GetTheme(
            [Service] IThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var theme = await themeService.GetThemeAsync(id);

            return mapper.Map<Theme>(theme);
        }

        public async Task<GraphQLCollection<ThemeType>> GetThemeTypes(
            [Service] IThemeService themeService,
            [Service] IMapper mapper)
        {
            var themeTypes = await themeService.GetThemeTypesAsync();

            var graphThemeTypes = mapper.Map<List<ThemeType>>(themeTypes);

            return new GraphQLCollection<ThemeType>(graphThemeTypes, graphThemeTypes.Count);
        }
    }
}