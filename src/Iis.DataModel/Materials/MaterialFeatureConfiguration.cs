using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Materials
{
    internal sealed class MaterialFeatureConfiguration : IEntityTypeConfiguration<MaterialFeatureEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialFeatureEntity> builder)
        {
            //builder
            //    .HasOne(e => e.Node)
            //    .WithMany()
            //    .HasForeignKey(e => e.NodeId);
        }
    }
}
