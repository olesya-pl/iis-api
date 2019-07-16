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
            var oos = new Type
            {
                Id = Guid.NewGuid(), Kind = Kind.Entity, Name = "ObjectOfStudy", Title = "Объект разведки",
                IsAbstract = true
            };
            var person = new Type {Id = Guid.NewGuid(), Kind = Kind.Entity, Name = "Person", Title = "Человек"};
            var p_rel_t = new Type {Id = Guid.NewGuid(), Kind = Kind.Relation, Name = "INHERITS"};
            var p_rel = new RelationType {Id = p_rel_t.Id, SourceType = person, TargetType = oos, Kind = RelationKind.Inheritance};
            var attrguid = Guid.NewGuid();
            var nameattr = new Type
            {
                Id = Guid.NewGuid(), Kind = Kind.Attribute, Name = "Name", Title = "Название",
                AttributeType = new AttributeType
                    {Id = attrguid, ScalarType = Core.Ontology.EntityFramework.Context.ScalarType.String}
            };
            var rel_t = new Type {Id = Guid.NewGuid(), Kind = Kind.Relation, Name = "FirstName", Title = "Имя"};
            var rel = new RelationType {Id = rel_t.Id, SourceType = person, TargetType = nameattr};
            
            context.Add(oos);
            context.Add(person);
            context.Add(p_rel_t);
            context.Add(p_rel);
            context.Add(nameattr);
            context.Add(rel_t);
            context.Add(rel);
            context.SaveChanges();

            return "Yep.";
        }
    }
}