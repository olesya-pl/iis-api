using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Xunit2;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Ontology.Schema;
using Iis.Domain.Graph;
using Iis.Domain.Materials;
using Iis.Services.Mappers.Graph;

namespace Iis.UnitTests.Services.Mappers.GraphMapperTests
{
    public class MapNodeToGraphNodeTests
    {
        [Fact]
        public void Should_ReturnNull_WhenNodeIsNull()
        {
            GraphTypeMapper.MapNodeToGraphNode(null, Array.Empty<Guid>())
            .Should().BeNull();
        }
    }
}