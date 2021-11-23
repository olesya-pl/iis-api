using System;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json.Linq;

using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using Iis.Elastic.SearchQueryExtensions;

namespace Iis.Elastic
{
    public class ElasticSerializer : IElasticSerializer
    {
        private const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private const string AccessLevelPropertyName = "__accessLevel";
        private const string LattiturePropertyName = "lat";
        private const string LongitudePropertyName = "long";
        private const string ShortLongitudePropertyName = "lon";
        private const string CoordinatesPropertyName = "__coordinates";
        private const string LocationPropertyName = "location";

        public string GetJsonByExtNode(IExtNode extNode)
        {
            return GetJsonObjectByExtNode(extNode).ToString();
        }
        public JObject GetJsonObjectByExtNode(IExtNode extNode, bool IsHeadNode = true)
        {
            var json = new JObject();

            if (IsHeadNode)
            {
                json[nameof(extNode.Id)] = extNode.Id;
                json[nameof(extNode.NodeTypeName)] = extNode.NodeTypeName;

                if (extNode.AccessLevel.HasValue)
                    json[AccessLevelPropertyName] = extNode.AccessLevel;

                if (!string.IsNullOrEmpty(extNode.NodeTypeTitle))
                {
                    json[nameof(extNode.NodeTypeTitle)] = extNode.NodeTypeTitle;
                    json[$"{nameof(extNode.NodeTypeTitle)}{SearchQueryExtension.AggregateSuffix}"] = extNode.NodeTypeTitle;
                }
                json[nameof(extNode.CreatedAt)] = extNode.CreatedAt.ToString(DateFormat, CultureInfo.InvariantCulture);
                json[nameof(extNode.UpdatedAt)] = extNode.UpdatedAt.ToString(DateFormat, CultureInfo.InvariantCulture);
            }
            else if (extNode.NodeType.IsObject)
            {
                json[nameof(extNode.Id)] = extNode.Id;
            }

            var coordinates = extNode.GetCoordinatesWithoutNestedObjects();
            if (coordinates != null && coordinates.Count > 0)
            {
                var coords = new JArray();
                foreach (var coord in coordinates)
                {
                    var item = new JObject();
                    item[LattiturePropertyName] = coord.Latitude;
                    item[LongitudePropertyName] = coord.Longitude;
                    coords.Add(item);
                }
                json[CoordinatesPropertyName] = coords;
            }

            TryAddGeoPointProperty(json, LocationPropertyName, extNode.Location);

            foreach (var childGroup in extNode.Children.GroupBy(p => new { p.NodeTypeName, p.EntityTypeName }))
            {
                var key = childGroup.Key.NodeTypeName;
                if (childGroup.Count() == 1)
                {
                    var child = childGroup.First();

                    json[key] = GetExtNodeValue(child);
                }
                else
                {
                    var items = new JArray();
                    json[key] = items;
                    foreach (var child in childGroup)
                    {
                        items.Add(GetExtNodeValue(child));
                    }
                }
            }

            return json;
        }

        private static JToken GetFuzzyDateJToken(IExtNode extNode)
        {
            int? year = (int?)extNode.Children.SingleOrDefault(c => c.NodeTypeName == "year")?.AttributeValue;
            if (year == null)
            {
                return null;
            }
            int month = (int?)extNode.Children.SingleOrDefault(c => c.NodeTypeName == "month")?.AttributeValue ?? 1;
            int day = (int?)extNode.Children.SingleOrDefault(c => c.NodeTypeName == "day")?.AttributeValue ?? 1;

            try
            {
                var date = new DateTime(year.Value, month, day);
                return JToken.FromObject(date.ToString(DateFormat, CultureInfo.InvariantCulture));
            }
            catch
            {
                return null;
            }
        }

        private static JToken GetDateTimeJToken(IExtNode extNode)
        {
            var returnValue = extNode.AttributeValue;

            if (DateTimeOffset.TryParse(extNode.AttributeValue.ToString(), out DateTimeOffset offsetValue))
            {
                returnValue = offsetValue.ToString(DateFormat, CultureInfo.InvariantCulture);
            }
            return JToken.FromObject(returnValue);
        }

        private static bool TryAddGeoPointProperty(JObject jobject, string propertyName, GeoCoordinates location)
        {
            if (jobject is null || string.IsNullOrWhiteSpace(propertyName) || location is null) return false;

            var coordinate = new JObject(
                new JProperty(LattiturePropertyName, location.Latitude),
                new JProperty(ShortLongitudePropertyName, location.Longitude));

            jobject[propertyName] = coordinate;

            return true;
        }

        private JToken GetExtNodeValue(IExtNode extNode)
        {
            if (extNode.EntityTypeName == EntityTypeNames.FuzzyDate.ToString())
            {
                return GetFuzzyDateJToken(extNode);
            }

            if (extNode.ScalarType.HasValue && extNode.ScalarType.Value == ScalarType.Date)
            {
                return GetDateTimeJToken(extNode);
            }

            if (extNode.IsAttribute && extNode.AttributeValue != null)
            {
                return JToken.FromObject(extNode.AttributeValue);
            }

            return GetJsonObjectByExtNode(extNode, false);
        }
    }
}
