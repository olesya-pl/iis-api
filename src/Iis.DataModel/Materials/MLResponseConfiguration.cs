using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Materials
{
    [DbContext(typeof(OntologyContext))]
    internal sealed class MLResponseConfiguration : IEntityTypeConfiguration<MLResponseEntity>
    {
        public void Configure(EntityTypeBuilder<MLResponseEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder
                .HasOne(p => p.Material)
                .WithMany(p => p.MLResponses)
                .HasForeignKey(p => p.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasIndex(p => p.MaterialId);
        }
    }
}