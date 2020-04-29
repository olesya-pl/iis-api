using NUnit.Framework;
using GraphQL;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Iis.ApiTests.Tests
{
	[TestFixture]
	public class MaterialsQueriesTests : Authentification
	{
        public class MaterialResponse
        {
            public JObject Materials { get; set; }
        }

        [Test]
        [TestCase(1, 5)]
		public void RetrieveMaterials(int page, int pageSize)
		{
            var request = new GraphQLRequest
            {
                Query =
                @"query{
                    materials(pagination:{pageSize:" + pageSize + @", page:" + page +@"}){
                        count
                        items{
                            id,
                            metadata
                            {
                                type,
                                source
                            }
                        }
                    }
                }"
            };

            var materialResponse = ExecureGraphQlRequest<MaterialResponse>(@"http://dev.contour.net:5000", request, _authToken);
            
            Assert.NotZero(materialResponse.Materials.Value<int>("count"));
            Assert.AreEqual(pageSize,  ((JContainer)materialResponse.Materials["items"]).Count);
        }
	}
}
