using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using HotChocolate.Resolvers;
using Iis.Api.GraphQL.Common;
using Iis.Interfaces.Elastic;

namespace IIS.Core.GraphQL.Themes
{
    public class Query
    {
        public async Task<GraphQLCollection<Theme>> GetThemesForUser(
            IResolverContext ctx,
            [Service] IThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] PaginationInput pagination,
            SortingInput sorting)
        {
            var userId = ctx.GetToken().UserId;

            var paginationParams = new PaginationParams(pagination.Page, pagination.PageSize);
            
            var sortingParams = mapper.Map<SortingParams>(sorting) ?? SortingParams.Default;
            
            var themes = await themeService.GetThemesByUserIdAsync(userId, paginationParams, sortingParams);

            var graphThemes = mapper.Map<List<Theme>>(themes);

            return new GraphQLCollection<Theme>(graphThemes, graphThemes.Count);
        }
        
        public async Task<GraphQLCollection<Theme>> GetThemesByEntityTypeNames(
            IResolverContext ctx,
            [Service] IThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] ThemesFilterInput filter)
        {
            var userId = ctx.GetToken().UserId;
            
            var entityTypeNames = filter.EntityTypeNames;
            
            var themes = await themeService.GetAllThemesByEntityTypeNamesAsync(userId, entityTypeNames);

            var graphThemes = mapper.Map<List<Theme>>(themes);

            return new GraphQLCollection<Theme>(graphThemes, graphThemes.Count);
        }
        
        public async Task<UnreadCount> GetUnreadThemesCount(
            IResolverContext ctx,
            [Service] IThemeService themeService)
        {
            var userId = ctx.GetToken().UserId;
            
            var themes = await themeService.GetAllThemesByUserIdAsync(userId);
            
            var count = themes.Aggregate(0, (acc, theme) => acc + theme.UnreadCount);
     
            return new UnreadCount
            {
                Count = count
            };
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
    
    public class UnreadCount
    {
        public int Count { get; set; }
    }
}