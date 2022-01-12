using System;
using System.Collections.Generic;
using System.Text;
using Iis.Security.SecurityLevels;
using Xunit;

namespace Iis.UnitTests.Iis.Security.SecurityLevels
{
    public class SecurityLevelTests
    {
        [Fact]
        public void ConstructorTest_NoChildren()
        {
            var top = new SecurityLevel("top", 1);
            Assert.Equal("top", top.Name);
            Assert.Equal(1, top.UniqueIndex);
            Assert.Null(top.Parent);
            Assert.Empty(top.Children);
        }

        [Fact]
        public void ConstructorTest_WithChildren()
        {
            var top = new SecurityLevel(
                "top",
                1,
                new List<SecurityLevel>
                {
                    new SecurityLevel("item1", 2),
                    new SecurityLevel("item2", 3)
                });
            Assert.Equal("top", top.Name);
            Assert.Equal(1, top.UniqueIndex);
            Assert.Null(top.Parent);
            Assert.Equal(2, top.Children.Count);

            var item1 = top.Children[0];
            Assert.Equal("item1", item1.Name);
            Assert.Equal(2, item1.UniqueIndex);
            Assert.Equal(top, item1.Parent);
            Assert.Empty(item1.Children);

            var item2 = top.Children[1];
            Assert.Equal("item2", item2.Name);
            Assert.Equal(3, item2.UniqueIndex);
            Assert.Equal(top, item2.Parent);
            Assert.Empty(item2.Children);
        }
        [Fact]
        public void GetItemTest()
        {
            var top = SecurityLevelModels.GetTestModel1();
            var item2 = top.GetItem(2);
            var item111 = top.GetItem(111);
            Assert.Equal("top", top.Name);
            Assert.Equal("item2", item2.Name);
            Assert.Equal("item111", item111.Name);
        }
        [Fact]
        public void IsRootTest()
        {
            var top = SecurityLevelModels.GetTestModel1();
            var item2 = top.GetItem(2);
            var item11 = top.GetItem(11);
            Assert.True(top.IsRoot);
            Assert.False(item2.IsRoot);
            Assert.False(item11.IsRoot);
        }
        [Fact]
        public void RootTest()
        {
            var top = SecurityLevelModels.GetTestModel1();
            var item2 = top.GetItem(2);
            var item11 = top.GetItem(11);
            Assert.Equal(0, top.Root.UniqueIndex);
            Assert.Equal(0, item2.Root.UniqueIndex);
            Assert.Equal(0, item11.Root.UniqueIndex);
        }
        [Fact]
        public void IsParentOfTest()
        {
            var top = SecurityLevelModels.GetTestModel1();
            var item1 = top.GetItem(1);
            var item2 = top.GetItem(2);
            var item11 = top.GetItem(11);
            var item12 = top.GetItem(12);
            var item111 = top.GetItem(111);
            Assert.True(item1.IsParentOf(item11));
            Assert.True(item1.IsParentOf(item12));
            Assert.True(item1.IsParentOf(item111));
            Assert.False(item1.IsParentOf(item2));
            Assert.False(item1.IsParentOf(top));
            Assert.True(top.IsParentOf(item111));
        }
        [Fact]
        public void IsChildOfTest()
        {
            var top = SecurityLevelModels.GetTestModel1();
            var item1 = top.GetItem(1);
            var item2 = top.GetItem(2);
            var item11 = top.GetItem(11);
            var item12 = top.GetItem(12);
            var item111 = top.GetItem(111);
            Assert.True(item11.IsChildOf(item1));
            Assert.True(item12.IsChildOf(item1));
            Assert.True(item111.IsChildOf(item1));
            Assert.False(item2.IsChildOf(item1));
            Assert.False(top.IsChildOf(item1));
            Assert.True(item111.IsChildOf(top));
        }
        [Fact]
        public void IsBrotherOfTest()
        {
            var top = SecurityLevelModels.GetTestModel1();
            var item1 = top.GetItem(1);
            var item2 = top.GetItem(2);
            var item11 = top.GetItem(11);
            var item12 = top.GetItem(12);
            var item111 = top.GetItem(111);
            var item112 = top.GetItem(112);
            Assert.True(item1.IsBrotherOf(item2));
            Assert.True(item111.IsBrotherOf(item112));
            Assert.False(item11.IsBrotherOf(item1));
            Assert.False(item2.IsBrotherOf(item12));
            Assert.False(top.IsBrotherOf(item1));
            Assert.False(item111.IsBrotherOf(top));
        }
    }
}
