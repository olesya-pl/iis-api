using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    public class TowerLocationEntity : BaseEntity
    {
        public string Mcc { get; set; }

        public string Mnc { get; set; }

        public string Lac { get; set; }

        public string CellId { get; set; }

        public decimal Lat { get; set; }

        public decimal Long { get; set; }
    }

    internal sealed class TowerLocationConfiguration : IEntityTypeConfiguration<TowerLocationEntity>
    {
        public void Configure(EntityTypeBuilder<TowerLocationEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .HasIndex(x => new { x.Mcc, x.Mnc, x.Lat, x.CellId })
                .IsUnique()
                .IncludeProperties(x => new { x.Lac, x.Long });
        }
    }
}