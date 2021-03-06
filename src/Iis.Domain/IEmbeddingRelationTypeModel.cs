using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using System.Collections.Generic;

namespace Iis.Domain
{
    public interface IEmbeddingRelationTypeModel: IRelationTypeModel
    {
        IEmbeddingRelationTypeModel DirectRelationType { get; }
        IRelationMetaBase EmbeddingMeta { get; }
        EmbeddingOptions EmbeddingOptions { get; }
        IEntityTypeModel EntityType { get; }
        IAttributeTypeModel AttributeType { get; }
        bool IsAttributeType { get; }
        bool IsEntityType { get; }
        bool IsInversed { get; }
        INodeTypeModel TargetType { get; }
        bool AcceptsOperation(EntityOperation create);
    }
}