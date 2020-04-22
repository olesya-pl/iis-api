using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Iis.Api.Export;
using Iis.Elastic;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Moq;
using Xunit;

namespace Iis.UnitTests.Export
{
    public class ExportToJsonServiceTests
    {
        [Theory, AutoMoqData]
        public async Task ExportNode_SuccessPath(Guid nodeId, IExtNode node, string output,
            [Frozen] Mock<IExtNodeService> nodeServiceMock,
            [Frozen] Mock<IElasticSerializer> serializerMock,
            ExportToJsonService sut)
        {
            nodeServiceMock.Setup(p => p.GetExtNodeByIdAsync(nodeId, default(CancellationToken)))
                    .ReturnsAsync(node);
            serializerMock.Setup(p => p.GetJsonByExtNode(node)).Returns(output);

            var res = await sut.ExportNodeAsync(nodeId);

            Assert.Equal(output, res);
        }
    }
}
