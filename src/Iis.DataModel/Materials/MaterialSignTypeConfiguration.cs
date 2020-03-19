using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Materials
{
    internal sealed class MaterialSignTypeConfiguration: IEntityTypeConfiguration<MaterialSignTypeEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialSignTypeEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder.HasData(
                new MaterialSignTypeEntity { Id = new Guid("10170a812916420b8bd75688cb43b82f"), Name = "Importance", Title = "Важливість" },
                new MaterialSignTypeEntity { Id = new Guid("202f605f4fb249a7beb8e40cd41f2b83"), Name = "Reliability", Title = "Достовірність" },
                new MaterialSignTypeEntity { Id = new Guid("30106adee768438ab7365c19df3ffd52"), Name = "Relevance", Title = "Актуальність" },
                new MaterialSignTypeEntity { Id = new Guid("4061d06fc14a454e9247ccdd6d9388f0"), Name = "Completeness", Title = "Повнота" },
                new MaterialSignTypeEntity { Id = new Guid("5023bb79f98748fea86c38b7aa8495c4"), Name = "SourceReliability", Title = "Надійність джерела" }
            );
        }
    }
}
