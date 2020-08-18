using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Roles
{
    public class RoleActiveDirectoryGroupConfiguration : IEntityTypeConfiguration<RoleActiveDirectoryGroupEntity>
    {
        public void Configure(EntityTypeBuilder<RoleActiveDirectoryGroupEntity> builder)
        {
            builder
                .HasOne(x => x.Role)
                .WithMany(x => x.RoleGroups)
                .HasForeignKey(x => x.RoleId);

            builder
                .Property(x => x.GroupId)
                .IsRequired();
        }
    }
}