using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Entities
{
    public static class Resolvers
    {
        private const string LastRelation = "LastRelation";
        
        public static async Task<object> ResolveEntityRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            // todo: pass real relation here
            ctx.ScopedContextData = ctx.ScopedContextData.SetItem(LastRelation, new Relation()); // Pass relation to _relation field
            var node = new Entity(); // parent.GetNodeByRelationType(relationType)
            return node;
            throw new NotImplementedException();
        }
        
        public static async Task<object> ResolveAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            switch (relationType.AttributeType.ScalarTypeEnum)
            {
                case Core.Ontology.ScalarType.String:
                    return "Hello";
                case Core.Ontology.ScalarType.Integer:
                    return 42;
                case Core.Ontology.ScalarType.Decimal:
                    return 4.2;
                case Core.Ontology.ScalarType.Boolean:
                    return true;
                case Core.Ontology.ScalarType.DateTime:
                    return DateTime.Now;
                case Core.Ontology.ScalarType.Geo:
                    throw new NotImplementedException();
                case Core.Ontology.ScalarType.File:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new NotImplementedException();
        }
        
        public static async Task<Relation> ResolveParentRelation(IResolverContext ctx)
        {
            // resolver for "_relation" field on schema
            // todo: You should resolve source-to-current entity relation here. Pass relation info through context.
            return (Relation) ctx.ScopedContextData.GetValueOrDefault(LastRelation);
            throw new NotImplementedException();
        }

        public static async Task<Guid> ResolveId(IResolverContext ctx)
        {
            return ctx.Parent<Node>().Id;
        }

        public static async Task<Node> ResolveEntity(IResolverContext ctx, EntityType type)
        {
            var id = ctx.Argument<Guid>("id");
            return new Entity();
            throw new NotImplementedException();
        }

        public static async Task<IEnumerable<Node>> ResolveEntityList(IResolverContext ctx, EntityType type)
        {
            return new[] {new Entity(), new Entity(), new Entity(),};
            throw new NotImplementedException();
        }

        public static ObjectType ResolveAbstractType(IResolverContext context, object resolverResult)
        {
            var node = (Node) resolverResult;
            var typeName = "CountryEntity"; // OntologyObjectType.GetName(node.Type)
            return context.Schema.GetType<ObjectType>(typeName);
//            return (ObjectType) repository.GetType<IOntologyType>(node.Type.Name);
            throw new NotImplementedException();
        }
    }
}