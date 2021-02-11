using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class TowerLocationConfiguration : IEntityTypeConfiguration<TowerLocationEntity>
    {
        public void Configure(EntityTypeBuilder<TowerLocationEntity> builder)
        {
            builder
                .HasNoKey();
        }
    }
}