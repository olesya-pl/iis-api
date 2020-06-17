using System;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public abstract class BaseOntologyProvider
    {
        protected virtual RelationType _addInversedRelation(RelationType relation, NodeType sourceType)
        {
            if (!(relation is EmbeddingRelationType relationType) || !relationType.HasInversed())
                return null;

            var meta = relationType.GetInversed();
            var embeddingOptions = meta.Multiple ? EmbeddingOptions.Multiple : EmbeddingOptions.Optional;
            var name = meta.Code ?? sourceType.Name.ToLowerCamelcase();
            var inversedRelation = new EmbeddingRelationType(Guid.NewGuid(), name, embeddingOptions, isInversed: true)
            {
                Title = meta.Title ?? sourceType.Title ?? name
            };

            inversedRelation.AddType(sourceType);
            inversedRelation.AddType(relationType);
            relationType.TargetType.AddType(inversedRelation);

            return inversedRelation;
        }
    }
}