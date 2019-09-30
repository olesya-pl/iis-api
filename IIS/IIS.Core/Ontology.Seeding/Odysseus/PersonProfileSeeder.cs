namespace IIS.Core.Ontology.Seeding.Odysseus
{
    public partial class TypeSeeder
    {
        private void CreatePersonProfile(OntologyBuildContext ctx, ITypeBuilder profile)
        {
            var profileQuestion = ctx.CreateBuilder().IsEntity()
                    .WithName("ProfileQuestion")
                    .IsAbstraction()
                    .AcceptEmbeddedOperations()
                ;

            var textAttachmentForm = ctx.CreateBuilder().IsEntity()
                    .WithName("TextAttachmentForm")
                    .WithTitle(null)
                    .HasOptional("Text", null, "Коментар")
                    .HasOptional("Attachment", null, "Підтвердження")
                ;

            profile
                .HasOptional(r => r
                    .Target(ctx.CreateBuilder().IsEntity().Is(profileQuestion)
                        .WithName("ReasonToGetAccess")
                        .WithTitle("14. Пункт «Номенклатури посад працівників СРСД, перебування на яких потребує оформлення допуску та надання доступу до державної таємниці» (або факт потреби у роботі з секретною інформацією без урахування номенклатури посад, із посиланням на дату) – вмотивований запит у зв’язку із оформленням допуску до державної таємниці.")
                        // null name is acceptible ONLY if there are NO same types in answers
                        .HasOptional("Text", null, "За номенклатурою посад")
                        .HasOptional("Attachment", null, "Вмотивований запит")
                        .HasOptional("Date", null, "Дата вмотивованого запиту")
                    )
                    .WithFormFieldType("ReasonToGetAccess")
                )
                .HasOptional(r => r
                    .Target(ctx.CreateBuilder().IsEntity().Is(profileQuestion)
                        .WithName("WrittenObligation")
                        .WithTitle("15. Взяття особою письмового зобов’язання у зв’язку з доступом до державної таємниці у межах ст. 27 Закону України «Про державну таємницю» - повідомлення СРСД та фотокартка з Інтернет-ресурсів.")
                        // Multiple file attributes - we should give each one a name
                        .HasOptional("Text", null, "Коментар")
                        .HasOptional("Attachment", "message", "Повідомлення СРСД")
                        .HasOptional("Attachment", "photo", "Фотокартка з інтернет-ресурсів")
                    )
                )
                .HasOptional(r => r
                    .Target(ctx.CreateBuilder().IsEntity().Is(profileQuestion)
                        .WithName("NATOCertificate")
                        .WithTitle("16. Сертифікат особового допуску НАТО (стадії: оформлення – лист СРСД; надання – дата фактичного підписання сертифікату).")
                        .HasOptional("Attachment", null, "Лист СРСД")
                        .HasOptional("Date", null, "Дата фактичного надання")
                    )
                )
                .HasOptional(r => r
                    .Target(textAttachmentForm)
                    .WithName("AccessToCryptoProtection")
                    .WithTitle("17. Доступ до засобів криптографічного захисту секретної інформації (шифри тощо).")

                )
                .HasOptional(r => r
                    .Target(textAttachmentForm)
                    .WithName("AccessToBackupManagementPoints")
                    .WithTitle("18. Доступ до запасних пунктів управління. ")

                )
                .HasOptional(r => r
                    .Target(textAttachmentForm)
                    .WithName("AccessToResearchWork")
                    .WithTitle("19. Доступ до секретних науково-дослідних та дослідно-конструкторських робіт, а також – проведення інших наукових досліджень (захист дисертації тощо). ")

                )
                .HasOptional(r => r
                    .Target(textAttachmentForm)
                    .WithName("AccessToSpecialReserachWork")
                    .WithTitle("20. Доступ до особливих робіт (ядерних установок, ядерних матеріалів, радіоактивних відходів, інших джерел іонізуючого випромінювання).")

                )
                .HasOptional(r => r
                    .Target(textAttachmentForm)
                    .WithName("AccessToOfficeInformation")
                    .WithTitle("21.  Наявність доступу до службової інформації – повідомлення підприємства, установи чи організації. ")
                )
                .HasOptional(r => r
                    .Target("Text")
                    .WithName("ForeignCountriesRelations")
                    .WithTitle("22. Зв’язки особи з іноземною державою, тимчасово окупованими територіями або невизнаними Україною територіальними утвореннями (передбачається введення переліку відповідних іноземних країн, ТОТ чи утворень) – п.п. 3, 5, 6, 7, 8, 10, 15, 16, 17, 18, 19, 20, 21, 22, 24, 25, 30, 31 Переліку питань…")
                )
                .HasOptional(r => r
                    .Target("Text")
                    .WithName("OccupiedTerritoriesVisits")
                    .WithTitle("23. Виїзд з України, до тимчасово окупованих територій або невизнаних Україною територіальних утворень (передбачається введення переліку відповідних іноземних країн, ТОТ чи утворень) – п.п. 20, 31 Переліку питань…або офіційна чи оперативна інформація.")
                )
                .HasOptional(r => r
                    .Target("Text")
                    .WithName("ForeignersRelations")
                    .WithTitle("24. Зв’язки з іноземцями, особами без громадянства, особами, які мешкають на тимчасово окупованих територіях або у невизнаних Україною територіальних утвореннях (передбачається введення переліку відповідних іноземних країн, ТОТ чи утворень, та, окрема, прізвища, ім’я, по-батькові іноземця відповідною мовою) – п.п. 22, 24 Переліку питань… або офіційна чи оперативна інформація.")
                )
                .HasOptional(r => r
                    .Target(ctx.CreateBuilder().IsEntity().IsEntity()
                        .WithName("ForeignPlaceOfResidenceRequest")
                        .WithTitle("25. Подання особою клопотання про виїзд на постійне місце проживання до іноземної держави або її залишення на постійне проживання за кордоном (передбачається введення відповідної іноземної країни)– анкета, що подана особою.")
                        .HasOptional("Attachment")
                        .HasOptional("Country")
                    )
                )


                ;


        }
    }
}
