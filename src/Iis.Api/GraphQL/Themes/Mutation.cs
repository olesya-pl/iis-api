using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

using HotChocolate;
using HotChocolate.Types;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

using IIS.Core.GraphQL.Users;

namespace IIS.Core.GraphQL.Themes
{
    public class Mutation
    {
        public async Task<Theme> CreateTheme(
            [GraphQLNonNullType] ThemeInput themeInput)
        {
            Validator.ValidateObject(themeInput, new ValidationContext(themeInput), true);

            var user = new User
            {
                Id = themeInput.UserId.Value,
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
                Title = themeInput.Query,
                Query = themeInput.Query,
                User = user,
                Type = materialType
            };

            return await Task.FromResult(theme);
        }

        public async Task<Theme> DeleteTheme(
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
    } 
}