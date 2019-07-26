using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.Mocks
{
    public class EntityRelationMock<T> : IOntologyProvider where T : IOntologyProvider
    {
        private readonly T _ontologyProvider;

        public EntityRelationMock(T ontologyProvider)
        {
            _ontologyProvider = ontologyProvider;
        }

        public async Task<IEnumerable<Type>> GetTypesAsync(CancellationToken cancellationToken = default)
        {
            var types = await _ontologyProvider.GetTypesAsync();
            var entityTypes = types.OfType<EntityType>();
            var requiredArr = entityTypes.Single(t => t.Name == "Event");
            var required = entityTypes.Single(t => t.Name == "Enum");
            var optional = entityTypes.Single(t => t.Name == "ObjectAffiliation");
            var tag = entityTypes.Single(t => t.Name == "Tag");
            foreach (var et in entityTypes)
            {
                var r0 = new EmbeddingRelationType(Guid.NewGuid(), "TagsMOCK", EmbeddingOptions.Multiple);
                r0.AddType(tag);
                var r1 = new EmbeddingRelationType(Guid.NewGuid(), "AssociatedWithEventMOCK", EmbeddingOptions.Multiple);
                r1.AddType(requiredArr);
                var r2 = new EmbeddingRelationType(Guid.NewGuid(), "HasEnumMOCK", EmbeddingOptions.Required);
                r2.AddType(required);
                var r3 = new EmbeddingRelationType(Guid.NewGuid(), "AffiliatesMOCK", EmbeddingOptions.Optional);
                r3.AddType(optional);
                et.AddType(r0);
                et.AddType(r1);
                et.AddType(r2);
                et.AddType(r3);
            }
            return types;
        }
    }
}