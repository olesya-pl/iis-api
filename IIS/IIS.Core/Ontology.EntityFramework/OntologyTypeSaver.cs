using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Ontology.Meta;
using Microsoft.AspNetCore.Hosting;

namespace IIS.Core.Ontology.EntityFramework
{
    // Todo: move it to OntologyTypeService or create separate interface
    public class OntologyTypeSaver
    {
        private Context.OntologyContext _ontologyContext;
        private Dictionary<Guid, Context.Type> _types = new Dictionary<Guid, Context.Type>();

        public OntologyTypeSaver(Context.OntologyContext ontologyContext)
        {
            _ontologyContext = ontologyContext;
        }

        [Obsolete]
        public void ClearTypes()
        {
            _types.Clear();
            _ontologyContext.Types.RemoveRange(_ontologyContext.Types.ToArray());
            _ontologyContext.AttributeTypes.RemoveRange(_ontologyContext.AttributeTypes.ToArray());
            _ontologyContext.RelationTypes.RemoveRange(_ontologyContext.RelationTypes.ToArray());
            _ontologyContext.SaveChanges();
        }

        public async Task ClearTypesAsync()
        {
            _types.Clear();
            _ontologyContext.Types.RemoveRange(_ontologyContext.Types.ToArray());
            _ontologyContext.AttributeTypes.RemoveRange(_ontologyContext.AttributeTypes.ToArray());
            _ontologyContext.RelationTypes.RemoveRange(_ontologyContext.RelationTypes.ToArray());
            await _ontologyContext.SaveChangesAsync();
        }

        public void SaveTypes(IEnumerable<Type> types)
        {
            // for `types` you need to pass only EntityTypes and/or AttributeTypes
            // RelationTypes are walked recursively based on that 2 types
            ClearTypes();
            foreach (var type in types)
                SaveType(type);

            _ontologyContext.AddRange(_types.Values);
            _ontologyContext.SaveChanges();
        }

        private Context.Type SaveType(Type type, Type relationSourceType = null)
        {
            if (type.Id == Guid.Empty)
                throw new ArgumentException(nameof(type));
            if (_types.ContainsKey(type.Id))
                return _types[type.Id];

            if (type is EmbeddingRelationType rel && rel.IsInversed)
            {
                // inversed relations are virtual and shouldn't be saved
                return null;
            }

            var result = new Context.Type();
            result.Name = type.Name;
            result.Title = type.Title;
            result.Meta = type.Meta?.Serialize().ToString();
            result.IsArchived = false;
            result.IsAbstract = false;
            _types.Add(type.Id, result); // Add to created types cache

            // Filling specific properties for different types
            if (type is EntityType et)
            {
                result.IsAbstract = et.IsAbstract;
                result.Kind = Context.Kind.Entity;
            }
            else if (type is AttributeType at)
            {
                result.Kind = Context.Kind.Attribute;
                result.AttributeType = new Context.AttributeType
                {
                    ScalarType = Map(at.ScalarTypeEnum),
                };
            }
            else if (type is RelationType rt)
            {
                result.Kind = Context.Kind.Relation;

                if (type is EmbeddingRelationType ert)
                {
                    result.RelationType = new Context.RelationType
                    {
                        Kind = Context.RelationKind.Embedding,
                        EmbeddingOptions = Map(ert.EmbeddingOptions),
                        TargetType = SaveType(ert.TargetType),
                    };
                }
                else if (type is InheritanceRelationType irt)
                {
                    result.RelationType = new Context.RelationType
                    {
                        Kind = Context.RelationKind.Inheritance,
                        EmbeddingOptions = Context.EmbeddingOptions.None,
                        TargetType = SaveType(irt.ParentType),
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (relationSourceType == null)
                    throw new ArgumentNullException($"Unable to create relation type \"{type.Name}\" without specified source type");

                result.RelationType.SourceType = SaveType(relationSourceType);
            }
            else throw new NotImplementedException();

            foreach (var node in type.RelatedTypes.OfType<RelationType>()) // saving only relation nodes
                SaveType(node, type);

            return result;
        }

        private static Context.ScalarType Map(ScalarType scalarType)
        {
            switch (scalarType)
            {
                case ScalarType.Boolean: return Context.ScalarType.Boolean;
                case ScalarType.DateTime: return Context.ScalarType.Date;
                case ScalarType.Decimal: return Context.ScalarType.Decimal;
                case ScalarType.File: return Context.ScalarType.File;
                case ScalarType.Geo: return Context.ScalarType.Geo;
                case ScalarType.Integer: return Context.ScalarType.Int;
                case ScalarType.String: return Context.ScalarType.String;
                default: throw new NotImplementedException();
            }
        }

        private static Context.EmbeddingOptions Map(EmbeddingOptions embeddingOptions)
        {
            switch (embeddingOptions)
            {
                case EmbeddingOptions.Optional: return Context.EmbeddingOptions.Optional;
                case EmbeddingOptions.Required: return Context.EmbeddingOptions.Required;
                case EmbeddingOptions.Multiple: return Context.EmbeddingOptions.Multiple;
                default: throw new ArgumentOutOfRangeException(nameof(embeddingOptions), embeddingOptions, null);
            }
        }


    }
}
