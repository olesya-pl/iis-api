using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Analytics
{
    [DbContext(typeof(OntologyContext))]
    internal sealed class AnalyticIndicatorConfiguration : IEntityTypeConfiguration<AnalyticIndicatorEntity>
    {
        public void Configure(EntityTypeBuilder<AnalyticIndicatorEntity> builder)
        {
            builder.HasOne(i => i.Parent);
            builder.Property(i => i.ParentId).IsRequired(false);
        }
    }
}