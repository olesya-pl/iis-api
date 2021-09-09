using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    [DbContext(typeof(OntologyContext))]
    internal class ModifyDataLogConfiguration : IEntityTypeConfiguration<ModifyDataLogEntity>
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