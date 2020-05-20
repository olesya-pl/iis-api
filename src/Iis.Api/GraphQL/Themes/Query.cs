using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;

using IIS.Core.GraphQL.Users;
using IIS.Core.GraphQL.Common;

using ThemeMng = Iis.ThemeManagement;

namespace IIS.Core.GraphQL.Themes
{
    public class Query
    {
        public async Task<GraphQLCollection<Theme>> GetThemesForUser(
            [GraphQLType(typeof(NonNullType<IdType>))] Guid userId)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "test user"
            };
            var materialType = new ThemeType
            {
                Id = new Guid("2b8fd109-cf4a-4f76-8136-de761da53d20"),
                ShortTitle = "M",
                Title = "Матеріал"
            };
            var objectType = new ThemeType
            {
                Id = new Guid("043ae699-e070-4336-8513-e90c87555c58"),
                ShortTitle = "O",
                Title = "Об'єкт"
            };

            var list = new List<Theme>
            {
                new Theme
                {
                    Id = Guid.NewGuid(),
                    QueryResults = 122,
                    Title = "телефон:1234567",
                    Query = "телефон:1234567",
                    User = user,
                    Type = materialType
                },
                new Theme
                {
                    Id = Guid.NewGuid(),
                    QueryResults = 122,
                    Title = "войска РВСН",
                    Query = "войска РВСН",
                    User = user,
                    Type = objectType
                },
                new Theme
                {
                    Id = Guid.NewGuid(),
                    QueryResults = 122,
                    Title = "Чекмизов",
                    Query = "Чекмизов",
                    User = user,
                    Type = objectType
                }
            };

            return await Task.FromResult(new GraphQLCollection<Theme>(list, list.Count));
        }
        public async Task<Theme> GetTheme(
            [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "test user"
            };
            var materialType = new ThemeType
            {
                Id = new Guid("2b8fd109-cf4a-4f76-8136-de761da53d20"),
                ShortTitle = "M",
                Title = "Матеріал"
            };
            var theme = new Theme
            {
                Id = Guid.NewGuid(),
                QueryResults = 122,
                Title = "телефон:1234567",
                Query = "телефон:1234567",
                User = user,
                Type = materialType
            };
            return await Task.FromResult(theme);
        }

        public async Task<GraphQLCollection<ThemeType>> GetThemeTypes(
            [Service] ThemeMng.ThemeService themeService,
            [Service] IMapper mapper)
        {
            var themeTypes = await themeService.GetThemeTypesAsync();

            var graphThemeTypes = mapper.Map<List<ThemeType>>(themeTypes);

            return await Task.FromResult(new GraphQLCollection<ThemeType>(graphThemeTypes, graphThemeTypes.Count));
        }
    }
}