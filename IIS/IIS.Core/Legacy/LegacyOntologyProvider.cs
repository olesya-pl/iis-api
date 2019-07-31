using System.Collections.Generic;
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
            var attributes = await contourContext.Attributes.ToListAsync();
            var types = await contourContext.EntityTypes
                .Include(e => e.AttributeRestrictions)
                .Include(e => e.ForwardRestrictions).ThenInclude(e => e.Type)
                .ToListAsync();

            var buildContext = new OntologyBuildContext();
            foreach (var attribute in attributes) DescribeAttribute(attribute, buildContext);
            foreach (var type in types) DescribeTypeEntity(type, buildContext);

            var ontology = buildContext.BuildOntology();

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

            foreach (var restriction in srcType.ForwardRestrictions)
            {
                var code = restriction.Target.Code;
                var title = restriction.Type.Title; // todo: implement title for relations
                var relationName = restriction.Type.Code;
                if (restriction.IsMultiple) builder.HasMultiple(code, relationName, restriction.Meta);
                else if (restriction.IsRequired) builder.HasRequired(code, relationName, restriction.Meta);
                else builder.HasOptional(code, relationName, restriction.Meta);
            }

            if (srcType.IsAbstract) builder.IsAbstraction();
            else builder.IsEntity();
        }
    }
}