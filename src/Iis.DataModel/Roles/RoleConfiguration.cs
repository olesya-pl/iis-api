using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Roles
{
    [DbContext(typeof(OntologyContext))]
    internal class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .HasIndex(_ => _.Name)
                .IsUnique(true);

            builder.HasMany(r => r.RoleAccessEntities)
                .WithOne(ra => ra.Role)
                .HasForeignKey(ra => ra.RoleId);

            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}