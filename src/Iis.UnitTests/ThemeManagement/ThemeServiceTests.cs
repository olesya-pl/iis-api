using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using Iis.DataModel;
using Iis.DataModel.Themes;

using Iis.ThemeManagement;
using Iis.ThemeManagement.Models;
using Iis.Roles;

namespace Iis.UnitTests.ThemeManagement
{
    public class ThemeServiceTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public ThemeServiceTests()
        {
            _serviceProvider = Utils.GetServiceProvider();
        }
        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            context.ThemeTypes.RemoveRange(context.ThemeTypes);
            context.Themes.RemoveRange(context.Themes);
            context.SaveChanges();

            _serviceProvider.Dispose();
        }

        [Theory(DisplayName = "Get ThemeType list"), RecursiveAutoData]
        public async Task GetThemeTypes(
            ThemeTypeEntity themeType1,
            ThemeTypeEntity themeType2,
            ThemeTypeEntity themeType3)
        {
            //arrange: begin
            var service = _serviceProvider.GetRequiredService<ThemeService>();
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            context.ThemeTypes.Add(themeType1);
            context.ThemeTypes.Add(themeType2);
            context.ThemeTypes.Add(themeType3);

            context.SaveChanges();
            var themeTypeIds = new List<Guid>
            {
                themeType1.Id,
                themeType2.Id,
                themeType3.Id,
            };
            //arrange: end

            var themeTypesList = await service.GetThemeTypesAsync();

            Assert.Equal(3, themeTypesList.Count());
            Assert.Equal(themeTypeIds, themeTypesList.Select(e => e.Id));
        }

        [Theory(DisplayName = "Create Theme"), RecursiveAutoData]
        public async Task CreateTheme(
            Theme theme)
        {
            //arrange: begin
            var service = _serviceProvider.GetRequiredService<ThemeService>();
            //arrange: end

            var themeId = await service.CreateThemeAsync(theme);

            Assert.Equal(theme.Id, themeId);
        }

        [Theory, RecursiveAutoData]
        public async Task UpdateTheme_UsernameAndTypeEmpty_ExistingValuesUnchanged(ThemeEntity theme)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Themes.Add(theme);
            context.SaveChanges();

            //act
            var service = _serviceProvider.GetRequiredService<ThemeService>();
            await service.UpdateThemeAsync(new Theme {
                Title = "Updated",
                Query = theme.Query,
                Id = theme.Id
            });

            //assert
            var result = await service.GetThemeAsync(theme.Id);
            Assert.Equal("Updated", result.Title);
            Assert.Equal(theme.Type.Title, result.Type.Title);
            Assert.Equal(theme.User.Username, result.User.UserName);
        }

        [Theory, RecursiveAutoData]
        public async Task UpdateTheme_UsernameAndTypeProvided_ExistingValuesUnchanged(ThemeEntity theme,
            ThemeTypeEntity themeType, UserEntity user)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Themes.Add(theme);
            context.ThemeTypes.Add(themeType);
            context.Users.Add(user);
            context.SaveChanges();

            //act
            var service = _serviceProvider.GetRequiredService<ThemeService>();
            await service.UpdateThemeAsync(new Theme
            {
                Title = "Updated",
                Query = theme.Query,
                Id = theme.Id,
                Type = new ThemeType { Id = themeType.Id},
                User = new User { Id = user.Id}
            });

            //assert
            var result = await service.GetThemeAsync(theme.Id);
            Assert.Equal(themeType.Title, result.Type.Title);
            Assert.Equal(user.Username, result.User.UserName);
        }

        [Theory(DisplayName = "Delete Theme"), RecursiveAutoData]
        public async Task DeleteTheme(
            ThemeEntity themeEntity)
        {
            //arrange: begin
            var service = _serviceProvider.GetRequiredService<ThemeService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            context.Themes.Add(themeEntity);
            context.SaveChanges();

            //arrange: end
            var theme = await service.DeleteThemeAsync(themeEntity.Id);

            Assert.Equal(themeEntity.Id, theme.Id);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetThemeAsync(themeEntity.Id));

            Assert.Equal($"Theme does not exist for id = {themeEntity.Id}", exception.Message);
        }
    }
}