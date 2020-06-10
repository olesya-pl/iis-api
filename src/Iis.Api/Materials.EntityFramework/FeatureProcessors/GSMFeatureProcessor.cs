
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Iis.Utility;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using IIS.Domain;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class GSMFeatureProcessor : IFeatureProcessor
    {
        private const string HIGHLIGHT = "highlight";
        private const string SignTypeName = "PhoneSign";
        private readonly IElasticService _elasticService;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly MutationCreateResolver _createResolver;
        private readonly MutationUpdateResolver _updateResolver;

        private static readonly List<string> prioritizedField = new List<string>
        {
            FeatureFields.IMSIField,
            FeatureFields.PhoneNumberField,
            FeatureFields.IMEIField,
        };
        private static readonly JsonMergeSettings mergeSettings = new JsonMergeSettings
		{
			MergeArrayHandling = MergeArrayHandling.Union,
			PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase,
			MergeNullValueHandling = MergeNullValueHandling.Ignore
		};

        public GSMFeatureProcessor(IElasticService elasticService
        , IOntologyProvider ontologyProvider
        , MutationCreateResolver createResolver
        , MutationUpdateResolver updateResolver)
        {
            _elasticService = elasticService;
            _ontologyProvider = ontologyProvider;
            _createResolver = createResolver;
            _updateResolver = updateResolver;
        }

        public async Task<JObject> ProcessMetadata(JObject metadata)
        {
            if (!FeaturesSectionIsValidNotEmpty(metadata)) return metadata;

            var ontology = await _ontologyProvider.GetOntologyAsync();

            var signType = ontology.GetEntityType(SignTypeName);

            var features = metadata.SelectToken(FeatureFields.FeaturesSectionName);

            foreach (JObject feature in features)
            {
                RemoveEmptyValues(feature);

                var searchResult = await SearchExistingFeature(feature);

                if(searchResult.isExist)
                {
                    feature[FeatureFields.FeatureId] = searchResult.featureId.Value.ToString();
                    
                    var updatedFeature = MergeFeatures(feature, searchResult.feature);
                    
                    await PutFeature(updatedFeature);

                    //update existing node in ontology
                } 
                else
                {
                    //put new node to onlology
                    var entity = await SaveFeature(signType, feature);

                    //await PutFeature(feature);
                }
            }


            return metadata;
        }

        private bool FeaturesSectionIsValidNotEmpty(JObject metadata)
        {
            if (!metadata.ContainsKey(FeatureFields.FeaturesSectionName)) return false;

            if (!(metadata.SelectToken(FeatureFields.FeaturesSectionName) is JArray)) return false;

            if (!metadata.SelectToken(FeatureFields.FeaturesSectionName).HasValues) return false;

            return true;
        }
        private async Task<(bool isExist, string fieldName, Guid? featureId, JObject feature)> SearchExistingFeature(JObject feature)
        {
            foreach (var fieldName in prioritizedField)
            {
                var fieldValue = feature.GetValue(fieldName, StringComparison.InvariantCultureIgnoreCase)?.Value<string>();

                if(string.IsNullOrWhiteSpace(fieldValue)) continue;

                var searchResult = await SearchFeature(fieldName, fieldValue);

                if (searchResult.isExist) return (searchResult.isExist, fieldName, searchResult.featureId, searchResult.feature);
            }
            return (false, null, null, null);    
        }
        private async Task<(bool isExist, Guid? featureId, JObject feature)> SearchFeature(string fieldName, string fieldValue)
        {
            fieldName = fieldName.ToLowerInvariant();

            var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(
                        _elasticService.FeatureIndexes,
                        new ElasticFilter { Limit = 1, Offset = 0, Suggestion = $"({fieldName}:{fieldValue})"});
            
            if(searchResult.Count == 0) return (false, null, null);

            var featurePair = searchResult.Items.FirstOrDefault();

            featurePair.Value.SearchResult.Remove(HIGHLIGHT);

            return (true, featurePair.Key, featurePair.Value.SearchResult);
        }
        private async Task<bool> PutFeature(JObject feature)
        {
            var featureId = new Guid(feature[FeatureFields.FeatureId].Value<string>());

            var featureDocument = new JObject();

            foreach(var property in feature.Properties())
            {
                var propertyName = property.Name.ToLowerCamelcase();

                if(propertyName == FeatureFields.FeatureId.ToLowerCamelcase()) continue;

                featureDocument.Add(new JProperty(propertyName, property.Value.Value<string>()));
            }

            return await _elasticService.PutFeatureAsync(featureId, featureDocument);
        }
        private async Task<Entity> SaveFeature(EntityType signType, JObject feature)
        {
            var properties = new Dictionary<string, object>();

            var phoneNumber = feature.GetValue(FeatureFields.PhoneNumberField, StringComparison.InvariantCultureIgnoreCase)?.Value<string>();

            properties.Add("value", phoneNumber);

            var entity = await _createResolver.CreateEntity(signType, properties);

            return await Task.FromResult(entity);
        }
        private JObject MergeFeatures(JObject newFeature, JObject existingFeature)
        {
            existingFeature.Merge(newFeature, mergeSettings);

            return existingFeature;
        }
        private JObject RemoveEmptyValues(JObject feature)
        {
            var emptyPropsList = new List<string>();

            foreach (var property in feature.Properties())
            {
                var propertyName = property.Name;
                var propertyValue =  property.Value.Value<string>();

                if(string.IsNullOrWhiteSpace(propertyValue))
                {
                    emptyPropsList.Add(propertyName);
                }
            }

            foreach (var propertyName in emptyPropsList)
            {
                feature.Remove(propertyName);           
            }
            
            return feature;
        }
    }

}