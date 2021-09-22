using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Iis.Elastic.SearchResult;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.Ontology.EntityFramework;
using Moq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class SearchEntitiesByConfiguredFieldsAsyncTests
    {
        [Theory, AutoData]
        public async Task SearchEntitiesByConfiguredFieldsAsync_ReturnsNoAggregations(string indexName, int from, int limit, Guid userId)
        {
            var elasticServiceMock = new Mock<IElasticManager>();
            var elasticConfigurationMock = new Mock<IElasticConfiguration>();
            elasticConfigurationMock
                .Setup(e => e.GetOntologyIncludedFields(It.IsAny<IEnumerable<string>>()))
                .Returns(new List<IIisElasticField>());

            elasticServiceMock.Setup(e => e.WithUserId(userId)).Returns(elasticServiceMock.Object);
                
            var ontologySchemaMock = new Mock<IOntologySchema>();
            var nodeRepositoryMock = new Mock<INodeSaveService>();

            var resultMock = new Mock<IElasticSearchResult>();
            resultMock.Setup(e => e.Aggregations).Returns(new Dictionary<string, AggregationItem>());
            resultMock.Setup(e => e.Count).Returns(0);
            resultMock.Setup(e => e.Items).Returns(Array.Empty<ElasticSearchResultItem>());

            elasticServiceMock
                .Setup(e => e.SearchAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultMock.Object);

            elasticServiceMock
                .Setup(e => e.SearchAsync(It.IsAny<IIisElasticSearchParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultMock.Object);
            
            var sut = new ElasticService(
                elasticServiceMock.Object,
                elasticConfigurationMock.Object,
                nodeRepositoryMock.Object,
                new ElasticState(ontologySchemaMock.Object));

            var res = await sut.SearchEntitiesByConfiguredFieldsAsync(new[] { indexName }, new ElasticFilter
            {
                Suggestion = "!",
                Offset = from,
                Limit = limit                
            }, userId);

            Assert.Equal(0, res.Count);
        }
    }
}
