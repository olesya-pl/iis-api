using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel
{
    [DbContext(typeof(OntologyContext))]
    internal sealed class TowerLocationConfiguration : IEntityTypeConfiguration<TowerLocationEntity>
    {
        public void Configure(EntityTypeBuilder<TowerLocationEntity> builder)
        {
            builder
                .HasNoKey();
        }
    }
}