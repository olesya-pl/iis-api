using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.ChangeHistory
{
    [DbContext(typeof(OntologyContext))]
    internal class ChangeHistoryConfiguration: IEntityTypeConfiguration<ChangeHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<ChangeHistoryEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder.HasIndex(p => p.TargetId);

            builder.Property(p => p.Type)
                .HasConversion<int>();

            builder.Property(p => p.RequestId)
                .IsRequired(true)
                .HasDefaultValueSql("uuid_in(overlay(overlay(md5(random()::text || ':' || clock_timestamp()::text) placing '4' from 13) placing to_hex(floor(random()*(11-8+1) + 8)::int)::text from 17)::cstring)");
        }
    }
}