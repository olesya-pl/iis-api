using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Roles
{
    public class AccessObjectConfiguration: IEntityTypeConfiguration<AccessObjectEntity>
    {
        public void Configure(EntityTypeBuilder<AccessObjectEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder.HasAlternateKey(p => p.Kind);

            builder.HasMany(ao => ao.RoleAccessEntities)
                .WithOne(ra => ra.AccessObject)
                .HasForeignKey(ra => ra.AccessObjectId);
        }
    }
}
