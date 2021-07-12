using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Iis.Interfaces.Enums;
namespace Iis.DataModel.FlightRadar
{
    public class FlightRadarHistoryConfiguration : IEntityTypeConfiguration<LocationHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<LocationHistoryEntity> builder)
        {
            builder
                .HasOne(p => p.Node)
                .WithMany()
                .HasForeignKey(p => p.NodeId);

            builder
                .HasOne(p => p.Material)
                .WithMany()
                .HasForeignKey(p => p.MaterialId);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasDefaultValue(LocationType.Node);
        }
    }
}
