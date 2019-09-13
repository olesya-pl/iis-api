using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
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

        public async Task<string> MigrateLegacyTypes(
            [Service] ILegacyOntologyProvider provider,
            [Service] OntologyTypeSaver typeSaver)
        {
            var ontology = await provider.GetOntologyAsync();
            typeSaver.SaveTypes(ontology.Types);
            return "Legacy types migrated";
        }


        public async Task<string> FillOdysseysTypes(
            [Service] Core.Ontology.Odysseys.PersonSeeder seeder,
            [Service] OntologyTypeSaver typeSaver)
        {
            var ontology = await seeder.GetOntologyAsync();
            typeSaver.SaveTypes(ontology.Types);
            return "Odysseys types filled";
        }

        public async Task<string> SeedData([Service] Seeder seeder)
        {
            await seeder.Seed(default);
            return "Data seeded";
        }

        public async Task<string> Migrate([Service] OntologyContext context)
        {
            await context.Database.MigrateAsync();
            return "Migration has been applied.";
        }
    }
}
