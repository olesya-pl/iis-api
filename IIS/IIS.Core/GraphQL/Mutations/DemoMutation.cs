using System;
using HotChocolate;
using IIS.Core.Ontology.EntityFramework.Context;
using Type = IIS.Core.Ontology.EntityFramework.Context.Type;

namespace IIS.Core.GraphQL.Mutations
{
    public class DemoMutation
    {
        private static string _savedString;
        
        public string SaveString(string param)
        {
            var old = _savedString;
            _savedString = param;
            return old;
        }

        public string ClearDatabase([Service] OntologyContext context)
        {
            context.Types.RemoveRange(context.Types);
            context.SaveChanges();
            return "Cleared.";
        }
        
        public string FillDummyDatabase([Service] OntologyContext context)
        {
            var oos = CreateEntity(context, "ObjectOfStudy", "Объект разведки", true);
            var nameattr = CreateAttribute(context, "Name", "Название", Core.Ontology.EntityFramework.Context.ScalarType.String);
            CreateRelation(context, oos, nameattr, RelationKind.Embedding);
            var person = CreateEntity(context, "Person", "Человек");
            CreateRelation(context, person, oos, RelationKind.Inheritance);
            var car = CreateEntity(context, "Car", "Автомобиль");
            CreateRelation(context, car, oos, RelationKind.Inheritance);
            CreateRelation(context, person, car, RelationKind.Embedding);
            var ageattr = CreateAttribute(context, "Age", "Возраст", Ontology.EntityFramework.Context.ScalarType.Int);
            CreateRelation(context, person, ageattr, RelationKind.Embedding);
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

        private Type CreateAttribute(OntologyContext context, string name, string title, Ontology.EntityFramework.Context.ScalarType scalarType)
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
    }
}