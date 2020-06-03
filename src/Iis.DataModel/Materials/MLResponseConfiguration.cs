using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Iis.DataModel.Materials
{
    internal sealed class MLResponseConfiguration : IEntityTypeConfiguration<MLResponseEntity>
    {
        public void Configure(EntityTypeBuilder<MLResponseEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
