using Iis.Elastic;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class IisElasticManagerTests
    {
        [Fact]
        public void Test01()
        {
            var manager = new IisElasticManager(new IisElasticConfiguration(), new IisElasticSerializer());
            //manager.DoLowLevel();
        }
    }
}
