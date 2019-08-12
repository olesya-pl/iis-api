using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;
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
        
        public async Task<IEnumerable<Type>> GetTypesAsync(CancellationToken cancellationToken = default)
        {
            var opts = new DbContextOptionsBuilder().UseNpgsql(_connectionString).Options;
            var contourContext = new ContourContext(opts);
            var attributes = await contourContext.Attributes.ToListAsync();
            var types = await contourContext.EntityTypes
                .Include(e => e.AttributeRestrictions)
                .Include(e => e.ForwardRestrictions).ThenInclude(e => e.Type)
                .ToListAsync();

            var buildContext = new OntologyBuildContext();
            foreach (var attribute in attributes) DescribeAttribute(attribute, buildContext);
            foreach (var type in types) DescribeTypeEntity(type, buildContext);

            var ontology = buildContext.BuildOntology();
            BuildUnions(ontology);
            return ontology;
        }

        private void DescribeAttribute(OAttribute attribute, OntologyBuildContext buildContext)
        {
            var builder = buildContext.CreateBuilder();

            builder.WithName(attribute.Code)
                .WithTitle(attribute.Title)
                .WithMeta(attribute.Meta)
                .IsAttribute()
                .HasValueOf(attribute.Type.ToString().ToScalarType());
        }

        private void DescribeTypeEntity(OTypeEntity srcType, OntologyBuildContext buildContext)
        {
            var builder = buildContext.CreateBuilder();

            builder.WithName(srcType.Code)
                .WithTitle(srcType.Title)
                .WithMeta(srcType.Meta);

            foreach (var attr in srcType.AttributeRestrictions)
            {
                if (attr.IsMultiple) builder.HasMultiple(attr.Attribute.Code, attr.Attribute.Code, attr.Meta);
                else if (attr.IsRequired) builder.HasRequired(attr.Attribute.Code, attr.Attribute.Code, attr.Meta);
                else builder.HasOptional(attr.Attribute.Code, attr.Attribute.Code, attr.Meta);
            }
            
            if (srcType.Parent != null) builder.Is(srcType.Parent.Code);

            foreach (var restrictionGroup in srcType.ForwardRestrictions.GroupBy(r => r.Type))
            {
                var restriction = restrictionGroup.First();
                var code = restriction.Target.Code;
                // todo: move title to relation builder
                var title = restriction.Meta["title"]?.ToString() ?? restriction.Type.Title;
                var relationName = restriction.Type.Code;
                var meta = (JObject) restriction.Type.Meta.DeepClone();
                meta.Merge(restriction.Meta); // Merge restriction meta into type meta
                if (restrictionGroup.Count() > 1) // Create common abstract type to represent union or interface
                {
                    _unionInfos.Add(new UnionInfo {Name = relationName, Union = restrictionGroup, Source = srcType});
                }
                if (restriction.IsMultiple) builder.HasMultiple(code, relationName, meta, title);
                else if (restriction.IsRequired) builder.HasRequired(code, relationName, meta, title);
                else builder.HasOptional(code, relationName, meta, title);
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
                var relation = source.DirectProperties.Single(p => p.Name == info.Name);
                var nodes = (List<Type>) relation.Nodes;
                
                var unionName = $"{source.Name}_{relation.Name}";
                var unionType = new EntityType(Guid.Empty, unionName, true);
                foreach (var child in info.Union)
                {
                    var childType = entities[child.Target.Code];
                    var isRelation = new InheritanceRelationType(Guid.Empty);
                    isRelation.AddType(unionType); // set ParentType
                    childType.AddType(isRelation); // add relation to ParentType
                }
                
                nodes.RemoveAll(n => n is EntityType); // remove old relation type
                nodes.Add(unionType); // replace with union type
            }
        }
    }
}