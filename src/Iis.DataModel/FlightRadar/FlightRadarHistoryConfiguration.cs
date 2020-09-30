using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
        }
    }
}
