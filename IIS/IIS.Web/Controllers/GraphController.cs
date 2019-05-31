﻿using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using IIS.Introspection;
using IIS.Ontology.GraphQL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace IIS.Web.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class GraphController : Controller
    {
        public class GraphQuery
        {
            public string OperationName { get; set; }
            public string NamedQuery { get; set; }
            public string Query { get; set; }
            public JObject Variables { get; set; }
        }

        private readonly IGraphQLSchemaProvider _schemaProvider;
        private readonly DataLoaderDocumentListener _documentListener;
        private readonly QueueReanimator _reanimator;

        public GraphController(IGraphQLSchemaProvider schemaProvider, DataLoaderDocumentListener documentListener, 
            QueueReanimator reanimator)
        {
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _documentListener = documentListener ?? throw new ArgumentNullException(nameof(documentListener));
            _reanimator = reanimator;
        }

        public async Task<IActionResult> Post([FromBody] GraphQuery query)
        {
            var inputs = query.Variables.ToInputs();
            var schema = await _schemaProvider.GetSchemaAsync();
            var result = await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = query.Query;
                _.OperationName = query.OperationName;
                _.Inputs = inputs;
                _.Listeners.Add(_documentListener);
            });

            return Ok(result);
        }

        [HttpPost("/api/index")]
        public async Task<IActionResult> CreateIndex()
        {
            //var schema = await _schema.GetRootAsync();
            //await _replicationService.CreateIndexAsync(schema);

            return Ok();
        }

        [HttpPost("/api/restore")]
        public async Task<IActionResult> Restore()
        {
            await _reanimator.RestoreOntology();
            return Ok();
        }
    }

}
