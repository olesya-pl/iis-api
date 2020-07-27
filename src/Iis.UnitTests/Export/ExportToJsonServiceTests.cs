using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Iis.Api.Export;
using Iis.DbLayer.Repositories;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Iis.UnitTests.Export
{
    public class ExportToJsonServiceTests
    {
        [Theory, AutoMoqData]
        public async Task ExportNode_SuccessPath(Guid nodeId, JObject output,
            [Frozen] Mock<INodeRepository> nodeServiceMock,
            ExportToJsonService sut)
        {
            nodeServiceMock.Setup(p => p.GetJsonNodeByIdAsync(nodeId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(output);

            var res = await sut.ExportNodeAsync(nodeId);

            Assert.Equal(output.ToString(), res);
        }
    }
}
