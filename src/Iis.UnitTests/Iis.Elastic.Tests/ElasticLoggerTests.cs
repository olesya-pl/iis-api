using System;
using System.Collections.Generic;
using Elasticsearch.Net;
using Iis.Elastic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class ElasticLoggerTests
    {
        [Fact]
        public void Error_IndexNotFound()
        {
            //act
            var log = ElasticLogUtils.PrepareLog("test request", new StringResponse("{\"error\":{\"root_cause\":[{\"type\":\"index_not_found_exception\",\"reason\":\"no such index [ont_materials2]\",\"resource.type\":\"index_or_alias\",\"resource.id\":\"ont_materials2\",\"index_uuid\":\"_na_\",\"index\":\"ont_materials2\"}],\"type\":\"index_not_found_exception\",\"reason\":\"no such index [ont_materials2]\",\"resource.type\":\"index_or_alias\",\"resource.id\":\"ont_materials2\",\"index_uuid\":\"_na_\",\"index\":\"ont_materials2\"},\"status\":404}")
            {
                ApiCall = new ApiCallDetails
                {
                    HttpStatusCode = 404,
                    HttpMethod = HttpMethod.GET,
                    Uri = new Uri("https://kek.com"),
                    Success = false
                }
            });

            //assert
            Assert.Equal("Request GET with path https://kek.com/ and request test request completed. Success:False ", log.Message);
            Assert.Equal(LogLevel.Error, log.LogLevel);
        }

        [Fact]
        public void Success_IndexCreated()
        {
            //act
            var log = ElasticLogUtils.PrepareLog("test request", new StringResponse("{\"acknowledged\":true,\"shards_acknowledged\":true,\"index\":\"ont_materials2\"}")
            {
                ApiCall = new ApiCallDetails
                {
                    HttpStatusCode = 200,
                    HttpMethod = HttpMethod.GET,
                    Uri = new Uri("https://kek.com"),
                    Success = true
                }
            });

            //assert
            Assert.Equal("Request GET with path https://kek.com/ and request test request completed. Success:True ", log.Message);
            Assert.Equal(LogLevel.Information, log.LogLevel);
        }

        [Fact]
        public void Success_QuerySuccess()
        {
            //act
            var log = ElasticLogUtils.PrepareLog("test request", new StringResponse("{\"took\":48,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":{\"value\":10000,\"relation\":\"gte\"},\"max_score\":11.590206,\"hits\":[{\"_index\":\"ont_materials\",\"_type\":\"_doc\",\"_id\":\"049db1e620204abeb4bb712dd65828df\",\"_score\":11.590206,\"_source\":{\"LoadData\":{\"States\":[],\"Coordinates\":null,\"Objects\":[],\"ReceivingDate\":\"2020-08-03T15:14:47.4192097Z\",\"From\":null,\"Code\":null,\"Tags\":[],\"LoadedBy\":null},\"ProcessedStatus\":{\"Title\":\"Не оброблено\",\"Id\":\"0a641312-abb7-4b40-a766-0781308eb077\"},\"SessionPriority\":null,\"MLResponses\":null,\"Metadata\":{\"features\":[{\"featureId\":\"00000000-0000-0000-0000-000000000000\"}],\"GeoRegion\":\"STAHANOV\",\"CallType\":\"Речь\",\"RegTime\":\"07.09.2017, 10:05:37\",\"Duration\":34,\"source\":\"cell.voice\",\"type\":\"audio\",\"Unit\":\"POSEYDON\"},\"Source\":\"cell.voice\",\"Transcriptions\":null,\"Importance\":null,\"Relevance\":null,\"ProcessedMlHandlersCount\":0,\"Completeness\":null,\"Children\":[],\"ParentId\":null,\"Assignee\":null,\"Title\":null,\"Data\":[{\"Type\":\"sessionDateFrom\",\"Text\":\"2017-09-01T00:00:00.000Z\"},{\"Type\":\"sessionDateTo\",\"Text\":\"2017-09-30T00:00:00.000Z\"},{\"Type\":\"sessionInfo.json\",\"Text\":\"{\"},{\"Type\":\"originalContent\",\"Text\":\"\"},{\"Type\":\"uploadSessionKey\",\"Text\":\"perf_test\"},{\"Type\":\"createdDate\",\"Text\":\"2020-07-06T02:13:43.287Z\"},{\"Type\":\"modifiedDate\",\"Text\":\"2020-07-06T02:13:43.287Z\"},{\"Type\":\"size\",\"Text\":\"278829\"},{\"Type\":\"fileName\",\"Text\":\"2017/09/01.09-30.09/Voice_07-09-2017 10-05-37 (424247).mp3\"}],\"MlHandlersCount\":0,\"Type\":\"audio\",\"Content\":\"\",\"NodeIds\":[],\"CreatedDate\":\"2020-08-03T15:14:47Z\",\"SourceReliability\":null,\"FileId\":\"1194c893-4baf-4005-8f40-f1b944725d85\",\"Id\":\"049db1e6-2020-4abe-b4bb-712dd65828df\",\"Reliability\":null},\"highlight\":{\"Data.Text\":[\"<em>2020</em>-07-06T02:13:43.287Z\",\"<em>2020</em>-07-06T02:13:43.287Z\"],\"Id\":[\"049db1e6-<em>2020</em>-4abe-b4bb-712dd65828df\"]}}]}}")
            {
                ApiCall = new ApiCallDetails
                {
                    HttpStatusCode = 200,
                    HttpMethod = HttpMethod.GET,
                    Uri = new Uri("https://kek.com"),
                    Success = true
                }
            });

            //assert
            Assert.Equal("Request GET with path https://kek.com/ and request test request completed. Success:True Took:48. Timed out:False.", log.Message);
            Assert.Equal(LogLevel.Information, log.LogLevel);
        }
    }
}
