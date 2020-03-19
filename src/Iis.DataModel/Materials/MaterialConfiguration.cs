using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Materials
{
    internal sealed class MaterialConfiguration: IEntityTypeConfiguration<MaterialEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialEntity> builder)
        {
            //builder
            //    .HasOne(e => e.Parent)
            //    .WithMany(e => e.Children)
            //    .HasForeignKey(e => e.ParentId);

            //builder
            //    .HasOne(e => e.File)
            //    .WithOne()
            //    .HasForeignKey<MaterialEntity>(e => e.FileId);

            //builder
            //    .HasMany(e => e.MaterialInfos)
            //    .WithOne(e => e.Material)
            //    .HasForeignKey(e => e.MaterialId);

            //builder.Property(e => e.ParentId).IsRequired(false);
            //builder.Property(e => e.FileId).IsRequired(false);
        }
    }
}
