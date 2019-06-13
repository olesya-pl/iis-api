using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Schema;
using IIS.Ontology.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace IIS.Schema.EntityFramework
{
    public class SchemaProvider : ISchemaProvider
    {
        private readonly Dictionary<string, ComplexType> _types = new Dictionary<string, ComplexType>();
        private readonly ContourContext _context;

        public SchemaProvider(ContourContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public async Task<ComplexType> GetSchemaAsync()
        {
            var types = await GetTypesAsync();

            var schemaType = new ComplexType { Name = "Entities", Kind = Kind.Class };

            foreach (var typeEntity in types)
            {
                var type = MapType(typeEntity);
                var relationName = type.Name.ToLowerCamelcase();
                schemaType.AddComplexType(relationName, type, true, true);
            }

            return schemaType;
        }

        private ComplexType MapType(OTypeEntity typeEntity)
        {
            if (_types.ContainsKey(typeEntity.Code)) return _types[typeEntity.Code];

            var type = _types[typeEntity.Code] = new ComplexType
            {
                Name = typeEntity.Code,
                Kind = typeEntity.IsAbstract ? Kind.Interface : Kind.Class
            };

            if (typeEntity.Parent != null)
            {
                var parent = MapType(typeEntity.Parent);
                type.Parents.Add(parent);
            }

            var relationInfo = GetOrCreateRelationInfoType();
            type.AddComplexType("_relationInfo", relationInfo);
            type.AddAttribute("id", ScalarType.Keyword, true);

            var constraintsByName = typeEntity.ForwardRestrictions.GroupBy(r => r.Type.Code);
            foreach (var constraints in constraintsByName)
            {
                var name = constraints.Key;
                var constraint = constraints.First();
                var isArray = constraint.IsMultiple;
                var isRequired = constraint.IsRequired;
                var kind = constraints.Count() > 1 ? Core.Kind.Union : Core.Kind.Class;

                if (kind == Core.Kind.Class)
                {
                    var target = MapType(constraint.Target);
                    type.AddComplexType(name, target, isRequired, isArray);
                }
                else if (kind == Core.Kind.Union)
                {
                    var unionName = $"{type.Name}{name.Camelize()}RelationUnion";
                    var targets = constraints.Select(e => MapType(e.Target)).ToArray();
                    var union = new ComplexType { Name = unionName, Kind = Kind.Union };

                    foreach (var target in targets) target.Parents.Add(union);

                    type.AddComplexType(name, union, isRequired, isArray);
                }
            }

            foreach (var constraint in typeEntity.AttributeRestrictions)
            {
                var attribute = constraint.Attribute;
                type.AddAttribute(attribute.Code, attribute.Kind, constraint.IsRequired, constraint.IsMultiple);
            }

            return type;
        }

        private async Task<IEnumerable<OTypeEntity>> GetTypesAsync()
        {
            return await _context.Types
               .Include(_ => _.Parent)
               .Include(_ => _.AttributeRestrictions).ThenInclude(_ => _.Attribute)
               .OfType<OTypeEntity>()
               .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.Type)
               .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.Target)
               .ToArrayAsync();
        }

        private ComplexType GetOrCreateRelationInfoType()
        {
            var typeName = "RelationInfo";

            if (_types.ContainsKey(typeName)) return _types[typeName];

            var relationInfo = new ComplexType { Name = typeName, Kind = Kind.Class };
            relationInfo.AddAttribute("id", ScalarType.Keyword, true);
            relationInfo.AddAttribute("startsAt", ScalarType.Date);
            relationInfo.AddAttribute("endsAt", ScalarType.Date);
            relationInfo.AddAttribute("createdAt", ScalarType.Date, true);
            relationInfo.AddAttribute("isInferred", ScalarType.Int, true);
            _types.Add(relationInfo.Name, relationInfo);

            return relationInfo;
        }
    }
}
