using System;
using System.Collections.Generic;
using IIS.Core.Ontology.EntityFramework;
using Iis.Interfaces.Elastic;
using Xunit;
using Xunit.Abstractions;

namespace Iis.UnitTests.Services
{
    public class ElasticServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ElasticServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void test1()
        {
            var service = new ElasticService(null, null, null, null);

            var queryString = service.CreateFullQueryString(new ElasticFilter
            {
                Suggestion = "suggestion",
                CherryPickedItems = new List<string> {"cherry1", "cherry2"},
                FilteredItems = new List<Property>()
                {
                    new Property
                    {
                        Name = "name",
                        Value = "Taras"
                    },
                    new Property
                    {
                        Name = "age",
                        Value = "27"
                    }
                }
            });
            
            var queryString1 = service.CreateFullQueryString(new ElasticFilter
            {
                CherryPickedItems = new List<string> {"cherry1", "cherry2"},
                FilteredItems = new List<Property>()
                {
                    new Property
                    {
                        Name = "name",
                        Value = "Taras"
                    },
                    new Property
                    {
                        Name = "age",
                        Value = "27"
                    }
                }
            });
            
            var queryString2 = service.CreateFullQueryString(new ElasticFilter
            {
                Suggestion = "suggestion",
                FilteredItems = new List<Property>()
                {
                    new Property
                    {
                        Name = "name",
                        Value = "Taras"
                    },
                    new Property
                    {
                        Name = "age",
                        Value = "27"
                    }
                }
            });
            
            var queryString3 = service.CreateFullQueryString(new ElasticFilter
            {
                Suggestion = "suggestion",
                CherryPickedItems = new List<string> {"cherry1", "cherry2"},
            });
            
            var queryString4 = service.CreateFullQueryString(new ElasticFilter
            {
                CherryPickedItems = new List<string> {"cherry1", "cherry2"},
            });
            
            var queryString5 = service.CreateFullQueryString(new ElasticFilter
            {
                Suggestion = "suggestion",
            });
            
            var queryString6 = service.CreateFullQueryString(new ElasticFilter
            {
                FilteredItems = new List<Property>()
                {
                    new Property
                    {
                        Name = "name",
                        Value = "Taras"
                    },
                    new Property
                    {
                        Name = "age",
                        Value = "27"
                    }
                }
            });
            
            _testOutputHelper.WriteLine(queryString);
            _testOutputHelper.WriteLine(queryString1);
            _testOutputHelper.WriteLine(queryString2);
            _testOutputHelper.WriteLine(queryString3);
            _testOutputHelper.WriteLine(queryString4);
            _testOutputHelper.WriteLine(queryString5);
            _testOutputHelper.WriteLine(queryString6);

        }
    }
}