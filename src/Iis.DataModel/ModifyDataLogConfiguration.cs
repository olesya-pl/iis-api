using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel
{
    public class ModifyDataLogConfiguration : IEntityTypeConfiguration<ModifyDataLogEntity>
    {
        public void Configure(EntityTypeBuilder<ModifyDataLogEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .Property(p => p.Name)
                .IsRequired(true);

            builder
                .Property(p => p.Success)
                .IsRequired(true);
        }
    }
}
