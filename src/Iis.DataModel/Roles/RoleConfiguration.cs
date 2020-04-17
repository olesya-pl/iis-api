using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Roles
{
    public class RoleConfiguration: IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .Property(p => p.AdGroup)
                .IsRequired(false)
                .HasMaxLength(1024);

            builder.HasMany(r => r.RoleAccessEntities)
                .WithOne(ra => ra.Role)
                .HasForeignKey(ra => ra.RoleId);

            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
