using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Iis.DataModel;
using Iis.Domain.Users;
using Iis.Elastic.SearchResult;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using Iis.Services;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Iis.UnitTests.Services
{
    public class AutocompleteUnitTests
    {
        private static readonly string[] ObjectOfStudyTypeList = new[] { "ObjectOfStudy" };
        private readonly Mock<IOntologySchema> _ontologySchemaMock = new Mock<IOntologySchema>();
        private readonly Mock<IElasticService> _elasticServiceMock = new Mock<IElasticService>();
        private readonly Mock<IElasticManager> _elasticManagerMock = new Mock<IElasticManager>();

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

        private AutocompleteService GetService()
        {
            return new AutocompleteService(_ontologySchemaMock.Object, _elasticServiceMock.Object, _elasticManagerMock.Object);
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
                { "person.userInfo.firstname", new SchemaNodeType() { Kind = Kind.Attribute } },
                { "person.userInfo.lastname", new SchemaNodeType() { Kind = Kind.Attribute } },
                { "person.userInfo", new SchemaNodeType() { Kind = Kind.Relation } },
            });

            _ontologySchemaMock
                .Setup(x => x.GetEntityTypesByName(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns(new List<INodeTypeLinked>
                {
                    new SchemaNodeType { Kind = Kind.Entity, Name = "Person" },
                    new SchemaNodeType { Kind = Kind.Entity, Name = "MilitaryOrganization" }
                });

            _elasticServiceMock
                .Setup(x => x.TypesAreSupported(It.IsAny<string[]>()))
                .Returns(true);

            _elasticServiceMock
                .Setup(x => x.SearchByFieldsAsync("*some text*", It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
                            ""title"" : ""1 армійський корпус""
                         }")
                    }
                });

            _elasticServiceMock
                .Setup(x => x.SearchByFieldsAsync("*rpg*", It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ElasticSearchResultItem>()
                {
                    new ElasticSearchResultItem
                    {
                        Identifier = Guid.NewGuid().ToString("N"),
                        SearchResult = JObject.Parse(@"{
                            ""NodeTypeName"" : ""Ammunition"",
                            ""NodeTypeTitle"" : ""Озброєння"",
                            ""title"" : ["""",""RPG-5""],
                            ""__title"" : ""RPG-5""
                         }")
                    },
                    new ElasticSearchResultItem
                    {
                        Identifier = Guid.NewGuid().ToString("N"),
                        SearchResult = JObject.Parse(@"{
                            ""NodeTypeName"" : ""Ammunition"",
                            ""NodeTypeTitle"" : ""Озброєння"",
                            ""title"" : ["""",""RPG-4""],
                            ""commonInfo"" : {
                                ""RealNameShort"" : ""RPG-4""
                                }
                         }")
                    }
                });
        }
    }
}
