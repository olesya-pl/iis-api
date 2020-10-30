using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Iis.UnitTests.Materials
{
    
    
    public class MaterialRepositoryTests : IDisposable
    {
        private ServiceProvider _serviceProvider;

        public MaterialRepositoryTests()
        {
            
        }
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

            var mlRepoMock = new Mock<IMLResponseRepository>();
            mlRepoMock.Setup(e => e.GetAllForMaterialAsync(materialId))
                .ReturnsAsync(responses);

            var elasticManagerMock = new Mock<IElasticManager>();
            

            _serviceProvider = Utils.GetServiceProviderWithCustomSetup(
                serviceCollection =>
                {
                    serviceCollection.AddTransient<IMLResponseRepository>(_ => mlRepoMock.Object);
                    serviceCollection.AddTransient<IElasticManager>(_ => elasticManagerMock.Object);
                }) as ServiceProvider;

            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            await context.Materials.AddAsync(materialEntity);
            await context.SaveChangesAsync();

            var sut = _serviceProvider.GetRequiredService<IMaterialRepository>() as MaterialRepository;
            sut.SetContext(context);
            
            
            var res = await sut.PutMaterialToElasticSearchAsync(materialId, CancellationToken.None);

            elasticManagerMock
                .Verify(
                    e => e.PutDocumentAsync("Materials", 
                        materialId.ToString("N"), 
                        It.Is<string>(s => JObject.Parse(s).Value<int>("ProcessedMlHandlersCount") == responses.Count), 
                        It.IsAny<CancellationToken>()), 
                    Times.Once);
        } 
    }
}
