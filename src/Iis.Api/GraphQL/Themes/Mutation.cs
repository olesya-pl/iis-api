using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace IIS.Core.GraphQL.Themes
{
    public class Mutation
    {
        public async Task<Theme> CreateTheme(
            [Service] IThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] ThemeInput themeInput)
        {
            Validator.ValidateObject(themeInput, new ValidationContext(themeInput), true);

            var theme = mapper.Map<ThemeDto>(themeInput);

            var themeType = await themeService.GetThemeTypeByEntityTypeNameAsync(themeInput.EntityTypeName);

            theme.Type = themeType;

            var themeId = await themeService.CreateThemeAsync(theme);

            theme = await themeService.GetThemeAsync(themeId);

            return mapper.Map<Theme>(theme);
        }

        public async Task<Theme> UpdateTheme(
            [Service] IThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] UpdateThemeInput themeInput)
        {
            Validator.ValidateObject(themeInput, new ValidationContext(themeInput), true);

            var theme = mapper.Map<ThemeDto>(themeInput);
            if (!string.IsNullOrEmpty(themeInput.EntityTypeName))
            {
                var themeType = await themeService.GetThemeTypeByEntityTypeNameAsync(themeInput.EntityTypeName);
                theme.Type = themeType;
            }

            var themeId = await themeService.UpdateThemeAsync(theme);

            theme = await themeService.GetThemeAsync(themeId);
            return mapper.Map<Theme>(theme);
        }

        public async Task<Theme> DeleteTheme(
            [Service] IThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var theme = await themeService.DeleteThemeAsync(id);

            return mapper.Map<Theme>(theme);
        }

        public async Task<Theme> SetThemeReadCount(
            [Service] IThemeService themeService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] SetThemeReadCountInput themeInput)
        {
            var theme = await themeService.SetReadCount(themeInput.Id, themeInput.ReadCount);

            return mapper.Map<Theme>(theme);
        }
    }
}