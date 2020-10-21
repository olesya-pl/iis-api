using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScalarTypeEnum = Iis.Interfaces.Ontology.Schema.ScalarType;

namespace Iis.Domain.ExtendedData
{
    public class ExtNode: IExtNode
    {
        public string Id { get; set; }
        public string NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public string NodeTypeTitle { get; set; }
        public string EntityTypeName { get; set; }
        public object AttributeValue { get; set; }
        public ScalarType? ScalarType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IReadOnlyList<IExtNode> Children { get; set; } = new List<ExtNode>();
        public bool IsAttribute => AttributeValue != null && Children.Count == 0;

        public INodeTypeLinked NodeType { get; set; }

        public List<IGeoCoordinates> GetCoordinatesWithoutNestedObjects()
        {
            var geoNodes = GetAttributesRecursiveWithoutNestedObjects(ScalarTypeEnum.Geo);
            if (geoNodes.Count == 0) return null;

            return geoNodes.Select(gn => ExtractCoordinates(gn.AttributeValue.ToString())).ToList();

        }

        public List<IExtNode> GetAttributesRecursiveWithoutNestedObjects(ScalarType scalarType)
        {
            if (IsAttribute && ScalarType == scalarType)
            {
                return new List<IExtNode> { this };
            }
            var children = Children.Where(p => !p.NodeType.IsObjectOfStudy).SelectMany(n => n.GetAttributesRecursiveWithoutNestedObjects(scalarType));
            return children.ToList();
        }

        public List<IExtNode> GetAttributesRecursive(ScalarType scalarType)
        {
            if (IsAttribute && ScalarType == scalarType)
            {
                return new List<IExtNode> { this };
            }
            var children = Children.SelectMany(n => n.GetAttributesRecursive(scalarType));
            return children.ToList();
        }

        private IGeoCoordinates ExtractCoordinates(string json)
        {
            try
            {
                var jObject = JObject.Parse(json);
                var coordinatesJson = (JArray)jObject["coordinates"];
                var lat = decimal.Parse(coordinatesJson[0].ToString());
                var lang = decimal.Parse(coordinatesJson[1].ToString());
                var geoCoordinates = new GeoCoordinates(lat, lang);
                return geoCoordinates;
            }
            catch
            {
                return new GeoCoordinates(0, 0);
            }
        }
    }
}
