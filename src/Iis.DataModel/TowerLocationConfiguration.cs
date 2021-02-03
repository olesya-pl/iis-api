using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class TowerLocationConfiguration : IEntityTypeConfiguration<TowerLocationEntity>
    {
        public void Configure(EntityTypeBuilder<TowerLocationEntity> builder)
        {
            builder
                .HasKey(x => x.Id);
            
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .HasIndex(x => new { x.Mcc, x.Mnc, x.Lac, x.CellId })
                .IsUnique(false)
                .IncludeProperties(x => new { x.Lat, x.Long });
        }
    }
}