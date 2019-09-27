using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Core.Ontology.Seeding;
using IIS.Core.Ontology.Seeding.Odysseus;
using IIS.Legacy.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.GraphQL
{
    public class SeederMutation
    {
        public string ClearTypes([Service] OntologyTypeSaver typeSaver)
        {
            typeSaver.ClearTypes();
            return "Types cleared";
        }

        public string ClearEntities([Service] OntologyContext context)
        {
            context.Nodes.RemoveRange(context.Nodes.ToArray());
            context.Attributes.RemoveRange(context.Attributes.ToArray());
            context.Relations.RemoveRange(context.Relations.ToArray());
            context.SaveChanges();
            return "Entities cleared.";
        }

        [GraphQLDescription("Used to migrate types from old NodeJS database to new structure. Used for Contour types generation")]
        public async Task<string> MigrateLegacyTypes(
            [Service] ILegacyOntologyProvider provider,
            [Service] OntologyTypeSaver typeSaver)
        {
            var ontology = await provider.GetOntologyAsync();
            typeSaver.SaveTypes(ontology.Types);
            return "Legacy types migrated";
        }

        [GraphQLDescription("Used to migrate entities from old NodeJS database to new structure.")]
        public async Task<string> MigrateLegacyEntities([Service] ILegacyMigrator migrator)
        {
            await migrator.Migrate();
            return "Legacy entities migrated";
        }

        public async Task<string> FillOdysseusTypes(
            [Service] TypeSeeder seeder,
            [Service] OntologyTypeSaver typeSaver)
        {
            var ontology = await seeder.GetOntologyAsync();
            typeSaver.SaveTypes(ontology.Types);
            return "Odysseys types filled";
        }

        public async Task<string> SeedContourData([Service] Seeder seeder)
        {
            await seeder.Seed("contour");
            return "Contour data seeded";
        }

        public async Task<string> SeedOdysseusData([Service] Seeder seeder)
        {
            await seeder.Seed("odysseus");
            return "Odysseus data seeded";
        }

        public async Task<string> Migrate([Service] OntologyContext context)
        {
            await context.Database.MigrateAsync();
            return "Migration has been applied.";
        }
    }
}
