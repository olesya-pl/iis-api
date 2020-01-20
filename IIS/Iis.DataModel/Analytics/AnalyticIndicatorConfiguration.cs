using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Analytics
{
    internal sealed class AnalyticIndicatorConfiguration : IEntityTypeConfiguration<AnalyticIndicatorEntity>
    {
        public void Configure(EntityTypeBuilder<AnalyticIndicatorEntity> builder)
        {
            builder.HasOne(i => i.Parent);
            builder.Property(i => i.ParentId).IsRequired(false);
        }
    }
}
