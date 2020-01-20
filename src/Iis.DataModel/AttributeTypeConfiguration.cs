using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    internal sealed class AttributeTypeConfiguration : IEntityTypeConfiguration<AttributeTypeEntity>
    {
        public void Configure(EntityTypeBuilder<AttributeTypeEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
