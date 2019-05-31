using System;
using System.IO;
using Newtonsoft.Json;

namespace IIS.Core.Ontology
{
    public class JsonOntologyReader //: OntologyVisitor
    {
        private readonly StringReader _stringReader;
        private readonly JsonTextReader _jsonReader;
        private readonly dynamic _builder;

        public JsonOntologyReader(string json)
        {
            _stringReader = new StringReader(json);
            _jsonReader = new JsonTextReader(_stringReader);
        }

        public Entity GetEntity()
        {
            while (_jsonReader.Read())
            {
                var token = _jsonReader.TokenType;
                var value = _jsonReader.Value;
            }

            return _builder.Build();
        }
        public ArrayRelation ReadArrayRelation(Constraint schema)
        {
            _jsonReader.Read();//propName
            _jsonReader.Read();//startArr
            //_builder.StartArrayRelation();
            var arrayRelation = new ArrayRelation(schema);
            return arrayRelation;
        }
        public ArrayRelation ReadEndArrayRelation()
        {
            _jsonReader.Read();//endArr
            //_builder.EndArrayRelation();
            return null;
        }
        public Attribute ReadAttribute(AttributeClass schema)
        {
            _jsonReader.Read();//propName
            _jsonReader.Read();//[Type]value
            var value = _jsonReader.Value;
            //_builder.StartAttribute();
            var attribute = new Attribute(schema, value);
            return attribute;
        }

        public Entity ReadObject(TypeEntity schema)
        {
            _jsonReader.Read();//startObj
            //_builder.StartEntity();
            var entity = new Entity(schema);
            return entity;
        }
        public Entity ReadEndObject()
        {
            _jsonReader.Read();//endObj
            //_builder.EndEntity();
            return null;
        }
        public Relation ReadRelation(Constraint schema)
        {
            _jsonReader.Read();//propName
            //_builder.StartRelation();
            var relation = new Relation(schema);
            return relation;
        }

        public Union ReadUnion(UnionClass schema)
        {
            //_builder.StartUnion();
            _jsonReader.Read();//startArr
            return new Union(schema);
        }
        public Union ReadEndUnion()
        {
            _jsonReader.Read();//endArr
            return null;
        }
    }
}
