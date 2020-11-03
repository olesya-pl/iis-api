using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.AcceptanceTests.Helpers;
using FluentAssertions;
using GraphQL;
using GraphQL.Client.Http;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;

namespace Iis.AcceptanceTests.APISteps
{
    [Binding]
    public class EventsSteps
    {
        private readonly ScenarioContext context;
        private readonly GraphQLHttpClient graphQlClient;

        public EventsSteps(ScenarioContext injectedContext)
        {
            context = injectedContext;

            graphQlClient = GraphQLHttpClientFactory
                                .CreateContourGraphQLHttpClient()
                                .WithAuthToken(context.GetAuthToken());
        }

        private async Task<JObject> ExecuteQueryAsync(GraphQLRequest request) 
        {
            var responce = await graphQlClient.SendQueryAsync<JObject>(request);

            if (responce.Errors != null && responce.Errors.Any()) throw new InvalidOperationException("Have error while request.");

            return responce.Data;
        }

        
        [Given(@"fetch required dictionaties")]
        public async Task FetchRequiredDictionaries()
        {
            var request = new GraphQLHttpRequest
            {
                Query = "query{ allEntities(pagination:{ page:1, pageSize: 2000}, filter:{types:\"Enum\"}){ count, items{ id, __typename ...on EntityEnum { name }}}}"
            };

            var responce = await ExecuteQueryAsync(request);

            JArray items = responce.SelectToken("allEntities").SelectToken("items") as JArray;

            var result = items.Select(token => (key: token.Value<string>("__typename") + "." + token.Value<string>("name"), value: token.Value<string>("id")));

            var dictionary = result
                                .GroupBy(kv => kv.key).Select(gkv => gkv.First())
                                .ToDictionary(kv => kv.key, kv => kv.value);

            context.Set(dictionary, ContextKeys.EntityEnumsDictionary);
        }

        [Given(@"I have fullfield (.*), (.*), (.*), (.*), (.*), (.*) in the event form")]
        public void GivenIHaveFullfieldТестоваПодіяАктивнаTerrorizmImportantOtherVisitsRussiaInTheEventForm(string name, string state, string сomponentfield, string importance, string eventType, string country)
        {
            var dictionary = context.Get<Dictionary<string, string>>(ContextKeys.EntityEnumsDictionary);

            var eventName = $"{name}_{DateTime.Now}";

            var tryStateID = dictionary.TryGetValue($"EntityEventState.{state}", out string stateID);

            var tryComponentID = dictionary.TryGetValue($"EntityEventComponent.{сomponentfield}", out string componentID);

            var tryImportanceID = dictionary.TryGetValue($"EntityEventImportance.{importance}", out string importanceID);

            var tryEventTypeID = dictionary.TryGetValue($"EntityEventType.{eventType}", out string eventTypeID);

            var tryCountryID = dictionary.TryGetValue($"EntityCountry.{country}", out string countryID);

            var related = new[] { new { targetId = countryID } };

            var request = new GraphQLHttpRequest
            {
                Query = @"mutation($data:CreateEventInput){ createEntityEvent(data:$data){ details{ id, name}}}",
                Variables = new
                {
                    data = new
                    {
                        name = eventName,
                        state = new 
                        { 
                            targetId = stateID 
                        },
                        component = new
                        {
                            targetId = componentID
                        },
                        importance = new
                        {
                            targetId = importanceID
                        },
                        eventType = new
                        {
                            targetId = eventTypeID
                        },
                        relatesToCountry = related
                    }
                }
            };

            context.Add(ContextKeys.CreateEventRequest, request);
        }

        [When(@"I press add")]
        public async Task WhenIPressAdd()
        {
            var request = context.Get<GraphQLHttpRequest>(ContextKeys.CreateEventRequest);

            var responce = await graphQlClient.SendMutationAsync<JObject>(request);
           
            context.SetResponse(ContextKeys.CreateEventResponse, responce);
        }

        [Then(@"the result should have id and name should start with (.*)")]
        public void ThenTheResultShouldHaveIdAndNameShouldStartWithТестоваПодія(string expectedName)
        {
            var response = context.Get<GraphQLResponse<JObject>>(ContextKeys.CreateEventResponse);

            var details = response.Data.SelectToken("createEntityEvent").SelectToken("details");

            var id = details.Value<string>("id");
            var givenName = details.Value<string>("name");

            id.Should().NotBeNullOrEmpty();

            givenName.Should().StartWith(expectedName);
        }

        [Then(@"create request should be executed without errors")]
        public void ThenCreateRequestShouldBeExecutedWithoutErrors()
        {
            var response = context.Get<GraphQLResponse<JObject>>(ContextKeys.CreateEventResponse);

            response.Errors.Should().BeNull();
        }

    }
}
