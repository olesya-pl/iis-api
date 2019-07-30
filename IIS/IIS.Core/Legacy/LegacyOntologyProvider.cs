using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IIS.Legacy.EntityFramework
{
    public class LegacyOntologyProvider : ILegacyOntologyProvider
    {
        private readonly string _connectionString;
        static Dictionary<string, Type> Ontology;

        public LegacyOntologyProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("db-legacy");
        }
        
        public async Task<IEnumerable<Type>> GetTypesAsync(CancellationToken cancellationToken = default)
        {
            if (Ontology != null) return Ontology.Values;
            var opts = new DbContextOptionsBuilder().UseNpgsql(_connectionString).Options;
            var contourContext = new ContourContext(opts);
            var attrs = await contourContext.Attributes.ToListAsync();
            var types = await contourContext.EntityTypes
                .Include(e => e.AttributeRestrictions)
                .Include(e => e.ForwardRestrictions).ThenInclude(e => e.Type)
                .ToListAsync()
                ;
            
            var builders = new List<OntologyBuilder>();
            foreach (var attr in attrs)
            {
                var builder = new OntologyBuilder();
                MapAttribute(attr, builder);
                builders.Add(builder);
            }
            foreach (var srcType in types)
            {
                var builder = MapTypeEntity(srcType);
                builders.Add(builder);
            }
            Ontology = new Dictionary<string, Type>();
            foreach (var builder in builders)
            {
                var type = builder.Build();
                Ontology[type.Name] = type;
            }
            return Ontology.Values;
        }

        private static OntologyBuilder MapAttribute(OAttribute attribute, OntologyBuilder builder)
        {
            builder.WithName(attribute.Code)
                .WithTitle(attribute.Title)
                .WithMeta(attribute.Meta)
                .IsAttribute()
                .HasValueOf(attribute.Type.ToString().ToScalarType());
            
            return builder;
        }

        private static OntologyBuilder MapTypeEntity(OTypeEntity srcType)
        {
            var builder = new OntologyBuilder();
            builder.WithName(srcType.Code)
                .WithTitle(srcType.Title)
                .WithMeta(srcType.Meta);

            foreach (var attr in srcType.AttributeRestrictions)
            {
                if (attr.IsMultiple) builder.HasMultiple(attr.Attribute.Code, attr.Attribute.Code, attr.Meta);
                else if (attr.IsRequired) builder.HasRequired(attr.Attribute.Code, attr.Attribute.Code, attr.Meta);
                else builder.HasOptional(attr.Attribute.Code, attr.Attribute.Code, attr.Meta);
            }
            
            if (srcType.Parent != null)
            {
                builder.Is(srcType.Parent.Code);
            }

            foreach (var restriction in srcType.ForwardRestrictions)
            {
                var code = restriction.Target.Code;
                var relationName = restriction.Type.Code;
                if (restriction.IsMultiple) builder.HasMultiple(code, relationName, restriction.Meta);
                else if (restriction.IsRequired) builder.HasRequired(code, relationName, restriction.Meta);
                else builder.HasOptional(code, relationName, restriction.Meta);
            }

            if (srcType.IsAbstract)
            {
                builder.IsAbstraction();
            }
            else
            {
                builder.IsEntity();
            }

            return builder;
        }
    }
}