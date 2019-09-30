using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.Ontology.Seeding.Odysseus
{
    public partial class TypeSeeder
    {
        public void CreateBuilders(OntologyBuildContext ctx)
        {
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
            var boolean = ctx.CreateBuilder().WithName("Boolean").WithTitle("Булево").IsAttribute().HasValueOf(ScalarType.Boolean);

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
                ; // all incoming relations - form-field type: "dateRange"


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
            var operationalDataSource = ctx.CreateEnum("OperationalDataSource") // seeded
                    .WithTitle("Джерела оперативної інформації")
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
                    .HasOptional(r => r
                        .Target(dateRange)
                        .WithName("ValidRange")
                        .WithTitle("Термін дії")
                        .WithFormFieldType("dateRange")
                    )
                ;

            // organization tabs
            var listOfPositionsItem = ctx.CreateBuilder().IsEntity()
                    .WithName("ListOfPositionsItem")
                    .AcceptEmbeddedOperations()
                    .HasOptional(r => r
                            .Target(text)
                            .WithName("Position")
                            .WithTitle("Посада")
                    )
                    .HasOptional(r => r
                            .Target(number)
                            .WithName("Count")
                            .WithTitle("Кількість")
                    )
                ;

            var listOfPositions = ctx.CreateBuilder().IsEntity()
                    .WithName("ListOfPositions")
                    .WithTitle("Штат")
                    .AcceptEmbeddedOperations()
                    .HasMultiple(r => r
                        .Target(listOfPositionsItem)
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
                    .HasOptional(r => r
                        .Target(dateRange)
                        .WithFormFieldType("dateRange")
                    )
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
                    .HasOptional(date)
                    .HasOptional(operationalDataSource, "Source",  "Джерело")
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
                        .Target(mcciSpecialCommunications)
                        .WithFormFieldType("table"))
                    .HasMultiple(r => r
                        .Target(srddw)
                        .WithFormFieldType("form"))
                    .HasOptional(r => r
                        .Target("SpecialPermit")
                        .WithFormFieldType("form"))
                    .HasMultiple(r => r
                        .Target(operationalData)
                        .WithMeta<EntityRelationMeta>(m =>
                            m.FormField = new FormField {Type = "table", HasIndexColumn = true})
                    )
                ;


            // Work in
            var workIn = ctx.CreateBuilder().IsEntity()
                    .WithName("WorkIn")
                    .WithTitle("Місце роботи")
                    .AcceptEmbeddedOperations()
                    .HasOptional(r => r
                        .Target(organization)
                        .HasInversed(ir => ir
                            .WithName("Employees")
                            .WithTitle("Працівники")
                            .IsMultiple()
                        )
                    )
                    .HasOptional(text, "JobPosition", "Посада")
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
                    .HasOptional(address, "BirthPlace", "Місце народження")
                    .HasOptional(address, "RegistrationPlace", "Місце реєстрації")
                    .HasOptional(address, "LivingPlace", "Місце фактичного проживання")
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
//                    .HasMultiple(workIn, "PastEmployments",  "Останні місця роботи")
                    .HasOptional(r => r
                        .Target(workIn)
                        .HasInversed(ir => { })
                    )
                    .HasOptional(text, "SecretCarrierAssignment", "Призначення на посаду державного експерта з питаннь таємниць")
                    .HasOptional(attachment, "ScanForm5", "Скан переліку питань (форма 5)")
                    .HasOptional(attachment, "AnswerRules", "Правила надання відповідей")
                    .HasOptional(attachment, "Autobiography", "Автобіографія")
                    .HasOptional(attachment, "Form8", "Форма 8")
                    .HasMultiple(familyRelationInfo, "FamilyRelations", "Родинні зв'язки")
                    .HasOptional("Access")
                    .HasMultiple(r => r
                        .Target(operationalData)
                        .WithMeta<EntityRelationMeta>(m =>
                            m.FormField = new FormField {Type = "table", HasIndexColumn = true})
                    )
                    .HasOptional("PersonControl")
                ;


            // Permits
            var access = ctx.CreateBuilder().IsEntity()
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

            // ----- Sanctions and vetting procedures ----- //

//            var legalAct = ctx.CreateBuilder().IsEntity()
//                    .WithName("LegalAct")
//                    .WithTitle(null)
//                    .HasRequired(name)
//                ;

            var legalActArticle = ctx.CreateBuilder().IsEntity()
                    .WithName("LegalActArticle")
                    .WithTitle(null)
                    .HasOptional(code, "Number", null)
                    .HasOptional(text, "Content", null)
//                    .HasOptional(r => r
//                        .Target(legalAct)
//                        .HasInversed(ir => ir.IsMultiple())
//                    )
                ;

            var criminalActArticle = ctx.CreateBuilder().IsEntity()
                    .WithName("CriminalActArticle")
                    .WithTitle(null)
                    .Is(legalActArticle)
                ;

            var administrativeActArticle = ctx.CreateBuilder().IsEntity()
                    .WithName("AdministrativeActArticle")
                    .WithTitle(null)
                    .Is(legalActArticle)
                ;

            var legalDocument = ctx.CreateBuilder().IsEntity()
                    .WithName("LegalDocument")
                    .WithTitle(null)
                    .AcceptEmbeddedOperations()
                    .HasOptional(date)
                    .HasOptional(code, "Number", null)
                    .HasOptional(attachment, "Original", null)
                    .HasOptional(text, "Content", null)
                ;

            var sanction = ctx.CreateBuilder().IsEntity()
                    .WithName("Sanction")
                    .WithTitle(null)
                    .IsAbstraction()
                    .AcceptEmbeddedOperations()
                ;

            // organization sanctions
            var organizationSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("OrganizationSanction")
                    .WithTitle(null)
                    .Is(sanction)
                    .IsAbstraction()
                ;

            var informingSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("InformingSanction")
                    .WithTitle(null)
                    .Is(organizationSanction)
                    .HasOptional(legalDocument, "Inform", null)
                    .HasOptional(legalDocument, "Answer", null)
                ;

            var organizationPermitSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("OrganizationPermitSanction")
                    .WithTitle(null)
                    .Is(organizationSanction)
                    .IsAbstraction()
                    .HasOptional(legalDocument, "Decree", null)
                    .HasOptional(legalActArticle, "BreachedArticles", null)
                    .HasOptional(text, "BreachInfo", null)
                ;

            var terminationSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("TerminationSanction")
                    .WithTitle(null)
                    .Is(organizationPermitSanction)
                ;

            var cancellationSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("CancellationSanction")
                    .WithTitle(null)
                    .Is(organizationPermitSanction)
                ;

            // person sanctions
            var disciplinarySanctionKind = ctx.CreateEnum("DisciplinarySanctionKind")
                    .WithTitle(null)
                ;

            var personSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("PersonSanction")
                    .WithTitle(null)
                    .IsAbstraction()
                    .HasOptional(person, "person", null)
                    .HasOptional(access)
                ;

            var disciplinaryPersonSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("DisciplinaryPersonSanction")
                    .WithTitle(null)
                    .Is(personSanction)
                    .HasOptional(legalDocument, "Report", null)
                    .HasOptional(disciplinarySanctionKind)
                ;

            var investigationPersonSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("InvestigationPersonSanction")
                    .WithTitle(null)
                    .Is(personSanction)
                    .HasMultiple(sanction)
                ;

            var criminalPersonSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("CriminalPersonSanction")
                    .WithTitle(null)
                    .Is(personSanction)
                    .HasOptional(text, "InvestigateBody", null)
                    .HasOptional(code, "URPINumber", null)
                    .HasMultiple(legalActArticle, "BreachedArticles", null)
                    .HasOptional(text, "Description", null)
                    .HasOptional(legalDocument, "Resolution", null)
                    .HasOptional(organization, "Court", null)
                ;

            var reportPersonSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("ReportPersonSanction")
                    .WithTitle(null)
                    .Is(personSanction)
                    .HasOptional(person, "DrawnUpBy", null)
                    .HasOptional(legalDocument, "Report", null)
                    .HasOptional(legalActArticle, "BreachedArticles", null)
                    .HasOptional(text, "Description", null)
                    .HasOptional(legalDocument, "Resolution", null)
                    .HasOptional(legalActArticle, "ResolutionArticles", null)
                ;

            var finalPersonSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("FinalPersonSanction")
                    .WithTitle(null)
                    .Is(personSanction)
                    .IsAbstraction()
                    .HasOptional(legalDocument, "Decree", null)
                    .HasOptional(legalActArticle, "BreachedArticles", null)
                    .HasOptional(text, "BreachInfo", null)
                ;

            var terminationPersonSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("TerminationPersonSanction")
                    .WithTitle(null)
                    .Is(finalPersonSanction)
                ;

            var cancellationPersonSanction = ctx.CreateBuilder().IsEntity()
                    .WithName("CancellationPersonSanction")
                    .WithTitle(null)
                    .Is(finalPersonSanction)
                ;

            // sanctions end

            var vettingProcedureType = ctx.CreateEnum("VettingProcedureType")
                    .WithTitle(null)
                ;

            var vettingProcedureKind = ctx.CreateEnum("VettingProcedureKind")
                    .WithTitle(null)
                ;


            var vettingProcedure = ctx.CreateBuilder().IsEntity()
                    .WithName("VettingProcedure")
                    .WithTitle(null)
                    .AcceptEmbeddedOperations()
                    .HasOptional(legalDocument, "Order", null)
                    .HasOptional(vettingProcedureType)
                    .HasOptional(vettingProcedureKind)
                    .HasOptional(r => r
                        .Target(dateRange)
                        .WithName("Duration")
                        .WithTitle(null)
                        .WithFormFieldType("dateRange")
                    )
                    .HasOptional(r => r
                        .Target(dateRange)
                        .WithName("CheckPeriod")
                        .WithTitle(null)
                        .WithFormFieldType("dateRange")
                    )
                    .HasOptional(r => r
                        .Target(organization)
                        .WithName("ConductedBy")
                        .WithTitle(null)
                        .HasInversed(ir => ir
                            .IsMultiple()
                        )
                    )
                    .HasOptional(legalDocument, "InspectionAct", null)
                    .HasMultiple(sanction)
                    .HasOptional(person, "CommitteeHead", null)
                    .HasMultiple(person, "CommitteeMembers", null)
                    .HasOptional(organizationPermit)
                ;

            // Person Control
            var personControl = ctx.CreateBuilder().IsEntity()
                    .WithName("PersonControl")
                    .WithTitle(null)
                    .AcceptEmbeddedOperations()
                    .HasOptional(attachment, "RequestAttachment", "Вмотивований запит")
                    .HasOptional(applyToAccessLevel)
                    .HasOptional(r => r
                        .Target(ctx.CreateEnum("PersonCheckResult"))
                        .WithName("CheckResult")
                        .WithTitle("Резолюція по кандидату")
                    )
                    .HasOptional(attachment, "ResultAttachment", "Розпорядження (результат)")
                    .HasOptional(date, "Date", "Дата розпорядження")
                ;


//            var personProfile = ctx.CreateBuilder().IsEntity()
//                    .WithName("PersonProfile")
//                    .WithTitle("Профайл людини")
//                    .AcceptEmbeddedOperations()
//                ;
//            CreatePersonProfile(ctx, personProfile);
//            person.HasOptional(personProfile);

            CreatePersonProfile(ctx, person);
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
