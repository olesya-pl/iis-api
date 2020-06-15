using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Materials
{
    internal sealed class MaterialConfiguration: IEntityTypeConfiguration<MaterialEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialEntity> builder)
        {
            builder
                .HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId);

            builder
                .HasOne(e => e.File)
                .WithMany(e => e.Materials)
                .HasForeignKey(e => e.FileId);

            builder
                .HasMany(e => e.MaterialInfos)
                .WithOne(e => e.Material)
                .HasForeignKey(e => e.MaterialId);

            builder
                .HasOne(e => e.Importance)
                .WithMany()
                .HasForeignKey(e => e.ImportanceSignId);

            builder
                .HasOne(e => e.Reliability)
                .WithMany()
                .HasForeignKey(e => e.ReliabilitySignId);

            builder
                .HasOne(e => e.Relevance)
                .WithMany()
                .HasForeignKey(e => e.RelevanceSignId);

            builder
                .HasOne(e => e.Completeness)
                .WithMany()
                .HasForeignKey(e => e.CompletenessSignId);

            builder
                .HasOne(e => e.SourceReliability)
                .WithMany()
                .HasForeignKey(e => e.SourceReliabilitySignId);

            builder
                .HasOne(e => e.SessionPriority)
                .WithMany()
                .HasForeignKey(e => e.SessionPriorityId);

            builder
                .HasOne(e => e.ProcessedStatus)
                .WithMany()
                .HasForeignKey(e => e.ProcessedStatusSignId);

            builder.Property(e => e.ParentId).IsRequired(false);
            builder.Property(e => e.FileId).IsRequired(false);

            builder
                .HasOne(e => e.Assignee)
                .WithMany(e => e.Materials)
                .HasForeignKey(e => e.AssigneeId);

            builder.Property(e => e.MlHandlersCount)
                .IsRequired(true)
                .HasDefaultValue(0);
        }
    }
}
