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
        public LegacyOntologyProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("db-legacy");
        }
        
        public async Task<IEnumerable<Type>> GetTypesAsync(CancellationToken cancellationToken = default)
        {
            var opts = new DbContextOptionsBuilder().UseNpgsql(_connectionString).Options;
            var contourContext = new ContourContext(opts);
            var attrs = await contourContext.Attributes.ToListAsync();
            var types = await contourContext.EntityTypes
                .Include(e => e.AttributeRestrictions)
                .Include(e => e.ForwardRestrictions)
                .ToListAsync()
                ;
            var ontology = new List<Type>();
            var builders = new List<OntologyBuilder>();
            foreach (var attr in attrs)
            {
                var builder = new OntologyBuilder(ontology);
                var type = MapAttribute(attr, builder);
                ontology.Add(type);
            }
            foreach (var srcType in types)
            {
                var builder = MapTypeEntity(srcType, ontology);
                builders.Add(builder);
            }

            foreach (var builder in builders)
            {
                var type = ontology.FirstOrDefault(t => t.Name == builder.Name);
                if (type == null)
                {
                    type = builder.Build();
                    ontology.Add(type);
                }
            }

            return ontology;
        }

        private static Type MapAttribute(OAttribute attribute, OntologyBuilder builder)
        {
            var type = builder.WithName(attribute.Code)
                .WithTitle(attribute.Title)
                .WithMeta(attribute.Meta)
                .IsAttribute()
                .HasValueOf(attribute.Type.ToString().ToScalarType())
                .Build();
            
            return type;
        }

        private static OntologyBuilder MapTypeEntity(OTypeEntity srcType, List<Type> ontology)
        {
            var builder = new OntologyBuilder(ontology);
            builder.WithName(srcType.Code)
                .WithTitle(srcType.Title)
                .WithMeta(srcType.Meta);

            foreach (var attr in srcType.AttributeRestrictions)
            {
                if (attr.IsMultiple) builder.HasMultiple(attr.Attribute.Code, attr.Meta);
                else if (attr.IsRequired) builder.HasRequired(attr.Attribute.Code, attr.Meta);
                else builder.HasOptional(attr.Attribute.Code, attr.Meta);
            }

            if (srcType.Parent != null)
            {
                MapTypeEntity(srcType.Parent, ontology);
                builder.Is(srcType.Parent.Code);
            }

            // Removed because of circular relation dependencies
//            foreach (var restriction in srcType.ForwardRestrictions)
//            {
//                var code = restriction.Target.Code;
//                
//                if (restriction.IsMultiple) builder.HasMultiple(code);
//                else if (restriction.IsRequired) builder.HasRequired(code);
//                else builder.HasOptional(code);
//
//                // todo: add stop-condition
//                //if (!ontology.Any(t => t.Name == code))
//                //{
//                //    MapTypeEntity(restriction.Target, ontology);
//                //}
//            }

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