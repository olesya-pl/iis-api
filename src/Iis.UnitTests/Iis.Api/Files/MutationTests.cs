using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using Iis.Api.Configuration;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using IIS.Core;
using IIS.Core.GraphQL.Files;
using IIS.Core.Materials;
using Moq;
using Xunit;

namespace Iis.UnitTests.Iis.Api.Files
{
    public class MutationTests
    {
        [Theory]
        [InlineData(".mp4")]
        [InlineData(".docx")]
        [InlineData(".pdf")]
        public async Task UploadFile_IsDuplicate_ReturnsErrorMessage(string extension)
        {
            var user = new User() {
                Id = Guid.NewGuid()
            };
            
            var contextMock = new Mock<IResolverContext>();
            var contextData = new Dictionary<string, object>() {
                {"token", new TokenPayload(user.Id, user) }
            };
            contextMock.Setup(e => e.ContextData).Returns(contextData);

            var fileSerivceMock = new Mock<IFileService>();
            fileSerivceMock.Setup(e => e.IsDuplicatedAsync(It.IsAny<byte[]>())).ReturnsAsync(new FileIdDto { 
                IsDuplicate = true
            });
            
            var sut = new Mutation();
            var result = await sut.Upload(new UploadConfiguration(),
                contextMock.Object,
                fileSerivceMock.Object,
                new Mock<IMaterialService>().Object,
                new[] { new UploadInput {
                    AccessLevel = 0,
                    Content = new byte [] {14, 88},
                    Name = $"testFile{extension}"
                } });

            Assert.False(result.First().Success);
            Assert.Equal("Даний файл вже завантажений до системи", result.First().Message);
        } 
    }
}
