using Elasticsearch.Net;
using Nest;
using System;
using System.IO;

namespace Iis.Elastic
{
    public class Person
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
    }

    public class IisElasticManager
    {
        ElasticClient _client;
        ElasticLowLevelClient _lowLevelClient;
        
        const string INDEX_NAME = "test";
        public IisElasticManager(string uriString = @"http://localhost:9200")
        {
            var uri = new Uri(uriString);
            var settings = new ConnectionSettings(uri);
            _client = new ElasticClient(settings);

            var connectionPool = new SniffingConnectionPool(new[] { uri });
            var config = new ConnectionConfiguration(connectionPool);
            _lowLevelClient = new ElasticLowLevelClient(config);
        }

        public void InsertPerson()
        {
            var settings = new IndexSettings { NumberOfReplicas = 1, NumberOfShards = 2 };
            var indexConfig = new IndexState
            {
                Settings = settings
            };
            
            if (!_client.Indices.Exists(INDEX_NAME).Exists)
            {
                _client.Indices.Create(INDEX_NAME, index => index
                    .InitializeUsing(indexConfig)
                    .Map<Person>(mp => mp.AutoMap()));
            }

            var person = new Person { Id = "abcdabcd12341234", Name = "John", Title = "Mister" };
            
            var result = _client.Index(person, i => i.Index(INDEX_NAME).Id(person.Id));
            
        }

        public void DoLowLevel()
        {
            //var response = _lowLevelClient.DoRequest()
        }
    }
}
