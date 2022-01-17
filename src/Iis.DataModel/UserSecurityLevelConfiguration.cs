using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel
{
    public class UserSecurityLevelConfiguration : IEntityTypeConfiguration<UserSecurityLevelEntity>
    {
        public void Configure(EntityTypeBuilder<UserSecurityLevelEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .HasOne(e => e.User)
                .WithMany(e => e.SecurityLevels)
                .HasForeignKey(e => e.UserId);
        }
    }
}
