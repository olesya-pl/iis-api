using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Legacy.EntityFramework
{
    public class LegacyOntologyProvider : ILegacyOntologyProvider
    {
        private readonly string _connectionString;

        struct UnionInfo
        {
            public IGrouping<OTypeRelation, ORestriction> Union;
            public OTypeEntity Source;
            public string Name;
        }

        private List<UnionInfo> _unionInfos = new List<UnionInfo>();


        public LegacyOntologyProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("db-legacy");
        }

        public async Task<Ontology> GetOntologyAsync(CancellationToken cancellationToken = default)
        {
            if (_connectionString == null)
                throw new ArgumentException("There is no db-legacy connection string configured.");
            var opts = new DbContextOptionsBuilder().UseNpgsql(_connectionString).Options;
            var contourContext = new ContourContext(opts);
            var attributes = await contourContext.Attributes.ToListAsync(cancellationToken);
            var types = await contourContext.EntityTypes
                .Include(e => e.AttributeRestrictions)
                .Include(e => e.ForwardRestrictions).ThenInclude(e => e.Type)
                .ToListAsync(cancellationToken);

            var buildContext = new OntologyBuildContext();
            foreach (var attribute in attributes) DescribeAttribute(attribute, buildContext);
            foreach (var type in types) DescribeTypeEntity(type, buildContext);

            var ontology = buildContext.BuildOntology();
            BuildUnions(ontology);
            return new Ontology(ontology);
        }

        private void DescribeAttribute(OAttribute attribute, OntologyBuildContext buildContext)
        {
            var builder = buildContext.CreateBuilder();
            var scalarType = attribute.Type.ToString().ToScalarType();


            builder.WithName(attribute.Code)
                .WithTitle(attribute.Title)
                .IsAttribute()
                .HasValueOf(scalarType);

            if (attribute.Meta != null)
                builder.WithMeta(MetaExtensions.CreateMeta<AttributeMeta>(attribute.Meta,
                    new MetaConverter<AttributeMeta>(scalarType)));
        }

        private void DescribeTypeEntity(OTypeEntity srcType, OntologyBuildContext buildContext)
        {
            var builder = buildContext.CreateBuilder();

            builder.WithName(srcType.Code)
                .WithTitle(srcType.Title);

            if (srcType.Meta != null)
                builder.WithMeta(MetaExtensions.CreateMeta<EntityMeta>(srcType.Meta,
                    new MetaConverter<EntityMeta>(null)));

            foreach (var attr in srcType.AttributeRestrictions)
            {
                var scalarType = attr.Attribute.Type.ToString().ToScalarType();
                var converter = new MetaConverter<AttributeRelationMeta>(scalarType);
                var meta = attr.Meta == null
                    ? null
                    : MetaExtensions.CreateMeta<AttributeRelationMeta>(attr.Meta, converter);

                void AttributeRelationDescriptor(IRelationBuilder r) => r
                    .Target(attr.Attribute.Code)
                    .WithName(attr.Attribute.Code)
                    .WithTitle(attr.Attribute.Title)
                    .WithMeta(meta)
                ;

                if (attr.IsMultiple) builder.HasMultiple(AttributeRelationDescriptor);
                else if (attr.IsRequired) builder.HasRequired(AttributeRelationDescriptor);
                else builder.HasOptional(AttributeRelationDescriptor);
            }

            if (srcType.Parent != null) builder.Is(srcType.Parent.Code);

            foreach (var restrictionGroup in srcType.ForwardRestrictions.GroupBy(r => r.Type))
            {
                var restriction = restrictionGroup.First();
                var code = restriction.Type.Code;
                var title = restriction.Meta["title"]?.ToString() ?? restriction.Type.Title;
                var joMeta = (JObject) restriction.Type.Meta.DeepClone();
                joMeta.Merge(restriction.Meta); // Merge restriction meta into type meta
                if (restrictionGroup.Count() > 1) // Create common abstract type to represent union or interface
                {
                    _unionInfos.Add(new UnionInfo {Name = code, Union = restrictionGroup, Source = srcType});
                }

                var converter = new MetaConverter<EntityRelationMeta>(null);
                var meta = MetaExtensions.CreateMeta<EntityRelationMeta>(joMeta, converter);

                void EntityRelationDescriptor(IRelationBuilder r) => r
                    .Target(restriction.Target.Code)
                    .WithName(code)
                    .WithTitle(title)
                    .WithMeta(meta)
                ;

                if (restriction.IsMultiple) builder.HasMultiple(EntityRelationDescriptor);
                else if (restriction.IsRequired) builder.HasRequired(EntityRelationDescriptor);
                else builder.HasOptional(EntityRelationDescriptor);
            }

            if (srcType.IsAbstract) builder.IsAbstraction();
            else builder.IsEntity();
        }

        private void BuildUnions(IEnumerable<Type> ontology)
        {
            var entities = ontology.OfType<EntityType>().ToDictionary(t => t.Name);
            foreach (var info in _unionInfos)
            {
                var source = entities[info.Source.Code];
                var relationName = info.Name.ToLowerCamelcase();
                var relation = source.DirectProperties.Single(p => p.Name == relationName);
                var nodes = (List<Type>) relation.RelatedTypes;

                var unionName = $"{source.Name}_{relation.Name}";
                var unionType = new EntityType(Guid.NewGuid(), unionName, true);
                unionType.Title = unionName;
                foreach (var child in info.Union)
                {
                    var childType = entities[child.Target.Code];
                    var isRelation = new InheritanceRelationType(Guid.NewGuid());
                    isRelation.AddType(unionType); // set ParentType
                    childType.AddType(isRelation); // add relation to ParentType
                }

                nodes.RemoveAll(n => n is EntityType); // remove old relation type
                nodes.Add(unionType); // replace with union type
            }
        }

        public async Task InvalidateAsync()
        {
            await Task.CompletedTask;
        }
    }
}
