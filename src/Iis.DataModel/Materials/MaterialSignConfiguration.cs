using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Materials
{
    internal sealed class MaterialSignConfiguration : IEntityTypeConfiguration<MaterialSignEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialSignEntity> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .HasOne(e => e.MaterialSignType)
                .WithMany(e => e.MaterialSigns)
                .HasForeignKey(e => e.MaterialSignTypeId);

            builder.HasData(
                // Важливість
                new MaterialSignEntity { 
                    Id = new Guid("1107a504c2a74f8ba218e5bbf5f281c4"), MaterialSignTypeId = new Guid("10170a812916420b8bd75688cb43b82f"), 
                    ShortTitle = "1", Title = "Перша категорія", OrderNumber = 1 },
                new MaterialSignEntity { 
                    Id = new Guid("1240c5048ecd4aca9b7524f0c6304426"), MaterialSignTypeId = new Guid("10170a812916420b8bd75688cb43b82f"), 
                    ShortTitle = "2", Title = "Друга категорія", OrderNumber = 2
                },
                new MaterialSignEntity {
                    Id = new Guid("1356a6b3c63f49858b74372236fe744f"), MaterialSignTypeId = new Guid("10170a812916420b8bd75688cb43b82f"),
                    ShortTitle = "3", Title = "Третя категорія", OrderNumber = 3
                },

                // Достовірність
                new MaterialSignEntity {
                    Id = new Guid("211f576508674d04976a70f3e34bf153"), MaterialSignTypeId = new Guid("202f605f4fb249a7beb8e40cd41f2b83"),
                    ShortTitle = "Д", Title = "Достовірна", OrderNumber = 1
                },
                new MaterialSignEntity {
                    Id = new Guid("225f189b9ad2468796240d4c991a3d6b"), MaterialSignTypeId = new Guid("202f605f4fb249a7beb8e40cd41f2b83"),
                    ShortTitle = "Й", Title = "Ймовірна", OrderNumber = 2
                },
                new MaterialSignEntity {
                    Id = new Guid("2326d6ef554242a883eb0c2b92d188f1"), MaterialSignTypeId = new Guid("202f605f4fb249a7beb8e40cd41f2b83"),
                    ShortTitle = "С", Title = "Сумнівна", OrderNumber = 3
                },
                new MaterialSignEntity {
                    Id = new Guid("2475d991b09e49979d0a2fc0bf07b1eb"), MaterialSignTypeId = new Guid("202f605f4fb249a7beb8e40cd41f2b83"),
                    ShortTitle = "Н", Title = "Недостовірна", OrderNumber = 4
                },
                new MaterialSignEntity {
                    Id = new Guid("25007914d5cb4def8162c12b4aa7038c"), MaterialSignTypeId = new Guid("202f605f4fb249a7beb8e40cd41f2b83"),
                    ShortTitle = "НД", Title = "Невизначеної достовірності", OrderNumber = 5
                },
                new MaterialSignEntity {
                    Id = new Guid("2616aa7dc379452b8c1ac815f9b989bc"), MaterialSignTypeId = new Guid("202f605f4fb249a7beb8e40cd41f2b83"),
                    ShortTitle = "ДІ", Title = "Дезінформація", OrderNumber = 6
                },

                // Актуальність
                new MaterialSignEntity {
                    Id = new Guid("313d1f5b7b3a446fab92e4046930a599"), MaterialSignTypeId = new Guid("30106adee768438ab7365c19df3ffd52"),
                    ShortTitle = "У", Title = "Упереджувальна", OrderNumber = 1 
                },
                new MaterialSignEntity {
                    Id = new Guid("320c2a19ed1b4250bb02eb4f7391165b"), MaterialSignTypeId = new Guid("30106adee768438ab7365c19df3ffd52"),
                    ShortTitle = "ДА", Title = "Дуже актуальна", OrderNumber = 2
                },
                new MaterialSignEntity {
                    Id = new Guid("3317a961192949579ef008b3007648a6"), MaterialSignTypeId = new Guid("30106adee768438ab7365c19df3ffd52"),
                    ShortTitle = "А", Title = "Актуальна", OrderNumber = 3
                },
                new MaterialSignEntity {
                    Id = new Guid("341892c939184a7fbf61d5b9050de7f4"), MaterialSignTypeId = new Guid("30106adee768438ab7365c19df3ffd52"),
                    ShortTitle = "П", Title = "Посередня", OrderNumber = 4
                },
                new MaterialSignEntity {
                    Id = new Guid("354436fe8c2543528b893e94bf5828e2"), MaterialSignTypeId = new Guid("30106adee768438ab7365c19df3ffd52"),
                    ShortTitle = "НI", Title = "Неактуальна інформація", OrderNumber = 5
                },

                // Повнота
                new MaterialSignEntity {
                    Id = new Guid("4124de78087740b4834af892060ea3f5"), MaterialSignTypeId = new Guid("4061d06fc14a454e9247ccdd6d9388f0"),
                    ShortTitle = "П", Title = "Повна", OrderNumber = 1
                },
                new MaterialSignEntity {
                    Id = new Guid("422914a7f7614075a91e4d34d33aedff"), MaterialSignTypeId = new Guid("4061d06fc14a454e9247ccdd6d9388f0"),
                    ShortTitle = "ДП", Title = "Достатньо повна", OrderNumber = 2
                },
                new MaterialSignEntity {
                    Id = new Guid("431a888f406b458a9905abc752710659"), MaterialSignTypeId = new Guid("4061d06fc14a454e9247ccdd6d9388f0"),
                    ShortTitle = "Ч", Title = "Часткова", OrderNumber = 3
                },
                new MaterialSignEntity {
                    Id = new Guid("44ddf35aeeee4aa39f3c9b73dc1d63ee"), MaterialSignTypeId = new Guid("4061d06fc14a454e9247ccdd6d9388f0"),
                    ShortTitle = "НРІ", Title = "Недостатня розвідувальна інформація", OrderNumber = 4
                },

                // Надійність джерела
                new MaterialSignEntity {
                    Id = new Guid("513de8b45c99414f94f1513a716fc01c"), MaterialSignTypeId = new Guid("5023bb79f98748fea86c38b7aa8495c4"),
                    ShortTitle = "A", Title = "Повністю надійне", OrderNumber = 1
                },
                new MaterialSignEntity {
                    Id = new Guid("521ad86baf5d4731b5e7e3e69ef23fc7"), MaterialSignTypeId = new Guid("5023bb79f98748fea86c38b7aa8495c4"),
                    ShortTitle = "B", Title = "Здебільшего надійне", OrderNumber = 2
                },
                new MaterialSignEntity {
                    Id = new Guid("5342ead6d4784abcb8d1fd5d6a741706"), MaterialSignTypeId = new Guid("5023bb79f98748fea86c38b7aa8495c4"),
                    ShortTitle = "C", Title = "Відносно надійне", OrderNumber = 3
                },
                new MaterialSignEntity {
                    Id = new Guid("5406768c581d4b95a549b2cd1d09cfd8"), MaterialSignTypeId = new Guid("5023bb79f98748fea86c38b7aa8495c4"),
                    ShortTitle = "D", Title = "Не завжди надійне", OrderNumber = 4
                },
                new MaterialSignEntity {
                    Id = new Guid("55b0a03823474fb082e16081933ac9e1"), MaterialSignTypeId = new Guid("5023bb79f98748fea86c38b7aa8495c4"),
                    ShortTitle = "E", Title = "Ненадійне", OrderNumber = 5
                },
                new MaterialSignEntity {
                    Id = new Guid("5636555924fb42f28305bbef01fd6e3e"), MaterialSignTypeId = new Guid("5023bb79f98748fea86c38b7aa8495c4"),
                    ShortTitle = "F", Title = "Неможливо оцінити надійність", OrderNumber = 6
                }
            );
        }
    }
}
