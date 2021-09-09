using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Iis.DataModel.Analytics
{
    [DbContext(typeof(OntologyContext))]
    internal sealed class AnalyticQueryEntityConfiguration : IEntityTypeConfiguration<AnalyticQueryEntity>
    {
        public void Configure(EntityTypeBuilder<AnalyticQueryEntity> builder)
        {
            builder.HasOne(q => q.Creator);
            builder.Property(q => q.DateRanges)
                .HasColumnType("jsonb")
                .HasConversion(new ValueConverter<List<AnalyticQueryEntity.DateRange>, string>(
                    dateRanges => JsonConvert.SerializeObject(dateRanges),
                    value => JsonConvert.DeserializeObject<List<AnalyticQueryEntity.DateRange>>(value)
                ));
        }
    }
}