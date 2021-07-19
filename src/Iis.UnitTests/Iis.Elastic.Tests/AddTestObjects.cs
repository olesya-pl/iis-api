using Iis.Elastic;
using Iis.Elastic.SearchResult;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class AddTestObjects
    {
        [Fact]
        public async Task AddObjects()
        {
            var elasticManager = GetElasticManager();
            for (int i = 1; i <= 2000; i++)
            {
                var json = "{ \"aaa\": \"bbb\", \"__accessLevels\":\"" + GetGroupLevels(i % 2 == 0) + "\"}";
                if (!await elasticManager.PutDocumentAsync("test", i.ToString(), json))
                {
                    break;
                }
            }
            //Func<string, string> getIdFunc = id => isHistoricalIndex ? Guid.NewGuid().ToString("N") : id;
            //return nodes.Aggregate("", (acc, p) => acc += $"{{ \"index\":{{ \"_id\": \"{getIdFunc(p.Id):N}\" }} }}\n{p.SerializedNode.RemoveNewLineCharacters()}\n");

        }

        private string GetGroupLevels(bool evenLevels)
        {
            const int GROUPS = 15;
            const int LEVELS = 20;
            var sb = new StringBuilder();
            for (int i = 1; i <= GROUPS; i++)
            {
                if (i > 1) sb.Append(';');
                sb.Append($"{i}:");
                for (int j = 1; j <= LEVELS; j++)
                {
                    var n = evenLevels ? j * 2 : j * 2 - 1;
                    if (j > 1) sb.Append(',');
                    sb.Append($"{n}");
                }
            }
            return sb.ToString();
        }

        private ElasticManager GetElasticManager()
        {
            var resultExtractor = new Mock<SearchResultExtractor>();
            var logger = new Mock<ILogger<ElasticManager>>();
            var elasticLogUtils = new Mock<ElasticLogUtils>();
            var configuration = new ElasticConfiguration
            {
                Uri = "http://192.168.88.93:25395",
                DefaultLogin = "elastic",
                DefaultPassword = "PW4RAqW5mIGF2oMR"
            };
            return new ElasticManager(
                configuration,
                resultExtractor.Object,
                logger.Object,
                elasticLogUtils.Object);
        }

    }
}
