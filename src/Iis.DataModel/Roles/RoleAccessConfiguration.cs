using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Roles
{
    public class RoleAccessConfiguration : IEntityTypeConfiguration<RoleAccessEntity>
    {
        public void Configure(EntityTypeBuilder<RoleAccessEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
