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


            var boolAttr = ctx.CreateBuilder().WithName("BoolAttr").IsAttribute().HasValueOf(ScalarType.Boolean);
            var strAttr = ctx.CreateBuilder().WithName("StrAttr").IsAttribute().HasValueOf(ScalarType.String);
            var fileAttr = ctx.CreateBuilder().WithName("FileAttr").IsAttribute().HasValueOf(ScalarType.File);
            var dateAttr = ctx.CreateBuilder().WithName("DateAttr").IsAttribute().HasValueOf(ScalarType.DateTime);

            profile
                .HasOptional(r => r
                    .Target(ctx.CreateBuilder().IsEntity().Is(profileQuestion)
                        .WithName("Question14")
                        .WithTitle("14. Пункт «Номенклатури посад працівників СРСД, перебування на яких потребує оформлення допуску та надання доступу до державної таємниці» (або факт потреби у роботі з секретною інформацією без урахування номенклатури посад, із посиланням на дату) – вмотивований запит у зв’язку із оформленням допуску до державної таємниці.")
                        // null name is acceptible ONLY if there are NO same types in answers
                        .HasOptional(boolAttr, null, "За номенклатурою посад")
                        .HasOptional(fileAttr, null, "Вмотивований запит")
                        .HasOptional(dateAttr, null, "Дата вмотивованого запиту")
                    )
                )
                .HasOptional(r => r
                    .Target(ctx.CreateBuilder().IsEntity().Is(profileQuestion)
                        .WithName("Question15")
                        .WithTitle("15. Взяття особою письмового зобов’язання у зв’язку з доступом до державної таємниці у межах ст. 27 Закону України «Про державну таємницю» - повідомлення СРСД та фотокартка з Інтернет-ресурсів.")
                        // Multiple file attributes - we should give each one a name
                        .HasOptional(fileAttr, "message", "Повідомлення СРСД")
                        .HasOptional(fileAttr, "photo", "Фотокартка з інтернет-ресурсів")
                    )
                )
                .HasOptional(r => r
                    .Target(ctx.CreateBuilder().IsEntity().Is(profileQuestion)
                        .WithName("Question16")
                        .WithTitle("16. Сертифікат особового допуску НАТО (стадії: оформлення – лист СРСД; надання – дата фактичного підписання сертифікату).")
                        .HasOptional(fileAttr, null, "Лист СРСД")
                        .HasOptional(dateAttr, null, "Дата фактичного надання")
                    )
                )
                ;
        }
    }
}
