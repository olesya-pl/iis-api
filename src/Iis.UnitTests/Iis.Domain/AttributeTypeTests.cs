using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Xunit;

namespace Iis.UnitTests.Iis.Domain
{
    public class AttributeTypeTests
    {
        [Fact]
        public void ParseAttributeValue_FloatRange_ReturnsSameValue()
        {
            var res = AttributeType.ParseValue("1-112", ScalarType.FloatRange);
            Assert.Equal("1-112", res);
        }

        [Fact]
        public void ParseAttributeValue_IntRange_ReturnsSameValue()
        {
            var res = AttributeType.ParseValue("1-112", ScalarType.IntegerRange);
            Assert.Equal("1-112", res);
        }
    }
}
