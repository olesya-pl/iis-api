using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Elastic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
using Iis.Services.Contracts.Interfaces;

namespace Iis.UnitTests.Materials
{


    public class MaterialRepositoryTests : IDisposable
    {
        private ServiceProvider _serviceProvider;
        private static readonly MaterialSignEntity ProcessedSign = CreateProcessedMaterialSign();

        public MaterialRepositoryTests() { }

        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Materials.RemoveRange(context.Materials);
            context.SaveChanges();

            _serviceProvider.Dispose();
        }

        [Theory, RecursiveAutoData]
        public async Task PutDocumentToElasticSearch_PutsProcessedMaterialsCount(MaterialEntity materialEntity,
            List<MLResponseEntity> responses)
        {
            var materialId = materialEntity.Id;
            materialEntity.Data = null;
            materialEntity.Metadata = null;
            materialEntity.LoadData = null;
            materialEntity.MaterialInfos = null;
            materialEntity.MaterialAssignees = new List<MaterialAssigneeEntity>();

            responses.First().MaterialId = materialId;

            var mlRepoMock = new Mock<IMLResponseRepository>();
            mlRepoMock.Setup(e => e.GetAllForMaterialAsync(materialId))
                .ReturnsAsync(responses);
            mlRepoMock.Setup(e => e.GetAllForMaterialListAsync(It.IsAny<IReadOnlyCollection<Guid>>()))
                .ReturnsAsync(responses);

            var signRepoMock = new Mock<IMaterialSignRepository>();

            signRepoMock.Setup(_ => _.GetById(MaterialEntity.ProcessingStatusProcessedSignId))
                .Returns(ProcessedSign);

            var expectedResponsesCount = responses.Count(e => e.MaterialId == materialId);

            var elasticManagerMock = new Mock<IElasticManager>();

            _serviceProvider = Utils.GetServiceProviderWithCustomSetup(
                serviceCollection =>
                {
                    serviceCollection.AddTransient<IMLResponseRepository>(_ => mlRepoMock.Object);
                    serviceCollection.AddTransient<IElasticManager>(_ => elasticManagerMock.Object);
                    serviceCollection.AddTransient<IMaterialSignRepository>(_ => signRepoMock.Object);
                }) as ServiceProvider;

            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            await context.Materials.AddAsync(materialEntity);
            await context.SaveChangesAsync();

            var materialElasticService = _serviceProvider.GetRequiredService<IMaterialElasticService>();
            var res = await materialElasticService.PutMaterialToElasticSearchAsync(materialId, CancellationToken.None);

            elasticManagerMock
                .Verify(
                    e => e.PutDocumentAsync("Materials",
                        materialId.ToString("N"),
                        It.Is<string>(s => JObject.Parse(s).Value<int>("ProcessedMlHandlersCount") == expectedResponsesCount),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        private static MaterialSignEntity CreateProcessedMaterialSign()
        {
            var singTypeId = new Guid("214ceeee-67d5-4692-a3b4-316007fa5d34");
            return new MaterialSignEntity
            {
                Id = MaterialEntity.ProcessingStatusProcessedSignId,
                MaterialSignTypeId = singTypeId,
                ShortTitle = "1",
                Title = "Оброблено",
                OrderNumber = 1,
                MaterialSignType = new MaterialSignTypeEntity
                {
                    Id = singTypeId,
                    Name = "ProcessedStatus",
                    Title = "Обробка"
                }
            };
        }
    }
}