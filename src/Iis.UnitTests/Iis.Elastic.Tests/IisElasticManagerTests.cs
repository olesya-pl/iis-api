using Iis.Domain.Elastic;
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
        public async void Test01()
        {
            var manager = new IisElasticManager(new IisElasticConfiguration(), new IisElasticSerializer());
            var searchParams = new IisElasticSearchParams
            {
                ResultFields = new List<string> { "_id"},
                Query = "*низ*"
            };
            var response = await manager.Search(searchParams);
        }
    }
}
