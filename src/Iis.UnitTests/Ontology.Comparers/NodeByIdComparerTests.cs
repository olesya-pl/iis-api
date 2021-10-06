using System;
using Xunit;
using FluentAssertions;
using Iis.OntologyData.DataTypes;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Comparers;

namespace Iis.UnitTests.Ontology.Comparers
{
    public class NodeByIdComparerTests
    {
        [Fact]
        public void EqualsNullAndNull()
        {
            var comparer = new NodeByIdComparer();

            comparer.Equals(null, null).Should().BeTrue();
        }

        [Fact]
        public void EqualsNullAndNotNull()
        {
            var comparer = new NodeByIdComparer();

            comparer.Equals(null, new NodeData { Id = Guid.NewGuid() }).Should().BeFalse();
        }

        [Fact]
        public void EqualsNotNullAndNull()
        {
            var comparer = new NodeByIdComparer();

            comparer.Equals(new NodeData { Id = Guid.NewGuid() }, null).Should().BeFalse();
        }

        [Fact]
        public void EqualNotNullWithSameId()
        {
            var comparer = new NodeByIdComparer();

            var id = Guid.NewGuid();

            comparer.Equals(new NodeData { Id = id }, new NodeData { Id = id }).Should().BeTrue();
        }

        [Fact]
        public void EqualNotNullWithDifferentId()
        {
            var comparer = new NodeByIdComparer();

            comparer.Equals(new NodeData { Id = Guid.NewGuid() }, new NodeData { Id = Guid.NewGuid() }).Should().BeFalse();
        }
    }
}