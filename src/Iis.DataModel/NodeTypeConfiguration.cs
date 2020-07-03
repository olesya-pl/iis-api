using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class NodeTypeConfiguration : IEntityTypeConfiguration<NodeTypeEntity>
    {
        public void Configure(EntityTypeBuilder<NodeTypeEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .HasOne(p => p.IAttributeTypeModel)
                .WithOne(p => p.INodeTypeModel)
                .HasForeignKey<AttributeTypeEntity>(p => p.Id);
            builder
                .HasOne(p => p.RelationType)
                .WithOne(p => p.INodeTypeModel)
                .HasForeignKey<RelationTypeEntity>(p => p.Id);

            builder
                .HasMany(e => e.IncomingRelations)
                .WithOne(e => e.TargetType)
                .HasForeignKey(e => e.TargetTypeId);
            builder
                .HasMany(e => e.OutgoingRelations)
                .WithOne(e => e.SourceType)
                .HasForeignKey(e => e.SourceTypeId);
        }
    }
}
