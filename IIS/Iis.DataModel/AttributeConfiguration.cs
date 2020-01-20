using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class AttributeConfiguration : IEntityTypeConfiguration<AttributeEntity>
    {
        public void Configure(EntityTypeBuilder<AttributeEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
            //builder.HasOne(p => p.Node)
            //    .WithOne(p => p.Attribute)
            //    .HasForeignKey<AttributeEntity>(p => p.Id)
            //    ;
        }
    }
}
