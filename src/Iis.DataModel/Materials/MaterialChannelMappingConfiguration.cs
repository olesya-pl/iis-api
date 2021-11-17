using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Materials
{
    public class MaterialChannelMappingConfiguration : IEntityTypeConfiguration<MaterialChannelMappingEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialChannelMappingEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
