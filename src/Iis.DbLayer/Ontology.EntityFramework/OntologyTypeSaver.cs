using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    // Todo: move it to OntologyTypeService or create separate interface
    public class OntologyTypeSaver
    {
        private OntologyContext _ontologyContext;
        private Dictionary<Guid, NodeTypeEntity> _types = new Dictionary<Guid, NodeTypeEntity>();

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

        public void SaveTypes(IEnumerable<INodeTypeModel> types)
        {
            // for `types` you need to pass only EntityTypes and/or AttributeTypes
            // RelationTypes are walked recursively based on that 2 types
            ClearTypes();
            foreach (INodeTypeModel type in types)
                SaveType(type);

            _ontologyContext.NodeTypes.AddRange(_types.Values);
            _ontologyContext.SaveChanges();
        }

        private NodeTypeEntity SaveType(INodeTypeModel type, INodeTypeModel relationSourceType = null)
        {
            if (type.Id == Guid.Empty)
                throw new ArgumentException(nameof(type));
            if (_types.ContainsKey(type.Id))
                return _types[type.Id];

            if (type is IEmbeddingRelationTypeModel rel && rel.IsInversed)
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
            else if (type is IAttributeTypeModel at)
            {
                result.Kind = Kind.Attribute;
                result.IAttributeTypeModel = new AttributeTypeEntity
                {
                    ScalarType = at.ScalarTypeEnum,
                };
            }
            else if (type is IRelationTypeModel rt)
            {
                result.Kind = Kind.Relation;

                if (type is IEmbeddingRelationTypeModel ert)
                {
                    result.RelationType = new RelationTypeEntity
                    {
                        Kind = RelationKind.Embedding,
                        EmbeddingOptions = Map(ert.EmbeddingOptions),
                        TargetType = SaveType(ert.TargetType),
                    };
                }
                else if (type is InheritanceRelationType irt)
                {
                    result.RelationType = new RelationTypeEntity
                    {
                        Kind = RelationKind.Inheritance,
                        EmbeddingOptions = Interfaces.Ontology.Schema.EmbeddingOptions.None,
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

            foreach (var node in type.RelatedTypes.OfType<IRelationTypeModel>()) // saving only relation nodes
                SaveType(node, type);

            return result;
        }
        private static Interfaces.Ontology.Schema.EmbeddingOptions Map(EmbeddingOptions embeddingOptions)
        {
            switch (embeddingOptions)
            {
                case EmbeddingOptions.Optional: return Interfaces.Ontology.Schema.EmbeddingOptions.Optional;
                case EmbeddingOptions.Required: return Interfaces.Ontology.Schema.EmbeddingOptions.Required;
                case EmbeddingOptions.Multiple: return Interfaces.Ontology.Schema.EmbeddingOptions.Multiple;
                default: throw new ArgumentOutOfRangeException(nameof(embeddingOptions), embeddingOptions, null);
            }
        }


    }
}
