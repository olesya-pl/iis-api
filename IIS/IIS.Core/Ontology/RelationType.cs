using System;

namespace IIS.Core.Ontology
{
    public abstract class RelationType : Type
    {
        public static RelationType Build(Guid id, string name, EmbeddingOptions options = EmbeddingOptions.Optional)
        {
            RelationType relation;

            if (InheritanceRelationType.RelationName == name)
                relation = new InheritanceRelationType(id);
            else
                relation = new EmbeddingRelationType(id, name, options);

            return relation;
        }

        protected RelationType(Guid id, string name)
            : base(id, name)
        {

        }
    }
}
