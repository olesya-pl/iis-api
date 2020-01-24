using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain;
using Iis.Domain.Meta;
using Microsoft.AspNetCore.Hosting;
using EmbeddingOptions = Iis.Domain.EmbeddingOptions;

namespace IIS.Core.Ontology.EntityFramework
{
    // Todo: move it to OntologyTypeService or create separate interface
    public class OntologyTypeSaver
    {
        private OntologyContext _ontologyContext;
        private Dictionary<Guid, Iis.DataModel.NodeTypeEntity> _types = new Dictionary<Guid, Iis.DataModel.NodeTypeEntity>();

        public OntologyTypeSaver(OntologyContext ontologyContext)
        {
            _ontologyContext = ontologyContext;
        }

        [Obsolete]
        public void ClearTypes()
        {
            _types.Clear();
            _ontologyContext.NodeTypes.RemoveRange(_ontologyContext.NodeTypes.ToArray());
            _ontologyContext.AttributeTypes.RemoveRange(_ontologyContext.AttributeTypes.ToArray());
            _ontologyContext.RelationTypes.RemoveRange(_ontologyContext.RelationTypes.ToArray());
            _ontologyContext.SaveChanges();
        }

        public async Task ClearTypesAsync()
        {
            _types.Clear();

            _ontologyContext.AttributeTypes.RemoveRange(_ontologyContext.AttributeTypes);
            _ontologyContext.RelationTypes.RemoveRange(_ontologyContext.RelationTypes);
            _ontologyContext.NodeTypes.RemoveRange(_ontologyContext.NodeTypes);
            await _ontologyContext.SaveChangesAsync();
        }

        public void SaveTypes(IEnumerable<NodeType> types)
        {
            // for `types` you need to pass only EntityTypes and/or AttributeTypes
            // RelationTypes are walked recursively based on that 2 types
            ClearTypes();
            foreach (NodeType type in types)
                SaveType(type);

            _ontologyContext.NodeTypes.AddRange(_types.Values);
            _ontologyContext.SaveChanges();
        }

        private Iis.DataModel.NodeTypeEntity SaveType(NodeType type, NodeType relationSourceType = null)
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

            var result = new NodeTypeEntity();
            result.Id = type.Id;
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
                result.Kind = Kind.Entity;
            }
            else if (type is AttributeType at)
            {
                result.Kind = Kind.Attribute;
                result.AttributeType = new Iis.DataModel.AttributeTypeEntity
                {
                    ScalarType = Map(at.ScalarTypeEnum),
                };
            }
            else if (type is RelationType rt)
            {
                result.Kind = Kind.Relation;

                if (type is EmbeddingRelationType ert)
                {
                    result.RelationType = new Iis.DataModel.RelationTypeEntity
                    {
                        Kind = RelationKind.Embedding,
                        EmbeddingOptions = Map(ert.EmbeddingOptions),
                        TargetType = SaveType(ert.TargetType),
                    };
                }
                else if (type is InheritanceRelationType irt)
                {
                    result.RelationType = new Iis.DataModel.RelationTypeEntity
                    {
                        Kind = RelationKind.Inheritance,
                        EmbeddingOptions = Iis.DataModel.EmbeddingOptions.None,
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

        private static Iis.DataModel.ScalarType Map(Iis.Domain.ScalarType scalarType)
        {
            switch (scalarType)
            {
                case Iis.Domain.ScalarType.Boolean: return Iis.DataModel.ScalarType.Boolean;
                case Iis.Domain.ScalarType.DateTime: return Iis.DataModel.ScalarType.Date;
                case Iis.Domain.ScalarType.Decimal: return Iis.DataModel.ScalarType.Decimal;
                case Iis.Domain.ScalarType.File: return Iis.DataModel.ScalarType.File;
                case Iis.Domain.ScalarType.Geo: return Iis.DataModel.ScalarType.Geo;
                case Iis.Domain.ScalarType.Integer: return Iis.DataModel.ScalarType.Int;
                case Iis.Domain.ScalarType.String: return Iis.DataModel.ScalarType.String;
                default: throw new NotImplementedException();
            }
        }

        private static Iis.DataModel.EmbeddingOptions Map(EmbeddingOptions embeddingOptions)
        {
            switch (embeddingOptions)
            {
                case EmbeddingOptions.Optional: return Iis.DataModel.EmbeddingOptions.Optional;
                case EmbeddingOptions.Required: return Iis.DataModel.EmbeddingOptions.Required;
                case EmbeddingOptions.Multiple: return Iis.DataModel.EmbeddingOptions.Multiple;
                default: throw new ArgumentOutOfRangeException(nameof(embeddingOptions), embeddingOptions, null);
            }
        }


    }
}
