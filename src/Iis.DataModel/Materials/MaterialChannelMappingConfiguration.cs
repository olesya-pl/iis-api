using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Materials
{
    public class MaterialChannelMappingConfiguration: IEntityTypeConfiguration<MaterialChannelMapping>
    {
        public void Configure(EntityTypeBuilder<MaterialChannelMapping> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
