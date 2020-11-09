using Iis.Interfaces.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class AliasConfiguration : IEntityTypeConfiguration<AliasEntity>
    {
        public void Configure(EntityTypeBuilder<AliasEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Type)
                .HasDefaultValue(AliasType.Ontology);

            builder.HasIndex(x => new { x.DotName, x.Type })
                .IsUnique();
        }
    }
}
