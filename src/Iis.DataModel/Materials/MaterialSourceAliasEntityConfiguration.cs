using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Iis.DataModel.Materials
{
    [DbContext(typeof(OntologyContext))]
    public class MaterialSourceAliasEntityConfiguration : IEntityTypeConfiguration<MaterialSourceAliasEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialSourceAliasEntity> builder)
        {
            builder.Property(_ => _.Source)
                .IsRequired(true)
                .HasMaxLength(50);

            builder.Property(_ => _.Alias)
                .IsRequired(true)
                .HasMaxLength(256);

            builder.HasIndex(_ => _.Source)
                .IsUnique();

            builder.HasData(
                new MaterialSourceAliasEntity { Id = new Guid("10170a812916420b8bd75688cb43b82f"), Name = "Importance", Title = "Важливість" },
                new MaterialSourceAliasEntity { Id = new Guid("202f605f4fb249a7beb8e40cd41f2b83"), Name = "Reliability", Title = "Достовірність" },
                new MaterialSourceAliasEntity { Id = new Guid("30106adee768438ab7365c19df3ffd52"), Name = "Relevance", Title = "Актуальність" },
                new MaterialSourceAliasEntity { Id = new Guid("4061d06fc14a454e9247ccdd6d9388f0"), Name = "Completeness", Title = "Повнота" },
                new MaterialSourceAliasEntity { Id = new Guid("5023bb79f98748fea86c38b7aa8495c4"), Name = "SourceReliability", Title = "Надійність джерела" }
        }
    }
}