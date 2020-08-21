using AutoMapper;
using Iis.DataModel;
using Iis.DbLayer.OntologyData;
using Iis.Interfaces.Ontology.Data;
using Iis.OntologyData;
using Iis.OntologyData.DataTypes;
using Iis.OntologyData.Migration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Iis.UnitTests.Iis.OntologyData
{
    public class OntologyDataTests
    {
        [Fact]
        public async Task LoadStorageTest()
        {
            var connectionString = "Server = localhost; Database = contour_dev_net; Username = postgres; Password = 123";
            await Test(connectionString);
        }
        
        private async Task Test(string connectionString)
        {
            var context = OntologyContext.GetContext(connectionString);
            var rawData = new NodesRawData(context.Nodes, context.Relations, context.Attributes);
            var schema = Utils.GetOntologySchemaFromDb(connectionString);
            var ontologyData = new OntologyNodesData(rawData, schema);
            var serviceProvider = Utils.GetServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();
            var ontologyPatchSaver = new OntologyPatchSaver(context, mapper);

            var militaryBaseMigration = new MigrationEntity
            {
                SourceEntityName = "MilitaryBase",
                TargetEntityName = "MilitaryOrganization",
                Items = new List<MigrationItem>
                {
                    new MigrationItem("baseCode", "commonInfo.OpenName"),
                    new MigrationItem("title", "commonInfo.RealNameFull"),
                    new MigrationItem("shortName", "commonInfo.RealNameShort"),
                    new MigrationItem("relatesToCountry", "country"),
                }
            };
            ontologyData.Migrate(militaryBaseMigration);
            await ontologyPatchSaver.SavePatch(ontologyData.Patch);
            ontologyData.ClearPatch();

            var subdivisionMigration = new MigrationEntity
            {
                SourceEntityName = "Subdivision",
                TargetEntityName = "MilitaryOrganization",
                Items = new List<MigrationItem>
                {
                    new MigrationItem("temporaryDislocation", "otherDislocationPlaces.coords"),
                    new MigrationItem("headquartersDislocation", "mainDislocation.coords"),
                    new MigrationItem("dislocation", "mainDislocation.coords",
                        new MigrationItemOptions { IgnoreIfFieldsAreNotEmpty = "headquartersDislocation"}),
                    new MigrationItem("subtype", null, new MigrationItemOptions { Ignore = true }),
                    new MigrationItem("militaryBase", "militaryBaseShortName",
                        new MigrationItemOptions { TakeValueFrom = "shortName" }),
                    new MigrationItem("corpsType", "classifiers.corps"),
                    new MigrationItem("name", "commonInfo.RealNameFull"),
                    new MigrationItem("description", "additionalInfo"),
                    new MigrationItem("shortName", "commonInfo.RealNameShort"),
                    new MigrationItem("placeOfHeadquartersDislocation", "mainDislocation.postalAddress"),
                    new MigrationItem("placeOfDislocation", "mainDislocation.postalAddress",
                        new MigrationItemOptions { IgnoreIfFieldsAreNotEmpty = "placeOfHeadquartersDislocation"}),
                    new MigrationItem("relatesToCountry", "country"),
                    new MigrationItem("sidcTitle", "classifiers.sidc"),
                    new MigrationItem("superior", "parent",
                        new MigrationItemOptions { IsHierarchical = true }),
                    new MigrationItem("bePartOf", "bePartOf",
                        new MigrationItemOptions { IsHierarchical = true }),
                    new MigrationItem("amountOfPersonnel", "staffInfo.currentAmount.total"),
                }
            };
            ontologyData.Migrate(subdivisionMigration);
            await ontologyPatchSaver.SavePatch(ontologyData.Patch);
            ontologyData.ClearPatch();
        }
    }
}
