using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Materials
{
    internal sealed class MaterialFeatureConfiguration : IEntityTypeConfiguration<MaterialFeatureEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialFeatureEntity> builder)
        {
            builder
                .HasOne(e => e.Node)
                .WithMany(e => e.MaterialFeatures)
                .HasForeignKey(e => e.NodeId);

            builder
                .Property(p => p.NodeType)
                .IsRequired()
                .HasDefaultValue(NodeEntityType.Entity);
        }
    }
}
