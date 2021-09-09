using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    [DbContext(typeof(OntologyContext))]
    internal sealed class NodeConfiguration : IEntityTypeConfiguration<NodeEntity>
    {
        public void Configure(EntityTypeBuilder<NodeEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .HasOne(p => p.Attribute)
                .WithOne(p => p.Node)
                .HasForeignKey<AttributeEntity>(p => p.Id);
            builder
                .HasOne(p => p.Relation)
                .WithOne(e => e.Node)
                .HasForeignKey<RelationEntity>(p => p.Id);

            builder
                .HasMany(e => e.IncomingRelations)
                .WithOne(e => e.TargetNode)
                .HasForeignKey(e => e.TargetNodeId);
            builder
                .HasMany(e => e.OutgoingRelations)
                .WithOne(e => e.SourceNode)
                .HasForeignKey(e => e.SourceNodeId);
        }
    }
}