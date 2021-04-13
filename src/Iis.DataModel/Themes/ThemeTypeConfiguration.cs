using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Themes
{
    internal class ThemeTypeConfiguration : IEntityTypeConfiguration<ThemeTypeEntity>
    {
        public void Configure(EntityTypeBuilder<ThemeTypeEntity> builder)
        {
            builder
                .HasKey(p => p.Id);

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();
            
            builder
                .Property(p => p.ShortTitle)
                .IsRequired()
                .HasMaxLength(16);

            builder
                .Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder
                .Property(p => p.EntityTypeName)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasData(
                new ThemeTypeEntity{Id = new Guid("2b8fd109-cf4a-4f76-8136-de761da53d20"), ShortTitle = "М", Title = "Матеріал", EntityTypeName = "EntityMaterial"},
                new ThemeTypeEntity{Id = new Guid("043ae699-e070-4336-8513-e90c87555c58"), ShortTitle = "О", Title = "Об'єкт", EntityTypeName = "EntityObject"},
                new ThemeTypeEntity{Id = new Guid("42f61965-8baa-4026-ab33-0378be8a6c3e"), ShortTitle = "П", Title = "Подія", EntityTypeName = "EntityEvent"},
                new ThemeTypeEntity{Id = new Guid("2b4b2a5a-bd2a-4159-839e-02e169fc018c"), ShortTitle = "З", Title = "Звіт", EntityTypeName = "EntityReport"}
            );
        }
    }
}