using IIS.Core.GraphQL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Iis.UnitTests.Iis.Api.GraphQL.Entities
{
    public class AggregationNodeTypeTreeTests
    {
        [Fact]
        public void MergeBucket_Null()
        {
            var items = new List<AggregationNodeTypeItem>
            {
                new AggregationNodeTypeItem { NodeTypeName = "nt1" },
                new AggregationNodeTypeItem { NodeTypeName = "nt2" },
            };
            var list = new AggregationNodeTypeTree(items);
            list.MergeBuckets(null);
            Assert.Empty(list.Items);
        }
        [Fact]
        public void MergeBucket_Empty()
        {
            var items = new List<AggregationNodeTypeItem>
            {
                new AggregationNodeTypeItem { NodeTypeName = "nt1" },
                new AggregationNodeTypeItem { NodeTypeName = "nt2" },
            };
            var list = new AggregationNodeTypeTree(items);
            list.MergeBuckets(new List<AggregationBucket>());
            Assert.Empty(list.Items);
        }
        [Fact]
        public void MergeBucket_OneLevel()
        {
            var items = new List<AggregationNodeTypeItem>
            {
                new AggregationNodeTypeItem { NodeTypeName = "nt1" },
                new AggregationNodeTypeItem { NodeTypeName = "nt2" },
                new AggregationNodeTypeItem { NodeTypeName = "nt3" },
            };
            var list = new AggregationNodeTypeTree(items);
            var buckets = new List<AggregationBucket>
            {
                new AggregationBucket { TypeName = "nt1", DocCount = 10 },
                new AggregationBucket { TypeName = "nt3", DocCount = 20 }
            };
            list.MergeBuckets(buckets);
            Assert.Equal(2, list.Items.Count);

            Assert.Equal(10, list.Items[0].DocCount);
            Assert.Equal("nt1", list.Items[0].NodeTypeName);

            Assert.Equal(20, list.Items[1].DocCount);
            Assert.Equal("nt3", list.Items[1].NodeTypeName);
        }
        [Fact]
        public void MergeBucket_Deep1()
        {
            var items = new List<AggregationNodeTypeItem>
            {
                new AggregationNodeTypeItem 
                { 
                    NodeTypeName = "nt1",
                    Children = new List<AggregationNodeTypeItem>
                    {
                        new AggregationNodeTypeItem
                        {
                            NodeTypeName = "nt1_1", 
                            Children = new List<AggregationNodeTypeItem>
                            {
                                new AggregationNodeTypeItem { NodeTypeName = "nt1_1_1" },
                                new AggregationNodeTypeItem { NodeTypeName = "nt1_1_2" },
                            }
                        },
                        new AggregationNodeTypeItem
                        {
                            NodeTypeName = "nt1_2",
                            Children = new List<AggregationNodeTypeItem>
                            {
                                new AggregationNodeTypeItem { NodeTypeName = "nt1_2_1" },
                                new AggregationNodeTypeItem { NodeTypeName = "nt1_2_2" },
                            }
                        },
                        new AggregationNodeTypeItem
                        {
                            NodeTypeName = "nt1_3",
                            Children = new List<AggregationNodeTypeItem>
                            {
                                new AggregationNodeTypeItem { NodeTypeName = "nt1_3_1" },
                                new AggregationNodeTypeItem { NodeTypeName = "nt1_3_2" },
                            }
                        }
                    }
                },
                new AggregationNodeTypeItem { NodeTypeName = "nt2" },
                new AggregationNodeTypeItem { NodeTypeName = "nt3" },
            };
            var list = new AggregationNodeTypeTree(items);
            var buckets = new List<AggregationBucket>
            {
                new AggregationBucket { TypeName = "nt1_1_1", DocCount = 3 },
                new AggregationBucket { TypeName = "nt1_1_2", DocCount = 5 },
                new AggregationBucket { TypeName = "nt1_3_2", DocCount = 8 },
                new AggregationBucket { TypeName = "nt1_1", DocCount = 2 },
            };
            list.MergeBuckets(buckets);
            Assert.Single(list.Items);

            Assert.Equal(18, list.Items[0].DocCount);
            Assert.Equal("nt1", list.Items[0].NodeTypeName);

            Assert.Equal(2, list.Items[0].Children.Count);

            Assert.Equal(10, list.Items[0].Children[0].DocCount);
            Assert.Equal("nt1_1", list.Items[0].Children[0].NodeTypeName);

            Assert.Equal(8, list.Items[0].Children[1].DocCount);
            Assert.Equal("nt1_3", list.Items[0].Children[1].NodeTypeName);

            Assert.Equal(2, list.Items[0].Children[0].Children.Count);

            Assert.Equal(3, list.Items[0].Children[0].Children[0].DocCount);
            Assert.Equal("nt1_1_1", list.Items[0].Children[0].Children[0].NodeTypeName);

            Assert.Equal(5, list.Items[0].Children[0].Children[1].DocCount);
            Assert.Equal("nt1_1_2", list.Items[0].Children[0].Children[1].NodeTypeName);

            Assert.Single(list.Items[0].Children[1].Children);

            Assert.Equal(8, list.Items[0].Children[1].Children[0].DocCount);
            Assert.Equal("nt1_3_2", list.Items[0].Children[1].Children[0].NodeTypeName);   
        }
    }
}
