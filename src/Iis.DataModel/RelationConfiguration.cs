using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class RelationConfiguration : IEntityTypeConfiguration<RelationEntity>
    {
        public void Configure(EntityTypeBuilder<RelationEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
