using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using IIS.Core.GraphQL.DataLoaders;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using Iis.Domain;
using Node = Iis.Domain.Node;
using Newtonsoft.Json.Linq;
using Relation = IIS.Core.GraphQL.Entities.ObjectTypes.Relation;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class OntologyQueryResolver : IOntologyQueryResolver
    {

        public ObjectType ResolveAbstractType(IResolverContext context, object resolverResult)
        {
            var node = (Node)resolverResult;
            var typeName = OntologyObjectType.GetName(node.Type);
            return context.Schema.GetType<ObjectType>(typeName);
        }

        // ----- Root Entity resolvers ----- //

        public async Task<Guid> ResolveMultipleId(IResolverContext ctx)
        {
            var guid = ctx.Parent<string>();
            Guid.TryParse(guid, out var res);
            return res;
        }

        public async Task<Guid> ResolveId(IResolverContext ctx)
        {
            var id = ctx.Parent<JObject>()["Id"];
            if (id == null)
            {
                return Guid.Empty;
            }
            Guid.TryParse(id.ToString(), out var res);
            return res;
        }

        public async Task<JObject> ResolveEntity(IResolverContext ctx, IEntityTypeModel type)
        {
            var id = ctx.Argument<Guid>("id");
            var node = await ctx.DataLoader<QueryNodeDataLoader>().LoadAsync(Tuple.Create<Guid, IEmbeddingRelationTypeModel>(id, null), default);
            return node ?? JObject.Parse("{}"); // return null if node was not entity
        }

        public async Task<Tuple<IEnumerable<IEntityTypeModel>, ElasticFilter, IEnumerable<Guid>>> ResolveEntityList(IResolverContext ctx, IEntityTypeModel type)
        {
            var nf = ctx.CreateNodeFilter(type);

            var filter = ctx.Argument<FilterInput>("filter");

            IEnumerable<Guid> ids = new List<Guid>();

            if (filter != null && filter.MatchList?.Any() == true)
            {
                ids = filter.MatchList;
            }

            return Tuple.Create((IEnumerable<IEntityTypeModel>)new[] { type }, nf, ids);
        }

        // ----- Relations to attributes ----- //

        // Resolve single entity-attribute relation. Return any scalar type.
        public object ResolveAttributeRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType)
        {
            var pathItemsCollection = GetPathWithoutRootEntity(ctx);
            var source = GetOriginalObject(ctx, pathItemsCollection);
            var path = IsListTravarsal(pathItemsCollection)
                ? GetPathWithoutListItems(pathItemsCollection)
                : pathItemsCollection;
            JToken parent = source as JObject;
            foreach (var item in path)
            {
                if (parent == null)
                {
                    break;
                }
                parent = parent[item.ToString()];

            }
            var targetType = relationType.AttributeType.ClrType;
            return parent?.ToObject(targetType);
        }

        // resolve multiple entity-[attribute] relation
        public object ResolveMultipleAttributeRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType)
        {
            var pathItemsCollection = GetPathWithoutRootEntity(ctx);
            var source = GetOriginalObject(ctx, pathItemsCollection);
            var path = IsListTravarsal(pathItemsCollection)
                ? GetPathWithoutListItems(pathItemsCollection)
                : pathItemsCollection;
            JToken parent = source as JObject;
            foreach (var item in path)
            {
                if (parent == null)
                {
                    break;
                }
                parent = parent[item.ToString()];

            }
            var targetType = relationType.AttributeType.ClrType;
            return new[] { parent?.ToObject(targetType) ?? New.Instance(targetType) };
        }

        private static object GetOriginalObject(IResolverContext ctx, IEnumerable<object> pathItemsCollection)
        {
            return IsListTravarsal(pathItemsCollection)
                            ? ctx.Source.Skip(ctx.Source.Count() - 3).FirstOrDefault()
                            : ctx.Source.Skip(ctx.Source.Count() - 2).FirstOrDefault();
        }

        private static IEnumerable<object> GetPathWithoutListItems(IEnumerable<object> pathItemsCollection)
        {
            return pathItemsCollection.Skip(2);
        }

        private static bool IsListTravarsal(IEnumerable<object> pathItemsCollection)
        {
            return pathItemsCollection.First().ToString() == "items";
        }

        private static IEnumerable<object> GetPathWithoutRootEntity(IResolverContext ctx)
        {
            return ctx.Path.ToCollection().Skip(1);
        }

        // Parent - embedding relation to attribute, resolve attribute value here
        public object ResolveMultipleAttributeRelationTarget(IResolverContext ctx)
        {
            //var guid =
            //Guid.TryParse(guid, out var res);
            return ctx.Parent<string>();
        }

        // Return any scalar type for given attribute
        // ----- Relations to entities ----- //


        public object ResolveEntityRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType)
        {
            try
            {
                var parent = ctx.Parent<JObject>();
                return parent[relationType.Name] ?? JObject.Parse("{}");
            }
            catch (Exception e)
            {
                throw;
            }

        }

        // Resolve one or multiple relations to entity. Return either Entity or IEnumerable<Entity>
        public Task<Relation> ResolveParentRelation(IResolverContext ctx)
        {
            var pathItemsCollection = GetPathWithoutRootEntity(ctx);
            var source = GetOriginalObject(ctx, pathItemsCollection);
            return Task.FromResult(GetRelationInfo(ctx, source as JObject));
        }

        // ----- Context operations ----- //

        private static Relation GetRelationInfo(IResolverContext ctx, JObject entity)
        {
            Guid.TryParse(entity["Id"].ToString(), out var id);
            return new Relation
            {
                Id = id
            };
        }

        // ----- Created-updated ----- //

        public async Task<DateTime> ResolveCreatedAt(IResolverContext ctx)
        {
            var createdAt = ctx.Parent<JObject>()["CreatedAt"].ToString();
            DateTime.TryParse(createdAt, out var res);
            return res;
        }

        public async Task<DateTime> ResolveUpdatedAt(IResolverContext ctx)
        {
            var updatedAt = ctx.Parent<JObject>()["UpdatedAt"].ToString();
            DateTime.TryParse(updatedAt, out var res);
            return res;
        }

        // ------ All entities ----- //

        public Task<Tuple<IEnumerable<IEntityTypeModel>, ElasticFilter, IEnumerable<Guid>>> GetAllEntities(IResolverContext ctx)
        {
            var filter = ctx.Argument<AllEntitiesFilterInput>("filter");
            var ontology = ctx.Service<IOntologyModel>();

            var types = ontology.EntityTypes;
            if (filter?.Types != null)
                types = types.Where(et => filter.Types.Contains(et.Name)).ToList();

            IEnumerable<Guid> ids = new List<Guid>();

            if (filter != null && filter.MatchList?.Any() == true)
            {
                ids = filter.MatchList;
            }

            return Task.FromResult(Tuple.Create(types, ctx.CreateNodeFilter(), ids));
        }
    }

    public static class New
    {
        public static object Instance(Type t)
        {
            if (t == typeof(string))
                return string.Empty;

            if (t.HasDefaultConstructor())
                return Activator.CreateInstance(t);

            return FormatterServices.GetUninitializedObject(t);
        }
    }
    public static class TypeExtensions
    {
        public static bool HasDefaultConstructor(this Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}
