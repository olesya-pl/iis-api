using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcceptanceTests.Helpers;
using FluentAssertions;
using GraphQL;
using GraphQL.Client.Http;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;

namespace AcceptanceTests.APISteps
{
    [Binding]
    public class EntityesSteps
    {
        private readonly ScenarioContext context;
        private readonly GraphQLHttpClient graphQlClient;

        public EntityesSteps(ScenarioContext injectedContext)
        {
            context = injectedContext;

            graphQlClient = GraphQLHttpClientFactory
                                .CreateContourGraphQLHttpClient()
                                .WithAuthToken(context.GetAuthToken());

        }

        [Given(@"I have fullfield (.*), (.*), (.*), (.*), (.*), (.*), (.*) in the object form")]
        public void GivenIHaveFullfieldВорожийПершочерговийAPCOПОРТАТИВНАРАДІОСТАНЦІЯSRXГлушенняЕфіруКолірЧорний_ZInTheObjectForm( string affiliationField, string importanceField, string nameField, string titleField, string purposeField, string descriptionField, string lastConfirmedAtField )
        {
            var dictionary = context.Get<Dictionary<string, string>>(ContextKeys.EntityEnumsDictionary);

            var tryAffiliation = dictionary.TryGetValue($"EntityObjectAffiliation.{affiliationField}", out string affiliationID);
            var tryImportance = dictionary.TryGetValue($"EntityObjectImportance.{importanceField}", out string importanceID);
            var name = $"{nameField}_{DateTime.Now}";
            var title = titleField;
            var purpose = purposeField;
            var description = descriptionField;
            var lastConfirmedAt = lastConfirmedAtField;
            var request = new GraphQLHttpRequest
            {
                Query = @"mutation($data:CreateRadionetworkInput!){ createEntityRadionetwork(data:$data){ details{ id, name}}}",
                Variables = new
                {
                    data = new
                    {
                        affiliation = new
                        {
                            targetId = affiliationID
                        },
                        importance = new
                        {
                            targetId = importanceID
                        },
                        name = name,
                        title = title,
                        purpose = purpose,
                        description = description,
                        lastConfirmedAt = lastConfirmedAt

                    }
                }
            };

            context.Add(ContextKeys.CreateEntityRequest, request);
        }
        [When(@"I press add entity")]
        public async Task WhenIPressAddEntity()
        {
            var request = context.Get<GraphQLHttpRequest>(ContextKeys.CreateEntityRequest);

            var responce = await graphQlClient.SendMutationAsync<JObject>(request);

            context.SetResponse(ContextKeys.CreateEntityResponse, responce);
        }

        [Then(@"the entity creation result should have id and name should start with (.*)")]
        public void ThenTheEntityCreationResultShouldHaveIdAndNameShouldStartWithAPCO(string expectedName)
        {
            //ScenarioContext.Current.Pending();
            var response = context.Get<GraphQLResponse<JObject>>(ContextKeys.CreateEntityResponse);

            var details = response.Data.SelectToken("createEntityRadionetwork").SelectToken("details");

            var id = details.Value<string>("id");
            var givenName = details.Value<string>("name");

            id.Should().NotBeNullOrEmpty();

            givenName.Should().StartWith(expectedName);
        }

        [Then(@"entity creation request should be executed without errors")]
        public void ThenEntityCreationRequestShouldBeExecutedWithoutErrors()
        {
            var response = context.Get<GraphQLResponse<JObject>>(ContextKeys.CreateEntityResponse);

            response.Errors.Should().BeNull();
        }

    }
}
