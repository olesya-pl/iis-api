using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

using HotChocolate;
using HotChocolate.Types;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

using IIS.Core.GraphQL.Users;
using ThemeMng = Iis.ThemeManagement;

namespace IIS.Core.GraphQL.Themes
{
    public class Mutation
    {
        public async Task<Theme> CreateTheme(
            [Service] ThemeMng.ThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] ThemeInput themeInput)
        {
            Validator.ValidateObject(themeInput, new ValidationContext(themeInput), true);

            var theme = mapper.Map<ThemeMng.Models.Theme>(themeInput);
            
            var themeType = await themeService.GetThemeTypeByEntityTypeNameAsync(themeInput.EntityTypeName);
            
            theme.Type = themeType;
            
            var themeId = await themeService.CreateThemeAsync(theme);

            theme = await themeService.GetThemeAsync(themeId);

            return mapper.Map<Theme>(theme);
        }

        public async Task<Theme> DeleteTheme(
            [Service] ThemeMng.ThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var theme = await themeService.DeleteThemeAsync(id);

            return mapper.Map<Theme>(theme);
        }
    } 
}