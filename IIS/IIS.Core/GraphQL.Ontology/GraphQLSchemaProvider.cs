using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.Ontology
{
    public class GraphQLSchemaProvider : IGraphQlSchemaProvider
    {
        private readonly IOntologyProvider _ontologyProvider;

        public GraphQLSchemaProvider(IOntologyProvider ontologyProvider)
        {
            _ontologyProvider = ontologyProvider;
        }

        public ISchema GetSchema()
        {
            //var ontology = await _ontologyProvider.GetTypesAsync(cancellationToken);
            var ontology = new Dictionary<string, Type>();

            //var builder = new OntologyBuilder();
            //var name = builder
            //    .WithName("Name")
            //    .IsAttribute()
            //    .HasValueOf(Core.Ontology.ScalarType.String)
            //    .Build();
            //ontology.Add(name);

            //builder = new OntologyBuilder(ontology);
            //var obj = builder
            //    .WithName("ObjectOfStudy")
            //    .HasRequired("Name")
            //    .IsAbstraction()
            //    .Build();
            //ontology.Add(obj);

//            builder = new OntologyBuilder(ontology);
//            var phone = builder
//                .WithName("PhoneNumber")
//                .IsAttribute()
//                .HasValueOf("String")
//                .Build();
//            ontology.Add(phone);
//
//            builder = new OntologyBuilder(ontology);
//            var person = builder
//                .WithName("Person")
//                //.Is("ObjectOfStudy")
//                .Is(b => b.WithName("ObjectOfStudy").HasRequired("Name").IsAbstraction())
//                .HasMultiple("PhoneNumber")
//                .IsEntity()
//                .Build();
//            ontology.Add(person);


            // todo: Build graphql mutation schema based on the ontology
            throw new NotImplementedException();
        }
    }
}
