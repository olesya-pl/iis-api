using Iis.Elastic.SearchResult;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Elastic.Converters
{
    public class AggregationItemConverter : JsonConverter
    {
        private const string SubAggs = "sub_aggs";

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var aggregationItem = new SerializableAggregationItem();
            var token = JObject.Load(reader);
            using var tokenReader = token.CreateReader();

            serializer.Populate(tokenReader, aggregationItem);

            IReadOnlyCollection<JProperty> properties = token.Properties()
                .Where(IsGroupedAggregate)
                .ToArray();
            if (properties.Count == 0) return aggregationItem;

            aggregationItem.GroupedSubAggs = new Dictionary<string, AggregationItem>(properties.Count);

            foreach (var property in properties)
            {
                var item = new SerializableAggregationItem();
                using var propertyReader = property.Value.CreateReader();

                serializer.Populate(propertyReader, item);
                aggregationItem.GroupedSubAggs.Add(property.Name, item);
            }

            return aggregationItem;
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(SerializableAggregationItem);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        private bool IsGroupedAggregate(JProperty property) => property.HasValues
                && property.Value is JObject
                && property.Name != SubAggs;
    }
}