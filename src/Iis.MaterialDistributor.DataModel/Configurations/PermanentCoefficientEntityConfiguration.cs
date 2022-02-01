using Iis.MaterialDistributor.DataModel.Contexts;
using Iis.MaterialDistributor.DataModel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.MaterialDistributor.DataModel.Configurations
{
    [DbContext(typeof(MaterialDistributorContext))]
    internal class PermanentCoefficientEntityConfiguration : IEntityTypeConfiguration<PermanentCoefficientEntity>
    {
        public void Configure(EntityTypeBuilder<PermanentCoefficientEntity> builder)
        {
            builder.Property(_ => _.Name)
                .HasMaxLength(256)
                .IsRequired();

            builder.HasIndex(_ => _.Name)
                .IsUnique();
        }
    }
}