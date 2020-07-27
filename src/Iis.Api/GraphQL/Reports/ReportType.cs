using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.Ontology;
using System;
using System.Linq;
using Iis.Domain;
using IIS.Domain;

namespace IIS.Core.GraphQL.Reports
{
    public class ReportType : ObjectType<Report>
    {
        private readonly TypeRepository _typeRepository;
        private readonly IOntologyModel _ontology;
        IOntologyType objectType;
        IEntityTypeModel type;

        public ReportType(TypeRepository typeRepository, IOntologyModel ontology)
        {
            _typeRepository   = typeRepository ?? throw new System.ArgumentNullException(nameof(typeRepository));
            
            _ontology = ontology;

            type = _ontology.GetEntityType("Event");

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
