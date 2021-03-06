using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Themes
{
    internal class ThemeConfiguration : IEntityTypeConfiguration<ThemeEntity>
    {
        public void Configure(EntityTypeBuilder<ThemeEntity> builder)
        {
            builder
                .HasKey(p => p.Id);

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(1024);

            builder
                .Property(p => p.Query)
                .IsRequired()
                .HasMaxLength(1024);

            builder
                .HasOne(p => p.Type)
                .WithMany()
                .HasForeignKey(p => p.TypeId);

            builder
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);
            builder
                .Property(p => p.Comment)
                .IsRequired(false);
        }
    }
}