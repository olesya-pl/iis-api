using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    [DbContext(typeof(OntologyContext))]
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