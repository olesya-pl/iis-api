using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Materials
{
    [DbContext(typeof(OntologyContext))]
    internal sealed class MaterialFeatureConfiguration : IEntityTypeConfiguration<MaterialFeatureEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialFeatureEntity> builder)
        {
            builder
                .HasOne(e => e.Node)
                .WithMany(e => e.MaterialFeatures)
                .HasForeignKey(e => e.NodeId);
        }
    }
}