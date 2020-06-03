using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.ChangeHistory
{
    internal class ChangeHistoryConfiguration: IEntityTypeConfiguration<ChangeHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<ChangeHistoryEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder.HasIndex(p => p.TargetId);
        }
    }
}
