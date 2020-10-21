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

        private Lazy<IGeoCoordinates> _lazyGeoCoordinates;
        public IGeoCoordinates ValueAsGeoCoordinates => _lazyGeoCoordinates.Value;

        public AttributeData()
        {
            var _lazyCoordinates = new Lazy<IGeoCoordinates>(() =>
                ScalarType == ScalarType.Geo ? ExtractCoordinates(Value) : null
            );
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
