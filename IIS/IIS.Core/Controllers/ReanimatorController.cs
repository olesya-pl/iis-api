using System;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.GraphQL;
using Microsoft.AspNetCore.Mvc;
//using Type = IIS.Core.Ontology.EntityFramework.Context.Type;
//using RelationType = IIS.Core.Ontology.EntityFramework.Context.RelationType;
using IIS.Legacy.EntityFramework;
using Microsoft.EntityFrameworkCore;
using IIS.Core.Ontology;
using System.Collections.Generic;
using Type = IIS.Core.Ontology.Type;
using System.Linq;

namespace IIS.Core.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class ReanimatorController : Controller
    {
        //private readonly QueueReanimator _reanimator;
        //private OntologyContext _context;

        public ReanimatorController()//, OntologyContext context
        {
            //_schemaProvider = schemaProvider;
            //_reanimator = reanimator;
            //_context = context;
        }

        public async Task<IActionResult> Get()
        {
            var opts = new DbContextOptionsBuilder().UseNpgsql("Server=db;Database=contour_dev;Username=postgres;Password=;").Options;
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

            return null;
        }

        private static Type MapAttribute(OAttribute attribute, OntologyBuilder builder)
        {
            var type = builder.WithName(attribute.Code)
                .WithTitle(attribute.Title)
                .WithMeta(attribute.Meta)
                .IsAttribute()
                .HasValueOf(attribute.Type.ToString())
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
                if (attr.IsMultiple) builder.HasMultiple(attr.Attribute.Code);
                else if (attr.IsRequired) builder.HasRequired(attr.Attribute.Code);
                else builder.HasOptional(attr.Attribute.Code);
            }

            if (srcType.Parent != null)
            {
                MapTypeEntity(srcType.Parent, ontology);
                builder.Is(srcType.Parent.Code);
            }

            foreach (var restriction in srcType.ForwardRestrictions)
            {
                var code = restriction.Target.Code;
                
                if (restriction.IsMultiple) builder.HasMultiple(code);
                else if (restriction.IsRequired) builder.HasRequired(code);
                else builder.HasOptional(code);

                // todo: add stop-condition
                //if (!ontology.Any(t => t.Name == code))
                //{
                //    MapTypeEntity(restriction.Target, ontology);
                //}
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

        [HttpPost("/api/schemarestore")]
        public async Task<IActionResult> SchemaRestore()
        {
            //await _reanimator.RestoreSchema();
            return Ok();
        }

        [HttpPost("/api/restore")]
        public async Task<IActionResult> Restore(CancellationToken cancellationToken)
        {
            //await _reanimator.RestoreOntology(cancellationToken);
            return Ok();
        }
    }
}
