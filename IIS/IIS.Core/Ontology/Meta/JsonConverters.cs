using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology.Meta
{
    public abstract class TypeMetaConverterBase : JsonConverter<ITypeMeta>
    {
        protected readonly JsonSerializer js;

        protected TypeMetaConverterBase(ScalarType? scalarType)
        {
            js = new JsonSerializer {MissingMemberHandling = MissingMemberHandling.Error};
            js.Converters.Add(new ValidationConverter(scalarType, js));
        }

        public override void WriteJson(JsonWriter writer, ITypeMeta value, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }
    }

    public class RelationTypeMetaConverter : TypeMetaConverterBase
    {
        public RelationTypeMetaConverter(ScalarType? scalarType) : base(scalarType)
        {
        }

        public override ITypeMeta ReadJson(JsonReader reader, System.Type objectType, ITypeMeta existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            if (jo.ContainsKey("inversed"))
                return jo.ToObject<MetaForEntityTypeRelation>(js);
            return jo.ToObject<RelationTypeMeta>(js);
        }
    }
    
    public class TypeMetaConverter : TypeMetaConverterBase
    {
        public TypeMetaConverter(ScalarType? scalarType) : base(scalarType)
        {
        }

        public override ITypeMeta ReadJson(JsonReader reader, System.Type objectType, ITypeMeta existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            return jo.ToObject<TypeMeta>(js);
        }
    }
    
    public class ValidationConverter : JsonConverter<IValidation>
    {
        private readonly ScalarType? _scalarType;
        private JsonSerializer js;

        public ValidationConverter(ScalarType? scalarType, JsonSerializer js)
        {
            _scalarType = scalarType;
            this.js = js;
        }

        public override void WriteJson(JsonWriter writer, IValidation value, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override IValidation ReadJson(JsonReader reader, System.Type objectType, IValidation existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            switch (_scalarType)
            {
                case ScalarType.String:
                    return jo.ToObject<StringValidation>();
                case ScalarType.Integer:
                    return jo.ToObject<IntValidation>();
                case ScalarType.DateTime:
                    return jo.ToObject<DateValidation>();
                default:
                    return jo.ToObject<Validation>();
            }
        }
    }
}