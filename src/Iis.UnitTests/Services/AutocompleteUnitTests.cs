using FluentAssertions;
using Iis.DataModel;
using Iis.Elastic;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using Iis.Services;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Iis.UnitTests.Services
{
    public class AutocompleteUnitTests
    {
        private readonly Mock<IOntologySchema> _ontologySchemaMock = new Mock<IOntologySchema>();
        private readonly Mock<IElasticService> _elasticServiceMock = new Mock<IElasticService>();

        public AutocompleteUnitTests()
        {
            Init();
        }

        [Fact]
        public void GetTips_GivenSomeFieldsAndAliases_ShouldReturn3Aliases()
        {
            //Arrange
            var service = GetService();

            //Act
            var tips = service.GetTips("al", 10);

            //Assert
            Assert.Equal(3, tips.Count);
        }

        [Fact]
        public void GetTips_GivenSomeFieldsAndAliases_ShouldReturn2Fields()
        {
            //Arrange
            var service = GetService();

            //Act
            var tips = service.GetTips("use", 10);

            //Assert
            Assert.Equal(2, tips.Count);
            Assert.DoesNotContain(tips, x => x.Contains("person"));
        }

        [Fact]
        public void GetTips_GivenSomeFieldsAndAliases_ShouldNotReturnMoreThen3Tips()
        {
            //Arrange
            var service = GetService();

            //Act
            var tips = service.GetTips("a", 3);

            //Assert
            Assert.Equal(3, tips.Count);
        }

        [Fact]
        public async Task GetEntities_GivenTwoEntities_ShouldReturnTwoValidAutocompleteEntity() 
        {
            //Arrange
            var service = GetService();

            //Act
            var entities = await service.GetEntitiesAsync("some text", 5);

            //Assert
            entities.Should().HaveCount(2);
            entities.Should().ContainSingle(x => x.Title == "1 армійський корпус");
            entities.Should().ContainSingle(x => x.Title == "1 АК");
            entities.Should().OnlyContain(x => x.TypeName == "MilitaryOrganization");
            entities.Should().OnlyContain(x => x.TypeTitle == "Військовий підрозділ");
        }

        private AutocompleteService GetService()
        {
            return new AutocompleteService(_ontologySchemaMock.Object, _elasticServiceMock.Object);
        }

        private void Init()
        {
            _ontologySchemaMock.Setup(x => x.Aliases.Items).Returns(new List<AliasEntity>
            {
                new AliasEntity
                {
                    Value = "Alias1"
                },
                new AliasEntity
                {
                    Value = "alias2"
                },
                new AliasEntity
                {
                    Value = "alias3"
                }

            });

            _ontologySchemaMock.Setup(x => x.GetFullHierarchyNodes()).Returns(new Dictionary<string, INodeTypeLinked>
            {
                { "person.userInfo.firstname", new SchemaNodeType() { Kind = Kind.Attribute} },
                { "person.userInfo.lastname", new SchemaNodeType() { Kind = Kind.Attribute} },
                { "person.userInfo", new SchemaNodeType() { Kind = Kind.Relation} },
            });

            _elasticServiceMock
                .Setup(x => x.SearchByFieldsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ElasticSearchResultItem>()
                {
                    new ElasticSearchResultItem
                    {
                        Identifier = Guid.NewGuid().ToString("N"),
                        SearchResult = JObject.Parse(@"{
                            ""NodeTypeName"" : ""MilitaryOrganization"",
                            ""NodeTypeTitle"" : ""Військовий підрозділ"",
                            ""title"" : """",
                            ""commonInfo"" : {
                                ""RealNameShort"" : ""1 АК""
                                }
                         }")
                    },
                    new ElasticSearchResultItem
                    {
                        Identifier = Guid.NewGuid().ToString("N"),
                         SearchResult = JObject.Parse(@"{
                            ""NodeTypeName"" : ""MilitaryOrganization"",
                            ""NodeTypeTitle"" : ""Військовий підрозділ"",
                            ""title"" : ""1 армійський корпус"",
                            ""commonInfo"" : {
                                ""RealNameShort"" : ""1 АК""
                                }
                         }")
                    }
                }); ;
        }
    }
}
