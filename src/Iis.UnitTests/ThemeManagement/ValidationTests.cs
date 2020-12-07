using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

using IIS.Core.GraphQL.Themes;

namespace Iis.UnitTests.ThemeManagement
{
    public class ValidationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public ValidationTests()
        {
            
        }

        public void Dispose()
        {

        }

        [Theory(DisplayName = "Checks validation attributes: Title."), RecursiveAutoData]
        public void CheckValidationTitle(ThemeInput theme)
        {
            theme.Title = null;

            var exception = Assert.Throws<ValidationException>(() => Validator.ValidateObject(theme, new ValidationContext(theme), true));

            Assert.Equal("The Title field is required.", exception.Message);
        }

        [Theory(DisplayName = "Checks validation attributes: QueryRequest."), RecursiveAutoData]
        public void CheckValidationQuery(ThemeInput theme)
        {
            theme.QueryRequest = null;

            var exception = Assert.Throws<ValidationException>(() => Validator.ValidateObject(theme, new ValidationContext(theme), true));

            Assert.Equal("The QueryRequest field is required.", exception.Message);
        }

        [Theory(DisplayName = "Checks validation attributes: UserId."), RecursiveAutoData]
        public void CheckValidationUserId(ThemeInput theme)
        {
            theme.UserId = null;

            var exception = Assert.Throws<ValidationException>(() => Validator.ValidateObject(theme, new ValidationContext(theme), true));

            Assert.Equal("The UserId field is required.", exception.Message);
        }

        [Theory(DisplayName = "Checks validation attributes: EntityTypeName."), RecursiveAutoData]
        public void CheckValidationEntityTypeName(ThemeInput theme)
        {
            theme.EntityTypeName = null;

            var exception = Assert.Throws<ValidationException>(() => Validator.ValidateObject(theme, new ValidationContext(theme), true));

            Assert.Equal("The EntityTypeName field is required.", exception.Message);
        }
    }
}