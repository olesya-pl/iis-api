using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Elastic
{
    public class ElasticFieldConfiguration: IEntityTypeConfiguration<ElasticFieldEntity>
    {
        public void Configure(EntityTypeBuilder<ElasticFieldEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}
