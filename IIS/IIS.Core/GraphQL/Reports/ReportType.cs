using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.Ontology;
using System;
using System.Linq;

namespace IIS.Core.GraphQL.Reports
{
    public class ReportType : ObjectType<Report>
    {
        private readonly TypeRepository _typeRepository;
        private readonly IOntologyProvider _ontologyProvider;
        IOntologyType objectType;
        EntityType type;

        public ReportType(TypeRepository typeRepository, [Service] IOntologyProvider ontologyProvider)
        {
            _typeRepository   = typeRepository ?? throw new System.ArgumentNullException(nameof(typeRepository));
            _ontologyProvider = ontologyProvider;

            var ontology = _ontologyProvider.GetOntologyAsync().Result;
            type = ontology.GetTypeOrNull<EntityType>("Event");

            if (type == null)
                throw new InvalidOperationException("Cannot find required type 'Event' in database. Add type to database or disable reports in configuration file using reportsAvailable : false");

            objectType = _typeRepository.GetOntologyType(type);
        }

        protected override void Configure(IObjectTypeDescriptor<Report> descriptor)
        {
            descriptor
                .Field("events")
                .Type(new NonNullType(new ListType(new NonNullType(objectType))))
                .Resolver(async ctx =>
                {
                    var service = ctx.Service<IOntologyService>();
                    var report = ctx.Parent<Report>();
                    var nodes = await service.LoadNodesAsync(report.EventIds, null);
                    return nodes.Cast<Entity>();
                });
        }
    }
}
