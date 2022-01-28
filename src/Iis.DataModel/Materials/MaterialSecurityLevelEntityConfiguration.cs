using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Materials
{
    public class MaterialSecurityLevelEntityConfiguration : IEntityTypeConfiguration<MaterialSecurityLevelEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialSecurityLevelEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .HasOne(e => e.Material)
                .WithMany(e => e.SecurityLevels)
                .HasForeignKey(e => e.MaterialId);
        }
    }
}
