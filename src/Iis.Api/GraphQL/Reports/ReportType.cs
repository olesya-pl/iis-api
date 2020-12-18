using HotChocolate.Types;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.GraphQL.Materials;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology;
using IIS.Core.Materials;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.Domain;
using IIS.Domain;
using AutoMapper;
using Iis.OntologySchema.DataTypes;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.Reports
{
    public class ReportType : ObjectType<Report>
    {
        private readonly TypeRepository _typeRepository;
        private readonly IOntologyModel _ontology;
        IOntologyType objectType;
        INodeTypeLinked type;

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
                .Resolver(ctx =>
                {
                    var service = ctx.Service<IOntologyService>();
                    var report = ctx.Parent<Report>();
                    var nodes = service.LoadNodes(report.EventIds, null);
                    return Task.FromResult(nodes.Cast<Entity>());
                });
            descriptor
                .Field("relatedMaterials")
                .Type(typeof(NonNullType<ListType<NonNullType<ObjectType<RelatedMaterialsItem>>>>))
                .Resolver(async ctx => {

                    var materialProvider = ctx.Service<IMaterialProvider>();

                    var mapper = ctx.Service<IMapper>();

                    var report = ctx.Parent<Report>();

                    if(!report.EventIds.Any()) return new List<RelatedMaterialsItem>();

                    var tasks = report.EventIds.Select(async eventId => {

                            var materialsResult = await materialProvider.GetMaterialsByNodeIdQuery(eventId);

                            var materials = materialsResult.Materials.Select(m => mapper.Map<Material>(m)).ToList();

                            return (Event:eventId, Materials: new GraphQLCollection<Material>(materials, materialsResult.Count));
                        });

                    var relatedMaterials = await Task.WhenAll(tasks);

                    return relatedMaterials
                    .Select(m => {
                        return new RelatedMaterialsItem
                        {
                            EventId = m.Event,
                            Materials = m.Materials
                        };
                    });
                });
        }
    }
}
