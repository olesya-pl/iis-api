using Newtonsoft.Json;

namespace IIS.Core.Ontology
{
    public class JsonOntologyWriter : OntologyVisitor
    {
        private readonly JsonWriter _jsonWriter;

        public JsonOntologyWriter(JsonWriter jsonWriter)
        {
            _jsonWriter = jsonWriter;
        }

        public override void EnterArrayRelation(ArrayRelation relation)
        {
            _jsonWriter.WritePropertyName(relation.Schema.Name);
            _jsonWriter.WriteStartArray();
        }
        public override void LeaveArrayRelation(ArrayRelation relation) => _jsonWriter.WriteEndArray();
        public override void EnterAttribute(Attribute attribute) => _jsonWriter.WriteValue(attribute.Value);
        public override void EnterObject(Entity entity) => _jsonWriter.WriteStartObject();
        public override void LeaveObject(Entity entity) => _jsonWriter.WriteEndObject();
        public override void EnterRelation(Relation relation) => _jsonWriter.WritePropertyName(relation.Schema.Name);
        public override void EnterUnion(Union union) => _jsonWriter.WriteStartArray();
        public override void LeaveUnion(Union union) => _jsonWriter.WriteEndArray();
    }
}
