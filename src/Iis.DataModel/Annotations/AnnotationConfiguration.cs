using Iis.DataModel.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Themes
{
    internal class AnnotationConfiguration : IEntityTypeConfiguration<AnnotationEntity> 
    {
        public void Configure(EntityTypeBuilder<AnnotationEntity> builder) 
        {
            builder
                .HasKey(p => p.Id);

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .Property(p => p.Content)
                .IsRequired();

        } 
    }
}