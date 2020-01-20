using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class NodeTypeConfiguration : IEntityTypeConfiguration<NodeTypeEntity>
    {
        public void Configure(EntityTypeBuilder<NodeTypeEntity> builder)
        {
            builder
                .HasOne(p => p.AttributeType)
                .WithOne(p => p.NodeType)
                .HasForeignKey<AttributeTypeEntity>(p => p.Id);
            builder
                .HasOne(p => p.RelationType)
                .WithOne(p => p.NodeType)
                .HasForeignKey<RelationTypeEntity>(p => p.Id);

            builder
                .HasMany(e => e.IncomingRelations)
                .WithOne(e => e.TargetType)
                .HasForeignKey(e => e.TargetTypeId);
            builder
                .HasMany(e => e.OutgoingRelations)
                .WithOne(e => e.SourceType)
                .HasForeignKey(e => e.SourceTypeId);

            //builder
            //    .HasMany(e => e.Nodes)
            //    .WithOne(e => e.NodeType)
            //    .HasForeignKey(e => e.NodeTypeId);
        }
    }
}
