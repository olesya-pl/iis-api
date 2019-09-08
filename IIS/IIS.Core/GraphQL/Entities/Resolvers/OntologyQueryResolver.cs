using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flee.PublicTypes;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Core.Ontology.Meta;
using Microsoft.EntityFrameworkCore;
using Attribute = IIS.Core.Ontology.Attribute;
using EmbeddingOptions = IIS.Core.Ontology.EmbeddingOptions;
using Node = IIS.Core.Ontology.Node;
using Relation = IIS.Core.Ontology.Relation;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class OntologyQueryResolver : IOntologyQueryResolver
    {
        private const string LastRelation = "LastRelation";

        public ObjectType ResolveAbstractType(IResolverContext context, object resolverResult)
        {
            var node = (Node) resolverResult;
            var typeName = OntologyObjectType.GetName(node.Type);
            return context.Schema.GetType<ObjectType>(typeName);
        }

        // ----- Root Entity resolvers ----- //

        public async Task<Guid> ResolveId(IResolverContext ctx)
        {
            return ctx.Parent<Node>().Id;
        }

        public async Task<Entity> ResolveEntity(IResolverContext ctx, EntityType type)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var id = ctx.Argument<Guid>("id");
            var node = await ontologyService.LoadNodesAsync(id, null);
            return node as Entity; // return null if node was not entity
        }

        public async Task<IEnumerable<Entity>> ResolveEntityList(IResolverContext ctx, EntityType type)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var pagination = ctx.Argument<PaginationInput>("pagination");
            var filter = ctx.Argument<FilterInput>("filter");
            var limit = pagination.PageSize;
            var offset = pagination.Offset();
            var list = await ontologyService.GetNodesByTypeAsync(type, limit, offset, filter?.Suggestion ?? filter?.SearchQuery); // Direct type
            return list.OfType<Entity>();
        }

        // ----- Relations to attributes ----- //

        // Resolve single entity-attribute relation. Return any scalar type.
        public async Task<object> ResolveAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            var ontologyService = ctx.Service<IOntologyService>();
            var node = await ontologyService.LoadNodesAsync(parent.Id, new[] {relationType});
            var computed = ResolveComputedAttribute(relationType, node);
            if (computed != null) return computed;
            var relation = node.GetRelation(relationType);
            if (relation == null) return null;
            return await ResolveAttributeValue(ctx, relation.AttributeTarget);
        }

        public static class HelperFunctions
        {
            public static string Join(params object[] values)
            {
                return string.Join(' ', values.Where(v => v != null));
            }

            public static string FullTitle(object first, object second)
            {
                if (first != null && second != null)
                    return $"{first} ({second})";
                return (first ?? second)?.ToString();
            }
        }

        private object ResolveComputedAttribute(EmbeddingRelationType relationType, Node node)
        {
            var formula = (relationType.CreateMeta() as AttributeRelationMeta)?.Formula;
            if (formula == null) return null;
            formula = formula.Replace("entity.", ""); // remove prefix from existing formulas, remove later
            formula = formula.Replace("h.", ""); // remove prefix from existing formulas, remove later
            ExpressionContext context = new ExpressionContext();
            context.Imports.AddType(typeof(HelperFunctions));
            context.Variables.ResolveVariableType += (sender, args)
                => { args.VariableType = typeof(object); };
            context.Variables.ResolveVariableValue += (sender, args)
                => { args.VariableValue = node.GetAttributeValue(args.VariableName); };
            IDynamicExpression eDynamic = context.CompileDynamic(formula);
            var result = eDynamic.Evaluate();
            return result;
        }

        private string GetAttr(Node node, string attrName) => node.GetAttributeValue(attrName) as string;

        // resolve multiple entity-[attribute] relation
        public async Task<IEnumerable<Relation>> ResolveMultipleAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            var ontologyService = ctx.Service<IOntologyService>();
            var node = await ontologyService.LoadNodesAsync(parent.Id, new[] {relationType});
            return node.GetRelations(relationType);
        }

        // Parent - embedding relation to attribute, resolve attribute value here
        public async Task<object> ResolveMultipleAttributeRelationTarget(IResolverContext ctx)
        {
            var parent = ctx.Parent<Relation>();
            return await ResolveAttributeValue(ctx, parent.AttributeTarget);
        }

        // Return any scalar type for given attribute
        public async Task<object> ResolveAttributeValue(IResolverContext ctx, Attribute attribute)
        {
            return attribute.Value;
        }

        // ----- Relations to entities ----- //

        // Resolve one or multiple relations to entity. Return either Entity or IEnumerable<Entity>
        public async Task<object> ResolveEntityRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var parent = ctx.Parent<Node>();
            var node = await ontologyService.LoadNodesAsync(parent.Id, new[] {relationType});
            var relations = node.GetRelations(relationType);
            if (!relations.GroupBy(r => r.EntityTarget).All(g => g.Count() == 1))
                return relations.Select(r => r.EntityTarget); // Non-unique targets breaks our _relation !!!
            var relationsInfo = relations.ToDictionary(r => r.EntityTarget);
            SetRelationInfo(ctx, relationsInfo); // pass info to _relation
            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                return relationsInfo.Keys;
            return relationsInfo.Keys.SingleOrDefault();
        }

        // resolver for "_relation" field on schema, passed through context
        public async Task<Relation> ResolveParentRelation(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return GetRelationInfo(ctx, parent);
        }

        // ----- Context operations ----- //

        private static void SetRelationInfo(IResolverContext ctx, Dictionary<Entity, Relation> relationsInfo)
        {
            ctx.ScopedContextData = ctx.ScopedContextData.SetItem(LastRelation, relationsInfo);
        }

        private static Relation GetRelationInfo(IResolverContext ctx, Entity entity)
        {
            var dict = (Dictionary<Entity, Relation>) ctx.ScopedContextData.GetValueOrDefault(LastRelation);
            return dict?.GetOrDefault(entity);
        }

        // ----- Created-updated ----- //

        public async Task<DateTime> ResolveCreatedAt(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return parent.CreatedAt;
        }

        public async Task<DateTime> ResolveUpdatedAt(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return parent.UpdatedAt;
        }

        // ------ All entities ----- //

        public async Task<IEnumerable<Entity>> GetAllEntities(IResolverContext ctx)
        {
            var filter = ctx.Argument<AllEntitiesFilterInput>("filter");
            var pagination = ctx.Argument<PaginationInput>("pagination");
            var ontologyService = ctx.Service<IOntologyService>();
            var ontologyTypesService = ctx.Service<IOntologyTypesService>();

            var types = ontologyTypesService.EntityTypes;
            if (filter?.Types != null)
                types = types.Where(et => filter.Types.Contains(et.Name)).ToList();

            var limit = pagination.PageSize;
            var offset = pagination.Offset();
            var nodes = await ontologyService.GetNodesAsync(types, limit, offset, filter?.Suggestion ?? filter?.SearchQuery);
            var entities = nodes.OfType<Entity>();
            return entities;
        }

        [Obsolete]
        public async Task<IEnumerable<Entity>> ResolveIncomingRelations(IResolverContext ctx)
        {
            var nodeId = ctx.Parent<Node>().Id;
            var pagination = ctx.Argument<PaginationInput>("pagination");
            var typesArg = ctx.Argument<IEnumerable<string>>("types");
            var ontologyContext = ctx.Service<OntologyContext>();
            var ontologyService = ctx.Service<IOntologyService>();
            var ontologyProvider = ctx.Service<IOntologyProvider>();
            var ontology = await ontologyProvider.GetOntologyAsync();
            var typesIds = ontology.EntityTypes.Where(et => typesArg.Contains(et.Name)).Select(et => et.Id);


            await ontologyContext.Semaphore.WaitAsync();
            List<Guid> sourceIds;
            try
            {
                var sourceQ = ontologyContext.Relations
                    .Include(e => e.SourceNode)
                    .Where(e => e.TargetNodeId == nodeId && typesIds.Contains(e.SourceNode.TypeId))
                    .Select(e => e.SourceNodeId);
                if (pagination != null)
                    sourceQ = sourceQ.Skip(pagination.Offset()).Take(pagination.PageSize);
                sourceIds = await sourceQ.ToListAsync();
            }
            finally
            {
                ontologyContext.Semaphore.Release();
            }

            var nodes = new List<Node>();
            foreach (var sourceId in sourceIds)
                nodes.Add(await ontologyService.LoadNodesAsync(sourceId, null));
            return nodes.OfType<Entity>().ToList();
        }
    }
}
