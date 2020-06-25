using System.Linq;
using Iis.Elastic;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class ElasticMappingPropertyTests
    {
        [Fact]
        public void DotnameConstructor_CreatesNestedProperties()
        {
            var metadata = new ElasticMappingProperty("Metadata.features.PhoneNumber", ElasticMappingPropertyType.Keyword);
            Assert.Equal(ElasticMappingPropertyType.Nested, metadata.Type);
            Assert.Equal("Metadata", metadata.Name);
            var features = metadata.Properties.First(p => p.Name == "features");
            Assert.Equal(ElasticMappingPropertyType.Nested, features.Type);
            var phoneNumber = features.Properties.First(p => p.Name == "PhoneNumber");
            Assert.Equal(ElasticMappingPropertyType.Keyword, phoneNumber.Type);
        }
    }
}
