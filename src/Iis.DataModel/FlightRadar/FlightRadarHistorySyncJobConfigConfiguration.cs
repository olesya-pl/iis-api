using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.FlightRadar
{
    internal class FlightRadarHistorySyncJobConfigConfiguration : IEntityTypeConfiguration<FlightRadarHistorySyncJobConfig>
    {
        public void Configure(EntityTypeBuilder<FlightRadarHistorySyncJobConfig> builder)
        {
            builder.Property(p => p.LatestProcessedId)
                .HasDefaultValue(0);

            builder.ToTable("FlightRadarHistorySyncJobConfig");

            builder.HasKey(p => p.LatestProcessedId);
        }
    }
}
