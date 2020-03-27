using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Roles
{
    public class RoleAccessConfiguration: IEntityTypeConfiguration<RoleAccessEntity>
    {
        public void Configure(EntityTypeBuilder<RoleAccessEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
