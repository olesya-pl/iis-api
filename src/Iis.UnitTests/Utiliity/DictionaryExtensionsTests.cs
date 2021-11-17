using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Iis.Utility;

namespace Iis.UnitTests.Utiliity
{
    public class DictionaryExtensionsTests    
    {
        [Theory]
        [AutoMoqData]
        public void DestinationEmpty_ResultEqualNewCollection(IDictionary<int, string> newCollection)
        {
            var actual = new Dictionary<int, string>();

            actual.TryAddRange(newCollection);

            actual.Should().Equal(newCollection);
        }

        [Fact]
        public void DestinationNull_ResultNull()
        {
            IDictionary<int, string> actual = null;

            IDictionary<int, string> newCollection = new Dictionary<int, string>
            {
                [4] = "value_4"
            };

            actual.TryAddRange(newCollection);
        }

        [Fact]
        public void CollectionNull_ResultDoesntChange()
        {
            var actual = new Dictionary<int, string>
            {
                [1] = "value_1",
                [2] = "value_2",
                [3] = "value_3"
            };

            var expected = new Dictionary<int, string>
            {
                [1] = "value_1",
                [2] = "value_2",
                [3] = "value_3"
            };

            actual.TryAddRange(null);

            actual.Should().Equal(expected);
        }

        [Fact]
        public void CollectionEmpty_ResultDoesntChange()
        {
            var actual = new Dictionary<int, string>
            {
                [1] = "value_1",
                [2] = "value_2",
                [3] = "value_3"
            };

            var expected = new Dictionary<int, string>
            {
                [1] = "value_1",
                [2] = "value_2",
                [3] = "value_3"
            };


            actual.TryAddRange(new Dictionary<int, string>());

            actual.Should().Equal(expected);
        }

        [Fact]
        public void CollectionAdded_ResultIncreased()
        {
            var actual = new Dictionary<int, string>
            {
                [1] = "value_1",
                [2] = "value_2",
                [3] = "value_3"
            };

            var newCollection = new Dictionary<int, string>
            {
                [4] = "value_4"
            };

            var expected = new Dictionary<int, string>
            {
                [1] = "value_1",
                [2] = "value_2",
                [3] = "value_3",
                [4] = "value_4"
            };

            actual.TryAddRange(newCollection);

            actual.Should().Equal(expected);
        }

        [Fact]
        public void CollectionAdded_ResultIncreasedButSkippedDuplicateKey()
        {
            var actual = new Dictionary<int, string>
            {
                [1] = "value_1",
                [2] = "value_2",
                [3] = "value_3"
            };

            var newCollection = new Dictionary<int, string>
            {
                [1] = "value_1",
                [4] = "value_4"
            };

            var expected = new Dictionary<int, string>
            {
                [1] = "value_1",
                [2] = "value_2",
                [3] = "value_3",
                [4] = "value_4"
            };

            actual.TryAddRange(newCollection);

            actual.Should().Equal(expected);
        }
    }
}
