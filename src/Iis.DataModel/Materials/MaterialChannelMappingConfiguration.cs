using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Materials
{
    public class MaterialChannelMappingConfiguration: IEntityTypeConfiguration<MaterialChannelMappingEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialChannelMappingEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
