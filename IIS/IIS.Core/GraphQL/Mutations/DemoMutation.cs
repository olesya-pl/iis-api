using System;
using System.Linq;
using HotChocolate;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Legacy.EntityFramework;
using Newtonsoft.Json.Linq;
using AttributeType = IIS.Core.Ontology.EntityFramework.Context.AttributeType;
using RelationType = IIS.Core.Ontology.EntityFramework.Context.RelationType;
using Type = IIS.Core.Ontology.EntityFramework.Context.Type;

namespace IIS.Core.GraphQL.Mutations
{
    public class DemoMutation
    {
        private static string _savedString;
        private static JObject _jobject;

        public string SaveString(string param)
        {
            var old = _savedString;
            _savedString = param;
            return old;
        }

        public string FillDummyTypes([Service] OntologyContext context)
        {
            var oos = CreateEntity(context, "ObjectOfStudy", "Объект разведки", true);
            var nameattr = CreateAttribute(context, "Name", "Название", Core.Ontology.EntityFramework.Context.ScalarType.String);
            CreateRelation(context, oos, nameattr, RelationKind.Embedding);
            var person = CreateEntity(context, "Person", "Человек");
            CreateRelation(context, person, oos, RelationKind.Inheritance);
            var car = CreateEntity(context, "Car", "Автомобиль");
            CreateRelation(context, car, oos, RelationKind.Inheritance);
            CreateRelation(context, person, car, RelationKind.Embedding);
            var ageattr = CreateAttribute(context, "Age", "Возраст", Core.Ontology.EntityFramework.Context.ScalarType.Int);
            CreateRelation(context, person, ageattr, RelationKind.Embedding);
            var homeattr = CreateAttribute(context, "HomeLocation", "Домашний адрес", Core.Ontology.EntityFramework.Context.ScalarType.Geo);
            CreateRelation(context, person, homeattr, RelationKind.Embedding);
            var photoattr = CreateAttribute(context, "Photo", "Фотография", Core.Ontology.EntityFramework.Context.ScalarType.File);
            CreateRelation(context, person, photoattr, RelationKind.Embedding);
            var birthattr = CreateAttribute(context, "BirthDate", "Дата рождения", Core.Ontology.EntityFramework.Context.ScalarType.Date);
            CreateRelation(context, person, birthattr, RelationKind.Embedding);
            context.SaveChanges();

            return "Yep.";
        }

        private RelationType CreateRelation(OntologyContext context, Type source, Type target, RelationKind kind)
        {
            var type = new Type
            {
                Id = Guid.NewGuid(), Kind = Kind.Relation, Name = kind.ToString(),
                Title = $"{source.Name}->{target.Name}",
                Meta = "{\"strProp\":\"relationDummy\",\"intProp\":42}"
            };
            var rel = new RelationType {Id = type.Id, SourceType = source, TargetType = target, Kind = kind};
            context.Add(rel);
            context.Add(type);
            return rel;
        }

        private Type CreateAttribute(OntologyContext context, string name, string title, Core.Ontology.EntityFramework.Context.ScalarType scalarType)
        {
            var guid = Guid.NewGuid();
            var attr = new Type
            {
                Id = guid, Kind = Kind.Attribute, Name = name, Title = title,
                AttributeType = new AttributeType {Id = guid, ScalarType = scalarType},
                Meta = "{\"strProp\":\"attributeDummy\",\"intProp\":42}"
            };
            context.Add(attr);
            return attr;
        }

        private Type CreateEntity(OntologyContext context, string name, string title, bool isAbstract = false)
        {
            var type = new Type
            {
                Id = Guid.NewGuid(), Kind = Kind.Entity, Name = name, Title = title, IsAbstract = isAbstract,
                Meta = "{\"strProp\":\"entityDummy\",\"intProp\":42}"
            };
            context.Add(type);
            return type;
        }

        [GraphQLType(typeof(JsonScalarType))]
        public JObject SaveObject([GraphQLType(typeof(JsonScalarType))] JObject data)
        {
            var old = _jobject;
            _jobject = data;
            return old;
        }

        [GraphQLType(typeof(GeoJsonScalarType))]
        public JObject SaveGeo([GraphQLType(typeof(GeoJsonScalarType))] JObject data)
        {
            var old = _jobject;
            _jobject = data;
            return old;
        }

        public string ClearTypes([Service] OntologyTypeSaver typeSaver)
        {
            typeSaver.ClearTypes();
            return "Types cleared";
        }

        public string MigrateLegacyTypes([Service] ILegacyOntologyProvider provider, [Service] OntologyTypeSaver typeSaver)
        {
            var task = provider.GetTypesAsync();
            task.Wait();
            typeSaver.SaveTypes(task.Result);
            return "Types migrated";
        }

        public string ClearEntities([Service] OntologyContext context)
        {
            context.Nodes.RemoveRange(context.Nodes.ToArray());
            context.Attributes.RemoveRange(context.Attributes.ToArray());
            context.Relations.RemoveRange(context.Relations.ToArray());
            context.SaveChanges();
            return "Entities cleared.";
        }
    }
}
