using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Materials
{
    internal sealed class MaterialInfoConfiguration : IEntityTypeConfiguration<MaterialInfoEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialInfoEntity> builder)
        {
            builder
                .HasMany(e => e.MaterialFeatures) //.HasMany(e => e.MaterialFeatures)
                .WithOne(e => e.MaterialInfo)
                .HasForeignKey(e => e.MaterialInfoId);
        }
    }
}
