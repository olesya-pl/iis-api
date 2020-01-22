using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class RelationTypeConfiguration : IEntityTypeConfiguration<RelationTypeEntity>
    {
        public void Configure(EntityTypeBuilder<RelationTypeEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
