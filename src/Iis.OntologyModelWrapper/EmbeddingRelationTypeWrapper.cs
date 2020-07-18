using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class EmbeddingRelationTypeWrapper : RelationTypeWrapper, IEmbeddingRelationTypeModel
    {
        public EmbeddingRelationTypeWrapper(INodeTypeLinked source) : base(source) { }
        public IEmbeddingRelationTypeModel DirectRelationType => throw new NotImplementedException();

        public IRelationMetaBase EmbeddingMeta => throw new NotImplementedException();

        public EmbeddingOptions EmbeddingOptions => throw new NotImplementedException();

        public IEntityTypeModel EntityType => throw new NotImplementedException();

        public IAttributeTypeModel IAttributeTypeModel => throw new NotImplementedException();

        public bool IsAttributeType => throw new NotImplementedException();

        public bool IsEntityType => throw new NotImplementedException();

        public bool IsInversed => _source.IsInversed;

        public IEnumerable<IRelationTypeModel> RelationTypes => throw new NotImplementedException();

        public INodeTypeModel TargetType => throw new NotImplementedException();

        public bool AcceptsOperation(EntityOperation create)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => Name;
    }
}
