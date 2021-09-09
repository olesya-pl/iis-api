using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Reports
{
    [DbContext(typeof(OntologyContext))]
    internal sealed class ReportEventConfiguration : IEntityTypeConfiguration<ReportEventEntity>
    {
        public void Configure(EntityTypeBuilder<ReportEventEntity> builder)
        {
            builder
                .HasKey(pt => new { pt.ReportId, pt.EventId });

            builder
                .HasOne(p => p.Report)
                .WithMany(p => p.ReportEvents)
                .HasForeignKey(p => p.ReportId);

            builder
                .HasOne(p => p.Node)
                .WithMany(p => p.ReportEvents)
                .HasForeignKey(p => p.EventId);
        }
    }
}