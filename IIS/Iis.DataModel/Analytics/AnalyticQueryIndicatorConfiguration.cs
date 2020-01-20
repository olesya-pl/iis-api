using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Analytics
{
    internal sealed class AnalyticQueryIndicatorConfiguration : IEntityTypeConfiguration<AnalyticQueryIndicatorEntity>
    {
        public void Configure(EntityTypeBuilder<AnalyticQueryIndicatorEntity> builder)
        {
            builder
                .HasOne(i => i.Query)
                .WithMany(i => i.Indicators);

            builder
                .HasOne(i => i.Indicator)
                .WithMany(i => i.QueryIndicators);
        }
    }
}
