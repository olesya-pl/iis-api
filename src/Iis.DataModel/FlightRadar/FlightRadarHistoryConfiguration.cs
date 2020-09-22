using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.FlightRadar
{
    public class FlightRadarHistoryConfiguration : IEntityTypeConfiguration<FlightRadarHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<FlightRadarHistoryEntity> builder)
        {
            builder
                .HasOne(p => p.Node)
                .WithMany()
                .HasForeignKey(p => p.NodeId);
        }
    }
}
