using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.Ontology.Seeding.Odysseus
{
    public class TypeSeeder : IOntologyProvider
    {
        public void CreateBuilders(OntologyBuildContext ctx)
        {
            // Attributes - title and meta omitted
            var name = ctx.CreateBuilder().WithName("Name").WithTitle("Назва").IsAttribute().HasValueOf(ScalarType.String);
            var code = ctx.CreateBuilder().WithName("Code").WithTitle("Код").IsAttribute().HasValueOf(ScalarType.String);
            var taxId = ctx.CreateBuilder().WithName("TaxId").WithTitle("Податковий ідентифікатор").IsAttribute().HasValueOf(ScalarType.String);
            var number = ctx.CreateBuilder().WithName("Number").WithTitle("Номер").IsAttribute().HasValueOf(ScalarType.Integer);
            var count = ctx.CreateBuilder().WithName("Count").WithTitle("Кількість").IsAttribute().HasValueOf(ScalarType.Integer);
            var firstName = ctx.CreateBuilder().WithName("FirstName").WithTitle("Ім’я").IsAttribute().HasValueOf(ScalarType.String);
            var secondName = ctx.CreateBuilder().WithName("SecondName").WithTitle("Прізвище").IsAttribute().HasValueOf(ScalarType.String);
            var fatherName = ctx.CreateBuilder().WithName("FatherName").WithTitle("По батькові").IsAttribute().HasValueOf(ScalarType.String);
            var photo = ctx.CreateBuilder().WithName("Photo").WithTitle("Фото").IsAttribute().HasValueOf(ScalarType.File);
            var birthDate = ctx.CreateBuilder().WithName("BirthDate").WithTitle("Дата народження").IsAttribute().HasValueOf(ScalarType.DateTime);
            var date = ctx.CreateBuilder().WithName("Date").WithTitle("Дата").IsAttribute().HasValueOf(ScalarType.DateTime);
            var attachment = ctx.CreateBuilder().WithName("Attachment").WithTitle("Додані файли").IsAttribute().HasValueOf(ScalarType.File);
            var website = ctx.CreateBuilder().WithName("Website").WithTitle("Офіційний сайт").IsAttribute().HasValueOf(ScalarType.String);
            var text = ctx.CreateBuilder().WithName("Text").WithTitle("Текст").IsAttribute().HasValueOf(ScalarType.String);

            var dateRange = ctx.CreateBuilder().IsEntity()
                    .WithName("DateRange")
                    .WithTitle("Проміжок часу")
                    .HasOptional(date, "StartDate", title: "Початок")
                    .HasOptional(date, "EndDate", title: "Кінець")
                ;


            // Signs
            var value = ctx.CreateBuilder().WithName("Value").WithTitle("Значення").IsAttribute().HasValueOf(ScalarType.String);

            var sign = ctx.CreateBuilder().IsEntity()
                    .WithName("Sign")
                    .WithTitle("Ознака")
                    .IsAbstraction()
                    .HasOptional(value)
                ;
            var phoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("PhoneSign")
                    .WithTitle("Телефон")
                    .Is(sign)
                    .IsAbstraction()
                ;
            var cellPhoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("CellPhoneSign")
                    .WithTitle("Мобільний телефон")
                    .Is(phoneSign)
                ;
            var homePhoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("HomePhoneSign")
                    .WithTitle("Домашній телефон")
                    .Is(phoneSign)
                ;
            var customPhoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("CustomPhoneSign")
                    .WithTitle("Інший телефон")
                    .Is(phoneSign)
//                    .HasOptional(name, "phoneType")
                ;
            var emailSign = ctx.CreateBuilder().IsEntity()
                    .WithName("EmailSign")
                    .WithTitle("Електронна скринька")
                    .Is(sign)
                ;
            var socialNetworksSign = ctx.CreateBuilder().IsEntity()
                    .WithName("SocialNetworkSign")
                    .WithTitle("Соціальна мережа")
                    .Is(sign)
                ;


            // Address
            var address = ctx.CreateBuilder().IsEntity()
                    .WithName("Address")
                    .WithTitle("Адреса")
                    .HasOptional(ctx, b =>
                        b.WithName("ZipCode").WithTitle("Індекс").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Region").WithTitle("Область").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("City").WithTitle("Населений пункт").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Subregion").WithTitle("Район").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Street").WithTitle("Вулиця").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Building").WithTitle("Будинок").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Apartment").WithTitle("Квартира").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Coordinates").WithTitle("Координати").IsAttribute().HasValueOf(ScalarType.Geo))
                ;


            // Enums
            var enumEntity = ctx.CreateBuilder().IsEntity()
                    .WithName("Enum")
                    .WithTitle("Перелік")
                    .IsAbstraction()
                    .HasOptional(code)
                    .HasOptional(name)
                ;
            var abstractAccessLevel = ctx.CreateEnum("AbstractAccessLevel")
                    .WithTitle("Рівень доступу")
                    .IsAbstraction()
                ;
            var tag = ctx.CreateEnum("Tag")
                    .WithTitle("Тег")
                    .IsAbstraction()
                ;
            var accessLevel = ctx.CreateEnum("AccessLevel") // seeded
                    .WithTitle("Рівень доступу")
                    .HasOptional(number)
                    .Is(abstractAccessLevel)
                ;
            var ussrAccessLevel = ctx.CreateEnum("UssrAccessLevel") // seeded
                    .WithTitle("Рівень доступу СРСР")
                    .Is(abstractAccessLevel)
                ;
            var natoAccessLevel = ctx.CreateEnum("NatoAccessLevel")
                    .WithTitle("Рівень доступу НАТО")
                ;
            var applyToAccessLevel = ctx.CreateEnum("ApplyToAccessLevel") // seeded
                    .WithTitle("Форма, на яку подається")
                    .HasOptional(number)
                    .HasOptional(ctx, b => b
                        .WithName("Years")
                        .WithTitle("Строк дії")
                        .IsAttribute()
                        .HasValueOf(ScalarType.Integer))
                ;
            var specialPermitStatus = ctx.CreateEnum("SpecialPermitStatus") // seeded
                .WithTitle("Статус спецдозволу")
                ;
            var accessStatus = ctx.CreateEnum("AccessStatus") // seeded
                .WithTitle("Статус допуску")
                ;
            var controlType = ctx.CreateEnum("ControlType") // seeded
                .WithTitle("Тип перевірки")
                ;
            var legalForm = ctx.CreateEnum("LegalForm") // seeded
                .WithTitle("Організаційно-правова форма")
                ;
            var propertyOwnership = ctx.CreateEnum("PropertyOwnership") // seeded
                .WithTitle("Форма власності")
                ;
            var sanctionAccessConclusion = ctx.CreateEnum("SanctionAccessConclusion") // seeded
                .WithTitle("Заходи реагування")
                ;
            var country = ctx.CreateEnum("Country") // seeded
                .WithTitle("Країна")
                ;

            // Family relations
            var familyRelationKind = ctx.CreateEnum("FamilyRelationKind") // seeded
                    .WithTitle("Ступінь родинного зв’язку")
                ;
            var familyRelationInfo = ctx.CreateBuilder().IsEntity()
                    .WithName("FamilyRelationInfo")
                    .WithTitle("Родинні зв’язки")
                    .HasOptional(familyRelationKind)
                    .HasOptional(text, "FullName", title: "Прізвище, ім’я та по батькові")
                    .HasOptional(text, "DateAndPlaceOfBirth", title: "Дата та місце народження, громадянство")
                    .HasOptional(text, "WorkPlaceAndPosition", title:"Місце роботи (служби, роботи), посада")
                    .HasOptional(text, "LiveIn", title: "Місце проживання")
                    .HasOptional("Person", "PersonLink")
                ;

            // Entities
            var passport = ctx.CreateBuilder().IsEntity()
                    .WithName("Passport")
                    .WithTitle("Паспорт")
                    .HasOptional(code, title:"Серія та номер")
                    .HasOptional(ctx, b =>
                        b.WithName("IssueInfo").WithTitle("Ким виданий").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("IssueDate").WithTitle("Дата видачі").IsAttribute().HasValueOf(ScalarType.DateTime))
                ;

            var citizenship = ctx.CreateBuilder().IsEntity()
                    .WithName("Citizenship")
                    .WithTitle("Громадянство")
                    .HasOptional(country)
                    .HasOptional(taxId)
                    .HasMultiple(passport)
                ;

            var license = ctx.CreateBuilder().IsEntity()
                    .WithName("License")
                    .WithTitle("Ліцензія")
                    .HasOptional(number, "LicenseNumber", title: "Номер ліцензіі")
                    .HasOptional(number, "ApprovalNumber", title: "Номер рішення")
                    .HasOptional(attachment, "Scan", title: "Скан ліцензії")
                    .HasOptional(text, "ActivityType", title: "Вид діяльності")
                    .HasOptional(dateRange, "ValidRange", title: "Термін дії")
                ;

            // organization tabs
            var listOfPositions = ctx.CreateBuilder().IsEntity()
                    .WithName("ListOfPositions")
                    .WithTitle("Штат")
                    .HasMultiple(text, "Position", title: "Посада")
                    .HasOptional(attachment, "OriginalDocument", title: "Оригінал документа")
                ;

            var mcci = ctx.CreateBuilder().IsEntity()
                    .WithName("Mcci")
                    .WithTitle("МНСІ")
                    .HasOptional(abstractAccessLevel, "Category", title: "Категорія")
                    .HasOptional(count)
                ;

            var mciNato = ctx.CreateBuilder().IsEntity()
                    .WithName("MciNato")
                    .WithTitle("МНІ НАТО")
                    .HasOptional(natoAccessLevel, "Category", title: "Категорія")
                    .HasOptional(count)
                ;

            var mcciSpecialCommunications = ctx.CreateBuilder().IsEntity()
                    .WithName("McciSpecialCommunications")
                    .WithTitle("МНСІ органів спецзв’язку")
                    .HasOptional(accessLevel, "Category", title: "Категорія")
                    .HasOptional(count)
                ;

            var srddw = ctx.CreateBuilder().IsEntity()
                    .WithName("Srddw")
                    .WithTitle("НДДКР")
                    .HasOptional(text, "CypherName", title: "Шифр роботи")
                    .HasOptional(text, "FullName", title: "Повна назва роботи")
                    .HasOptional(text, "WorkBasis", title: "Підстава для виконання роботи")
                    .HasOptional(text, "Customer", title: "Замовник роботи")
                    .HasOptional("Organization", "Contractor",
                        EmptyRelationMeta(),
                        title: "Виконавець роботи")
                    .HasMultiple("Organization", "CoContractor",
                        EmptyRelationMeta(),
                        title: "Співвиконавець (співвиконавці)")
                    .HasOptional(accessLevel, "SecretLevel", title: "Ступінь секретності роботи")
                    .HasOptional(date, "StartDate", title: "Дата початку роботи")
                    .HasOptional(date, "EndDate", title: "Дата завершення роботи")
                    .HasOptional(attachment, "PssActivitiesPlan", title: "План заходів ОДТ по НДДКР")
                    .HasMultiple("Person", "Accessors",
                        EmptyRelationMeta(),
                        title: "Список осіб, допущених до роботи")
                    .HasOptional(ctx, b =>
                        b.WithName("State").WithTitle("Стан").IsAttribute().HasValueOf(ScalarType.Boolean))
                    .HasOptional(ctx, b =>
                        b.IsEntity().WithName("Stage").WithTitle("Етапи НДДКР")
                            .HasOptional(name)
                            .HasOptional(dateRange))
                    .HasOptional("Stage", "CurrentStage",
                        EmptyRelationMeta(),
                        title: "Поточний етап")
                ;

            var operationalData = ctx.CreateBuilder().IsEntity()
                    .WithName("OperationalData")
                    .WithTitle("Оперативні дані")
                    .HasOptional(number)
                    .HasOptional(date)
                    .HasOptional(text, "Source", title: "Джерело")
                    .HasOptional(text, "Content", title: "Зміст")
                    .HasOptional(text, "Comment", title: "Примітка")
                    .HasOptional(attachment)
                ;

            // Organization
            var organization = ctx.CreateBuilder()
                    .WithName("Organization")
                    .WithTitle("Суб'єкт")
                    .IsEntity()
                    .HasOptional(taxId, title: "Код ЄДРПОУ")
                    .HasOptional(name)
                    .HasOptional(website)
                    .HasOptional(photo)
                    .HasMultiple(ctx, b =>
                        b.WithName("OrganizationTag").WithTitle("Теги").IsEntity().Is(tag))
                    .HasOptional(propertyOwnership)
                    .HasOptional(legalForm)
                    .HasOptional(address, "LocatedAt", title: "Фактична адреса") // Address kind?
                    .HasOptional(address, "RegisteredAt", title: "Юридична адреса")
                    .HasOptional(address, "BranchAddress", title: "Філія")
                    .HasOptional(address, "SecretFacilityAddress", title: "РСО")
                    .HasOptional(address, "SecretFacilityArchiveAddress", title: "Архів РСО")
                    .HasOptional(attachment, "RSOCreationRequest", title: "Вмотивований запит на створення РСО")
                    // ... edit
                    .HasMultiple("Person", "Beneficiary",
                        EmptyRelationMeta(),
                        title: "Засновнки (бенефіциари)")
                    .HasOptional("Person", "Head",
                        EmptyRelationMeta(),
                        title: "Керівник")
                    .HasOptional(attachment, "StatuteOnEPARSS", title: "Положення про СРСД")
                    .HasOptional("Organization", "HeadOrganization",
                        CreateInversed("ChildOrganizations", "Філії", true),
                        "Відомча підпорядкованість")
                    // ... next tabs
                    .HasOptional(listOfPositions)
                    .HasMultiple(license)
                    .HasMultiple(mcci)
                    .HasMultiple(mciNato)
                    .HasMultiple(mcciSpecialCommunications)
                    .HasMultiple(srddw)
                    .HasOptional("SpecialPermit")
                ;


            // Work in
            var workIn = ctx.CreateBuilder().IsEntity()
                    .WithName("WorkIn")
                    .WithTitle("Місце роботи")
                    .HasOptional(organization)
                    .HasOptional(ctx, d =>
                        d.WithName("JobPosition").WithTitle("Посада").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(text, "Subdivision", title: "Підрозділ")
                ;


                // Person
            var person = ctx.CreateBuilder().IsEntity()
                    .WithName("Person")
                    .WithTitle("Особа")
                    .HasOptional(name, "FullName", CreateComputed("Join(secondName, firstName, fatherName)"))
                    .HasOptional(firstName)
                    .HasOptional(secondName)
                    .HasOptional(fatherName)
                    .HasOptional(photo)
                    .HasOptional(birthDate)
                    .HasOptional(address, "BirthPlace", title: "Місце народження")
                    .HasOptional(address, "RegistrationPlace", title: "Місце реєстрації")
                    .HasOptional(address, "LivingPlace", title: "Місце фактичного проживання")
                    .HasMultiple(phoneSign)
                    .HasMultiple(emailSign)
                    .HasMultiple(socialNetworksSign)
//                    .HasMultiple(citizenship)
                    .HasOptional(taxId, title: "ІПН")
                    .HasOptional(passport)
                    // ... secret carrier
                    .HasMultiple(workIn, "PastEmployments", title: "Останні місця роботи")
                    .HasOptional(workIn)
                    .HasOptional(text, "SecretCarrierAssignment", title:"Призначення на посаду державного експерта з питаннь таємниць")
                    .HasOptional(applyToAccessLevel)
                    .HasOptional(attachment, "ScanForm5", title: "Скан переліку питань (форма 5)")
                    .HasOptional(attachment, "AnswerRules", title: "Правила надання відповідей")
                    .HasOptional(attachment, "Autobiography", title: "Автобіографія")
                    .HasOptional(attachment, "Form8", title: "Форма 8")
                    .HasMultiple(familyRelationInfo, "FamilyRelations", title: "Родинні зв'язки")
                    .HasOptional("Access")
                ;


            // Permits
            var acccess = ctx.CreateBuilder().IsEntity()
                    .WithName("Access")
                    .WithTitle("Допуск")
//                    .HasOptional(person, "Person",
//                        CreateInversed("Access", "Допуск"))
                    .HasOptional(date, "IssueDate", title: "Дата видачі")
                    .HasOptional(date, "EndDate", title: "Дата завершення дії")
                    .HasOptional(accessLevel)
//                    .HasOptional(workIn)
                    .HasOptional(accessStatus) // computed?
                ;

            var organizationPermit = ctx.CreateBuilder().IsEntity()
                    .WithName("SpecialPermit")
                    .WithTitle("Спецдозвіл")
//                    .HasOptional(organization, "Organization",
//                        CreateInversed("SpecialPermit", "Спецдозвiл"))
                    .HasOptional(code, "IssueNumber", title: "Номер спецдозволу")
                    .HasOptional(date, "IssueDate", title: "Дата видачі")
                    .HasOptional(date, "EndDate", title: "Дата завершення дії")
                    .HasOptional(accessLevel)
                    .HasOptional(specialPermitStatus) // computed?
                    .HasOptional(organization, "SBU", EmptyRelationMeta()) // restrictions?
                    .HasOptional(attachment, "Scan", title: "Скан спецдозволу")
                    .HasOptional(attachment, "ScanOfAct", title: "Акт перевірки")
                    .HasMultiple("Person", "CommitteeMembers", title: "Члени комісії")
                    .HasOptional("Person", "CommitteeHead", title: "Голова комісії")
                ;


        }

        private EntityRelationMeta CreateInversed(string code, string title = null, bool multiple = false) =>
            new EntityRelationMeta {
                Inversed = new InversedRelationMeta { Code = code.ToLowerCamelcase(), Title = title, Multiple = multiple }};

        private AttributeRelationMeta CreateComputed(string formula) =>
            new AttributeRelationMeta { Formula = formula };

        private EntityRelationMeta EmptyRelationMeta() => new EntityRelationMeta();

        public Task<Ontology> GetOntologyAsync(CancellationToken cancellationToken = default)
        {
            var ctx = new OntologyBuildContext();
            CreateBuilders(ctx);
            var types = ctx.BuildOntology();
            return Task.FromResult(new Ontology(types));
        }
    }
}
