using Xunit;
using System;
using System.Collections.Generic;

using Iis.Elastic;
using Iis.Domain.ExtendedData;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class ElasticSerializerTests
    {
        [Fact]
        public void GetJsonObjectByExtNodeTest_NoChildren()
        {
            var serializer = new ElasticSerializer();
            var extNode = new ExtNode
            {
                Id = "a01",
                NodeTypeName = "testEntity",
                CreatedAt = new DateTime(2020, 1, 2),
                UpdatedAt = new DateTime(2020, 1, 3),
                AttributeValue = null
            };

            var json = serializer.GetJsonObjectByExtNode(extNode);
            Assert.Equal(extNode.Id, json["Id"]);
            Assert.Equal(extNode.NodeTypeName, json["NodeTypeName"]);
            Assert.Equal(extNode.CreatedAt, Convert.ToDateTime(json["CreatedAt"]));
            Assert.Equal(extNode.UpdatedAt, Convert.ToDateTime(json["UpdatedAt"]));
        }

        [Fact]
        public void GetJsonObjectByExtNodeTest_Children()
        {
            var serializer = new ElasticSerializer();
            var extNode = new ExtNode
            {
                Id = "b01",
                NodeTypeName = "testEntity",
                CreatedAt = new DateTime(2020, 1, 2),
                UpdatedAt = new DateTime(2020, 1, 3),
                AttributeValue = null,
                Children = new List<ExtNode>
                {
                    new ExtNode
                    {
                        NodeTypeName = "name", 
                        AttributeValue = "abcde",
                    },
                    new ExtNode
                    {
                        NodeTypeName = "affiliation",
                        AttributeValue = null,
                        Children = new List<ExtNode>
                        {
                            new ExtNode
                            {
                                NodeTypeName = "code",
                                AttributeValue = "pending"
                            },
                            new ExtNode
                            {
                                NodeTypeName = "sidc",
                                AttributeValue = "SPG"
                            }
                        }
                    }
                }
            };

            var json = serializer.GetJsonObjectByExtNode(extNode);
            Assert.Equal(extNode.Id, json["Id"]);
            Assert.Equal(extNode.NodeTypeName, json["NodeTypeName"]);
            Assert.Equal(extNode.CreatedAt, Convert.ToDateTime(json["CreatedAt"]));
            Assert.Equal(extNode.UpdatedAt, Convert.ToDateTime(json["UpdatedAt"]));

            Assert.Equal("abcde", json["name"]);
            var affiliation = json["affiliation"];
            Assert.Equal("pending", affiliation["code"]);
            Assert.Equal("SPG", affiliation["sidc"]);
            Assert.Null(affiliation["CreatedAt"]);
            Assert.Null(affiliation["UpdatedAt"]);
        }
    }
}
