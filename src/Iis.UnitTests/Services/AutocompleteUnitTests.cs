using System;
using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using Iis.Services;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Iis.UnitTests.Services
{
    public class AutocompleteUnitTests
    {
        private readonly Mock<IOntologySchema> _ontologySchemaMock = new Mock<IOntologySchema>();

        public AutocompleteUnitTests()
        {
            Init();
        }

        [Fact]
        public void GetTips_GivenSomeFieldsAndAliases_ShouldReturn3Aliases()
        {
            //Arrange
            var service = new AutocompleteService(_ontologySchemaMock.Object);

            //Act
            var tips = service.GetTips("al", 10);

            //Assert
            Assert.Equal(3, tips.Count);
        }

        [Fact]
        public void GetTips_GivenSomeFieldsAndAliases_ShouldReturn2Fields()
        {
            //Arrange
            var service = new AutocompleteService(_ontologySchemaMock.Object);

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
            var service = new AutocompleteService(_ontologySchemaMock.Object);

            //Act
            var tips = service.GetTips("a", 3);

            //Assert
            Assert.Equal(3, tips.Count);
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
        }
    }
}
