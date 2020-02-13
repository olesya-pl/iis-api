using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class OntologyMigrationsConfiguration : IEntityTypeConfiguration<OntologyMigrationsEntity>
    {
        public void Configure(EntityTypeBuilder<OntologyMigrationsEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
