using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Moq;
using AutoMapper;
using FluentAssertions;
using Iis.MaterialDistributor.Contracts.Repositories;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Services;
using Iis.MaterialDistributor.DataModel.Entities;

namespace Iis.UnitTests.MaterialDistributor
{
    public class VariableCoefficientServiceTests
    {
        private readonly List<VariableCoefficientEntity> _coefficientEntityCollection;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IVariableCoefficientRepository> _repositoryMock;

        public VariableCoefficientServiceTests()
        {
            _coefficientEntityCollection = new List<VariableCoefficientEntity>();

            _mapperMock = new Mock<IMapper>();
            _mapperMock
                .Setup(_ => _.Map<VariableCoefficient>(It.IsAny<VariableCoefficientEntity>()))
                .Returns<VariableCoefficientEntity>(_ => new VariableCoefficient { Coefficient = _.Coefficient, OffsetHours = _.OffsetHours });

            _repositoryMock = new Mock<IVariableCoefficientRepository>();
            _repositoryMock
                .Setup(_ => _.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetVariableCoefficientEntities);
        }

        [Fact]
        public async Task GetVariableСoefficientWithMaxOffsetHoursAsync_Success()
        {
            _coefficientEntityCollection.Add(new VariableCoefficientEntity { Id = Guid.NewGuid(), OffsetHours = 0, Coefficient = 100 });
            _coefficientEntityCollection.Add(new VariableCoefficientEntity { Id = Guid.NewGuid(), OffsetHours = 5, Coefficient = 75 });
            _coefficientEntityCollection.Add(new VariableCoefficientEntity { Id = Guid.NewGuid(), OffsetHours = 10, Coefficient = 50 });
            _coefficientEntityCollection.Add(new VariableCoefficientEntity { Id = Guid.NewGuid(), OffsetHours = 15, Coefficient = 10 });

            var service = new VariableCoefficientService(_repositoryMock.Object, new VariableCoefficientRuleEvaluator(), _mapperMock.Object);

            var result = await service.GetWithMaxOffsetHoursAsync(CancellationToken.None);

            result.Should().NotBeNull();
            result.Coefficient.Should().Be(10);
            result.OffsetHours.Should().Be(15);
        }

        [Theory]
        [InlineData(16, 14, 100)]
        [InlineData(16, 00, 100)]
        [InlineData(15, 15, 100)]
        [InlineData(15, 00, 100)]
        [InlineData(14, 35, 100)]
        [InlineData(13, 45, 100)]
        [InlineData(12, 15, 100)]
        [InlineData(11, 15, 75)]
        [InlineData(10, 25, 75)]
        [InlineData(09, 35, 75)]
        [InlineData(08, 45, 75)]
        [InlineData(07, 55, 75)]
        [InlineData(06, 15, 50)]
        [InlineData(05, 15, 50)]
        [InlineData(01, 15, 10)]
        [InlineData(00, 50, 10)]
        [InlineData(00, 10, 0)]
        [InlineData(00, 01, 0)]
        public async Task SetVariableСoefficientForMaterialCollectionAsync_Success(int hour, int minute, int expected)
        {
            _coefficientEntityCollection.Add(new VariableCoefficientEntity { Id = Guid.NewGuid(), OffsetHours = 0, Coefficient = 100 });
            _coefficientEntityCollection.Add(new VariableCoefficientEntity { Id = Guid.NewGuid(), OffsetHours = 5, Coefficient = 75 });
            _coefficientEntityCollection.Add(new VariableCoefficientEntity { Id = Guid.NewGuid(), OffsetHours = 10, Coefficient = 50 });
            _coefficientEntityCollection.Add(new VariableCoefficientEntity { Id = Guid.NewGuid(), OffsetHours = 15, Coefficient = 10 });

            var comparisonTimeStamp = new DateTime(2021, 01, 01, 16, 15, 25);

            var document = new MaterialDocument
            {
                Id = Guid.NewGuid(),
                RegistrationDate = new DateTime(2021, 01, 01, hour, minute, 25),
                CreatedDate = DateTime.UtcNow
            };

            var documentCollection = new[] { document };

            var service = new VariableCoefficientService(_repositoryMock.Object, new VariableCoefficientRuleEvaluator(), _mapperMock.Object);

            var collection = await service.SetForMaterialsAsync(
                comparisonTimeStamp,
                documentCollection,
                CancellationToken.None);

            collection.FirstOrDefault()
                .VariableCoefficient.Should().Be(expected);
        }

        private VariableCoefficientEntity[] GetVariableCoefficientEntities()
        {
            return _coefficientEntityCollection.ToArray();
        }
    }
}