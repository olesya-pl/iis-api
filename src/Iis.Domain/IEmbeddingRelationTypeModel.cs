using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using System.Collections.Generic;

namespace Iis.Domain
{
    public interface IEmbeddingRelationTypeModel: IRelationTypeModel
    {
        ISchemaMeta EmbeddingMeta { get; }
        EmbeddingOptions EmbeddingOptions { get; }
        INodeTypeModel EntityType { get; }
        INodeTypeModel AttributeType { get; }
        bool IsAttributeType { get; }
        bool IsEntityType { get; }
        bool IsInversed { get; }
        INodeTypeModel TargetType { get; }
        bool AcceptsOperation(EntityOperation create);
    }
}