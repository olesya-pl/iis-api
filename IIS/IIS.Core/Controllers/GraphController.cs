using System;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.GraphQL;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.AspNetCore.Mvc;
using Type = IIS.Core.Ontology.EntityFramework.Context.Type;
using RelationType = IIS.Core.Ontology.EntityFramework.Context.RelationType;

namespace IIS.Core.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class GraphController : Controller
    {
        private readonly IGraphQLSchemaProvider _schemaProvider;
        //private readonly QueueReanimator _reanimator;
        private OntologyContext _context;

        public GraphController(IGraphQLSchemaProvider schemaProvider, OntologyContext context)//, QueueReanimator reanimator
        {
            _schemaProvider = schemaProvider;
            //_reanimator = reanimator;
            _context = context;
        }

        public async Task<IActionResult> Get()
        {
            //var aType = new Type
            //{
            //    Id = Guid.NewGuid(),
            //    Name = "AbstractFoo",
            //    IsAbstract = true,
            //    Kind = Kind.Entity
            //};
            //var cType = new Type
            //{
            //    Id = Guid.NewGuid(),
            //    Name = "ConcreteFoo",
            //    Kind = Kind.Entity
            //};
            //var iRelationBase = new Type
            //{
            //    Id = Guid.NewGuid(),
            //    Kind = Kind.Relation
            //};
            //var iRelation = new RelationType
            //{
            //    Id = iRelationBase.Id,
            //    Kind = RelationKind.Inheritance,
            //    Type = iRelationBase,
            //    SourceType = cType,
            //    TargetType = aType
            //};
            
            //_context.RelationTypes.Add(iRelation);
            //await _context.SaveChangesAsync();

            var schema = await _schemaProvider.GetSchemaAsync();

            return null;
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
