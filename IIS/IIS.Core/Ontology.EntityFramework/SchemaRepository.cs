using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core;
using Microsoft.EntityFrameworkCore;

namespace IIS.Ontology.EntityFramework
{
    public class SchemaRepository : IOSchema
    {
        private readonly ContourContext _context;
        private TypeEntity _schema;
        private readonly IDictionary<string, IRelationResolver> _resolvers;

        public SchemaRepository(ContourContext context, IDictionary<string, IRelationResolver> resolvers)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _resolvers = resolvers; // todo: replace this with resolver factory
        }

        public async Task<TypeEntity> GetRootAsync()
        {
            if (_schema != null) return _schema;

            var data = await _context.Types
                .Include(_ => _.Parent)
                .Include(_ => _.AttributeRestrictions).ThenInclude(_ => _.Attribute)
                .OfType<OTypeEntity>()
                .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.Type)
                .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.Target)
                .ToArrayAsync();

            var schemaType = new TypeEntity("Entities");
            if (_types.ContainsKey("RelationInfo"))
                schemaType.AddType("_relationInfo", _types["RelationInfo"], true, true, _resolvers["relationInfo"]);
            else
            {
                var attrResolver = _resolvers["attribute"];
                var relationInfo = new TypeEntity("RelationInfo");
                relationInfo.AddAttribute("id", ScalarType.Keyword, true, false, attrResolver);
                relationInfo.AddAttribute("startsAt", ScalarType.Date, false, false, attrResolver);
                relationInfo.AddAttribute("endsAt", ScalarType.Date, false, false, attrResolver);
                relationInfo.AddAttribute("createdAt", ScalarType.Date, true, false, attrResolver);
                relationInfo.AddAttribute("isInferred", ScalarType.Int, true, false, attrResolver);
                _types.Add(relationInfo.Name, relationInfo);
                schemaType.AddType("_relationInfo", relationInfo, true, true, _resolvers["relationInfo"]);
            }
            var resolver = _resolvers["entities"];
            foreach (var item in data)
            {
                var type = MapType(item);
                schemaType.AddType(type.Name.ToLowerCamelcase(), type, true, true, resolver);
            }
            _schema = schemaType;
            return schemaType;
        }

        private static readonly Dictionary<string, TypeEntity> _types = new Dictionary<string, TypeEntity>();
        private TypeEntity MapType(OTypeEntity typeEntity)
        {
            if (_types.ContainsKey(typeEntity.Code)) return _types[typeEntity.Code];

            var parent = typeEntity.Parent == null ? null : MapType(typeEntity.Parent);
            var type = _types[typeEntity.Code] = new TypeEntity(typeEntity.Code, typeEntity.IsAbstract, parent);
            //type.IndexConfig = typeEntity.Meta.ContainsKey("index") ?
            //typeEntity.Meta.GetValue("index") as JObject : new JObject();
            //var relationResolver = new (this, _contextAccessor);
            var relationInfo = _types["RelationInfo"];
            type.AddType("_relationInfo", relationInfo, false, false, _resolvers["relationInfo"]);
            type.AddType("id", new AttributeClass("id", ScalarType.Keyword), true, false, _resolvers["attribute"]);

            var constraintsByName = typeEntity.ForwardRestrictions.GroupBy(r => r.Type.Code);
            foreach (var constraints in constraintsByName)
            {
                var name = constraints.Key;
                var constraint = constraints.First();
                var isArray = constraint.IsMultiple;
                var isRequired = constraint.IsRequired;
                var kind = constraints.Count() > 1 ? Kind.Union : Kind.Class;

                if (kind == Kind.Class)
                {
                    var resolver = _resolvers["entityRelation"];
                    var target = MapType(constraint.Target);
                    type.AddType(name, target, isRequired, isArray, resolver);
                }
                else if (kind == Kind.Union)
                {
                    var resolver = _resolvers["entityRelation"];
                    var targets = constraints.Select(e => MapType(e.Target));
                    var target = new UnionClass($"{type.Name}{name.Camelize()}RelationUnion", targets);
                    type.AddType(name, target, isRequired, isArray, resolver);
                }
            }

            foreach (var constraint in typeEntity.AttributeRestrictions)
            {
                var resolver = _resolvers["attribute"];
                type.AddAttribute(
                    constraint.Attribute.Code,
                    constraint.Attribute.Kind,
                    constraint.IsRequired, 
                    constraint.IsMultiple, 
                    resolver);
            }

            return type;
        }
    }
}
