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
        }
    }
}
