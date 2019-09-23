using System;
using System.Collections.Generic;
using System.Linq;
using IIS.Core.Ontology.Meta;
using Microsoft.AspNetCore.Hosting;

namespace IIS.Core.Ontology.EntityFramework
{
    // Todo: move it to OntologyTypeService or create separate interface
    public class OntologyTypeSaver
    {
        private Context.OntologyContext _ontologyContext;
        private readonly IApplicationLifetime _applicationLifetime;
        private Dictionary<Guid, Context.Type> _types = new Dictionary<Guid, Context.Type>();

        public OntologyTypeSaver(Context.OntologyContext ontologyContext, IApplicationLifetime applicationLifetime)
        {
            _ontologyContext = ontologyContext;
            _applicationLifetime = applicationLifetime;
        }

        public void ClearTypes()
        {
            _types.Clear();
            _ontologyContext.Types.RemoveRange(_ontologyContext.Types.ToArray());
            _ontologyContext.AttributeTypes.RemoveRange(_ontologyContext.AttributeTypes.ToArray());
            _ontologyContext.RelationTypes.RemoveRange(_ontologyContext.RelationTypes.ToArray());
            _ontologyContext.SaveChanges();
        }

        public void SaveTypes(IEnumerable<Type> types)
        {
            ClearTypes();
            foreach (var type in types)
                SaveType(type);

            _ontologyContext.AddRange(_types.Values);
            _ontologyContext.SaveChanges();
            _applicationLifetime.StopApplication();
        }

        private Context.Type SaveType(Type type, Type relationSource = null)
        {
            if (type.Id == Guid.Empty)
                throw new ArgumentException(nameof(type));
            if (_types.ContainsKey(type.Id))
                return _types[type.Id];

            var result = new Context.Type();
            result.Name = type.Name;
            result.Title = type.Title;
            result.Meta = type.Meta?.Serialize().ToString();
            result.IsArchived = false;
            result.IsAbstract = false;
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
                if (type is EmbeddingRelationType ert)
                    result.RelationType = new Context.RelationType
                    {
                        Kind = Context.RelationKind.Embedding,
                        EmbeddingOptions = Map(ert.EmbeddingOptions),
                        TargetType = SaveType(ert.TargetType),
                    };
                else if (type is InheritanceRelationType irt)
                    result.RelationType = new Context.RelationType
                    {
                        Kind = Context.RelationKind.Inheritance,
                        EmbeddingOptions = Context.EmbeddingOptions.None,
                        TargetType = SaveType(irt.ParentType),
                    };
                else throw new NotImplementedException();
                if (relationSource == null) throw new ArgumentNullException(nameof(relationSource));

                result.Kind = Context.Kind.Relation;
                result.RelationType.SourceType = SaveType(relationSource);
            }
            else throw new NotImplementedException();

            _types.Add(type.Id, result); // Add to created types cache

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
