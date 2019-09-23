using System.Linq;
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
                    .AcceptEmbeddedOperations()
                    .HasOptional(r => r
                        .Target(date)
                        .WithName("StartDate")
                        .WithTitle("Початок")
                    )
                    .HasOptional(r => r
                        .Target(date)
                        .WithName("EndDate")
                        .WithTitle("Кінець")
                    )
                ;


            // Signs
            var value = ctx.CreateBuilder().WithName("Value").WithTitle("Значення").IsAttribute().HasValueOf(ScalarType.String);

            var sign = ctx.CreateBuilder().IsEntity()
                    .WithName("Sign")
                    .WithTitle("Ознака")
                    .AcceptEmbeddedOperations()
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
                    .AcceptEmbeddedOperations()
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
                        b.WithName("Housing").WithTitle("Корпус").IsAttribute().HasValueOf(ScalarType.String))
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
                    .RejectEmbeddedOperations()
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
                    .AcceptEmbeddedOperations()
                    .HasOptional(familyRelationKind)
                    .HasOptional(text, "FullName",  "Прізвище, ім’я та по батькові")
                    .HasOptional(text, "DateAndPlaceOfBirth",  "Дата та місце народження, громадянство")
                    .HasOptional(text, "WorkPlaceAndPosition", "Місце роботи (служби, роботи), посада")
                    .HasOptional(text, "LiveIn",  "Місце проживання")
                    .HasOptional(r => r
                        .Target("Person")
                        .WithName("PersonLink")
                    )
                ;

            // Entities
            var passport = ctx.CreateBuilder().IsEntity()
                    .WithName("Passport")
                    .WithTitle("Паспорт")
                    .AcceptEmbeddedOperations()
                    .HasOptional(r => r
                        .Target(code)
                        .WithTitle("Серія та номер"))
                    .HasOptional(ctx, b =>
                        b.WithName("IssueInfo").WithTitle("Ким виданий").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("IssueDate").WithTitle("Дата видачі").IsAttribute().HasValueOf(ScalarType.DateTime))
                ;

            var citizenship = ctx.CreateBuilder().IsEntity()
                    .WithName("Citizenship")
                    .WithTitle("Громадянство")
                    .AcceptEmbeddedOperations()
                    .HasOptional(country)
                    .HasOptional(taxId)
                    .HasMultiple(passport)
                ;

            var license = ctx.CreateBuilder().IsEntity()
                    .WithName("License")
                    .WithTitle("Ліцензія")
                    .AcceptEmbeddedOperations()
                    .HasOptional(number, "LicenseNumber",  "Номер ліцензіі")
                    .HasOptional(number, "ApprovalNumber",  "Номер рішення")
                    .HasOptional(attachment, "Scan",  "Скан ліцензії")
                    .HasOptional(text, "ActivityType",  "Вид діяльності")
                    .HasOptional(dateRange, "ValidRange",  "Термін дії")
                ;

            // organization tabs
            var listOfPositions = ctx.CreateBuilder().IsEntity()
                    .WithName("ListOfPositions")
                    .WithTitle("Штат")
                    .AcceptEmbeddedOperations()
                    .HasMultiple(r => r
                        .Target(text)
                        .WithName("Position")
                        .WithTitle("Посада")
//                        .WithMeta<EntityRelationMeta>(m => m.FormField = new FormField {Type = "table"})
                        .WithFormFieldType("table")
                    )
                    .HasOptional(attachment, "OriginalDocument",  "Оригінал документа")
                ;

            var mcci = ctx.CreateBuilder().IsEntity()
                    .WithName("Mcci")
                    .WithTitle("МНСІ")
                    .AcceptEmbeddedOperations()
                    .HasOptional(abstractAccessLevel, "Category",  "Категорія")
                    .HasOptional(count)
                ;

            var mciNato = ctx.CreateBuilder().IsEntity()
                    .WithName("MciNato")
                    .WithTitle("МНІ НАТО")
                    .AcceptEmbeddedOperations()
                    .HasOptional(natoAccessLevel, "Category",  "Категорія")
                    .HasOptional(count)
                ;

            var mcciSpecialCommunications = ctx.CreateBuilder().IsEntity()
                    .WithName("McciSpecialCommunications")
                    .WithTitle("МНСІ органів спецзв’язку")
                    .AcceptEmbeddedOperations()
                    .HasOptional(accessLevel, "Category",  "Категорія")
                    .HasOptional(count)
                ;

            var stage = ctx.CreateBuilder().IsEntity()
                    .WithName("Stage")
                    .WithTitle("Етапи НДДКР")
                    .AcceptEmbeddedOperations()
                    .HasOptional(name)
                    .HasOptional(dateRange)
                ;

            var srddw = ctx.CreateBuilder().IsEntity()
                    .WithName("Srddw")
                    .WithTitle("НДДКР")
                    .AcceptEmbeddedOperations()
                    .HasOptional(text, "CypherName",  "Шифр роботи")
                    .HasOptional(text, "FullName",  "Повна назва роботи")
                    .HasOptional(text, "WorkBasis",  "Підстава для виконання роботи")
                    .HasOptional(text, "Customer",  "Замовник роботи")
                    .HasOptional(r => r
                        .Target("Organization")
                        .WithName("Contractor")
                        .WithTitle("Виконавець роботи")
                    )
                    .HasMultiple(r => r
                        .Target("Organization")
                        .WithName("CoContractor")
                        .WithTitle("Співвиконавець (співвиконавці)")
                    )
                    .HasOptional(accessLevel, "SecretLevel",  "Ступінь секретності роботи")
                    .HasOptional(date, "StartDate",  "Дата початку роботи")
                    .HasOptional(date, "EndDate",  "Дата завершення роботи")
                    .HasOptional(attachment, "PssActivitiesPlan",  "План заходів ОДТ по НДДКР")
                    .HasMultiple(r => r
                        .Target("Person")
                        .WithName("Accessors")
                        .WithTitle("Список осіб, допущених до роботи")
                    )
                    .HasOptional(ctx, b =>
                        b.WithName("State").WithTitle("Стан").IsAttribute().HasValueOf(ScalarType.Boolean))
                    .HasMultiple(r => r
                            .Target(stage)
//                            .WithMeta<EntityRelationMeta>(m => m.FormField = new FormField {Type = "table"})
                            .WithFormFieldType("table")
                        )
                    .HasOptional(r => r
                        .Target(stage)
                        .WithName("CurrentStage")
                        .WithTitle("Поточний етап")
                    )
                ;

            var operationalData = ctx.CreateBuilder().IsEntity()
                    .WithName("OperationalData")
                    .WithTitle("Оперативні дані")
                    .AcceptEmbeddedOperations()
                    .HasOptional(number)
                    .HasOptional(date)
                    .HasOptional(text, "Source",  "Джерело")
                    .HasOptional(text, "Content",  "Зміст")
                    .HasOptional(text, "Comment",  "Примітка")
                    .HasOptional(attachment)
                ;

            // Organization
            var organization = ctx.CreateBuilder()
                    .WithName("Organization")
                    .WithTitle("Суб'єкт")
                    .RejectEmbeddedOperations()
                    .IsEntity()
                    .HasOptional(r => r
                        .Target(taxId)
                        .WithTitle("Код ЄДРПОУ")
                    )
                    .HasOptional(name)
                    .HasOptional(website)
                    .HasOptional(photo)
                    .HasMultiple(ctx, b =>
                        b.WithName("OrganizationTag").WithTitle("Теги").IsEntity().Is(tag))
                    .HasOptional(propertyOwnership)
                    .HasOptional(legalForm)
                    .HasOptional(address, "LocatedAt",  "Фактична адреса") // Address kind?
                    .HasOptional(address, "RegisteredAt",  "Юридична адреса")
                    .HasOptional(address, "BranchAddress",  "Філія")
                    .HasOptional(address, "SecretFacilityAddress",  "РСО")
                    .HasOptional(address, "SecretFacilityArchiveAddress",  "Архів РСО")
                    .HasOptional(attachment, "RSOCreationRequest",  "Вмотивований запит на створення РСО")
                    // ... edit
                    .HasMultiple(r => r
                        .Target("Person")
                        .WithName("Beneficiary")
                        .WithTitle("Засновнки (бенефіциари)")
                    )
                    .HasOptional(r => r
                        .Target("Person")
                        .WithName("Head")
                        .WithTitle("Керівник")
                    )
                    .HasOptional(r => r
                        .Target("Organization")
                        .WithName("HeadOrganization")
                        .WithTitle("Відомча підпорядкованість")
                        .HasInversed(ir => ir
                            .WithName("ChildOrganizations")
                            .WithTitle("Філії")
                            .IsMultiple()
                        )
                    )
                    .HasOptional(attachment, "StatuteOnEPARSS",  "Положення про СРСД")
                    // ... next tabs
                    .HasMultiple(license)
                    .HasOptional(r => r
                        .Target(listOfPositions)
                        .WithFormFieldType("form"))
                    .HasMultiple(r => r
                        .Target(mcci)
                        .WithFormFieldType("table"))
                    .HasMultiple(r => r
                        .Target(mciNato)
                        .WithFormFieldType("table"))
                    .HasMultiple(r => r
                        .Target(mcciSpecialCommunications).WithFormFieldType("table"))
                    .HasMultiple(r => r
                        .Target(srddw)
                        .WithFormFieldType("form"))
                    .HasOptional(r => r
                        .Target("SpecialPermit")
                        .WithFormFieldType("form"))
                ;


            // Work in
            var workIn = ctx.CreateBuilder().IsEntity()
                    .WithName("WorkIn")
                    .WithTitle("Місце роботи")
                    .AcceptEmbeddedOperations()
                    .HasOptional(organization)
                    .HasOptional(ctx, d =>
                        d.WithName("JobPosition").WithTitle("Посада").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(text, "Subdivision",  "Підрозділ")
                ;


            // Person
            var person = ctx.CreateBuilder().IsEntity()
                    .WithName("Person")
                    .WithTitle("Особа")
                    .RejectEmbeddedOperations()
                    .HasOptional(r => r
                        .Target(name)
                        .WithName("FullName")
                        .WithTitle("Повне ім'я")
                        .WithFormula("Join(secondName, firstName, fatherName)")
                    )
                    .HasOptional(firstName)
                    .HasOptional(secondName)
                    .HasOptional(fatherName)
                    .HasOptional(photo)
                    .HasOptional(birthDate)
                    .HasOptional(address, "BirthPlace",  "Місце народження")
                    .HasOptional(address, "RegistrationPlace",  "Місце реєстрації")
                    .HasOptional(address, "LivingPlace",  "Місце фактичного проживання")
                    .HasMultiple(phoneSign)
                    .HasMultiple(emailSign)
                    .HasMultiple(socialNetworksSign)
//                    .HasMultiple(citizenship)
                    .HasOptional(r => r
                        .Target(taxId)
                        .WithTitle("ІПН")
                    )
                    .HasOptional(passport)
                    // ... secret carrier
                    .HasMultiple(workIn, "PastEmployments",  "Останні місця роботи")
                    .HasOptional(workIn)
                    .HasOptional(text, "SecretCarrierAssignment", "Призначення на посаду державного експерта з питаннь таємниць")
                    .HasOptional(applyToAccessLevel)
                    .HasOptional(attachment, "ScanForm5",  "Скан переліку питань (форма 5)")
                    .HasOptional(attachment, "AnswerRules",  "Правила надання відповідей")
                    .HasOptional(attachment, "Autobiography",  "Автобіографія")
                    .HasOptional(attachment, "Form8",  "Форма 8")
                    .HasMultiple(familyRelationInfo, "FamilyRelations",  "Родинні зв'язки")
                    .HasOptional("Access")
                ;


            // Permits
            var acccess = ctx.CreateBuilder().IsEntity()
                    .WithName("Access")
                    .WithTitle("Допуск")
                    .AcceptEmbeddedOperations()
                    .HasOptional(date, "IssueDate",  "Дата видачі")
                    .HasOptional(date, "EndDate",  "Дата завершення дії")
                    .HasOptional(accessLevel)
//                    .HasOptional(workIn)
                    .HasOptional(accessStatus) // computed?
                ;

            var organizationPermit = ctx.CreateBuilder().IsEntity()
                    .WithName("SpecialPermit")
                    .WithTitle("Спецдозвіл")
                    .AcceptEmbeddedOperations()
                    .HasOptional(code, "IssueNumber",  "Номер спецдозволу")
                    .HasOptional(date, "IssueDate",  "Дата видачі")
                    .HasOptional(date, "EndDate",  "Дата завершення дії")
                    .HasOptional(accessLevel)
                    .HasOptional(specialPermitStatus) // computed?
                    .HasOptional(r => r
                        .Target(organization)
                        .WithName("SBU")
                        .WithTitle("СБУ")
                    ) // restrictions?
                    .HasOptional(attachment, "Scan",  "Скан спецдозволу")
                    .HasOptional(attachment, "ScanOfAct",  "Акт перевірки")
                    .HasMultiple(r => r
                        .Target("Person")
                        .WithName( "CommitteeMembers")
                        .WithTitle("Члени комісії"))
                    .HasOptional(r => r
                        .Target("Person")
                        .WithName( "CommitteeHead")
                        .WithTitle("Голова комісії"))
                ;
        }

        public Task<Ontology> GetOntologyAsync(CancellationToken cancellationToken = default)
        {
            var ctx = new OntologyBuildContext();
            CreateBuilders(ctx);
            var types = ctx.BuildOntology();
            return Task.FromResult(new Ontology(types));
        }
    }
}
