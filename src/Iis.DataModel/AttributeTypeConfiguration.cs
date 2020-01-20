using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class AttributeTypeConfiguration : IEntityTypeConfiguration<AttributeTypeEntity>
    {
        public void Configure(EntityTypeBuilder<AttributeTypeEntity> builder)
        {
            //builder
            //    .HasOne(p => p.NodeType)
            //    .WithOne(p => p.AttributeType)
            //    .HasForeignKey<AttributeTypeEntity>(p => p.Id);
        }
    }
}
