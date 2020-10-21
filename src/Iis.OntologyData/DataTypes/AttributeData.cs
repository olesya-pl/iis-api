using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.DataTypes
{
    public class AttributeData: IAttribute
    {
        public Guid Id { get; set; }
        public string Value { get; set; }

        internal NodeData _node;
        public INode Node => _node;
        public ScalarType ScalarType => Node.NodeType.AttributeType.ScalarType;

        private IGeoCoordinates _coordinates;
        public IGeoCoordinates ValueAsGeoCoordinates => _coordinates ?? (_coordinates = ExtractCoordinates(Value));

        private IGeoCoordinates ExtractCoordinates(string json)
        {
            try
            {
                if (ScalarType != ScalarType.Geo) return null;

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
