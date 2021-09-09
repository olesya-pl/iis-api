using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Elastic
{
    [DbContext(typeof(OntologyContext))]
    internal class ElasticFieldConfiguration: IEntityTypeConfiguration<ElasticFieldEntity>
    {
        public void Configure(EntityTypeBuilder<ElasticFieldEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
        }
    }
}