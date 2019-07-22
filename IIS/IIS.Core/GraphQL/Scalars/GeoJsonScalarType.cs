using GeoJSON.Net.Converters;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Scalars
{
    public class GeoJsonScalarType : JsonScalarType
    {
        public GeoJsonScalarType() : base("GeoJsonScalar")
        {
        }

        public override bool TryDeserialize(object serialized, out object value)
        {
            if (base.TryDeserialize(serialized, out value) && value is JObject jo)
            {
                // quick way to validate GeoJSON deserialization, still returning JObject
                JsonConvert.DeserializeObject<IGeometryObject>(jo.ToString(), new GeometryConverter());
                return true;
            }
            return false;
        }
    }
}