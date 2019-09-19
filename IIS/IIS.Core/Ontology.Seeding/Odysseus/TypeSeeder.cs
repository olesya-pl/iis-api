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
            var taxId = ctx.CreateBuilder().WithName("TaxId").WithTitle("Код ЄДРПОУ").IsAttribute().HasValueOf(ScalarType.String);
            var number = ctx.CreateBuilder().WithName("Number").WithTitle("Номер").IsAttribute().HasValueOf(ScalarType.Integer);
            var firstName = ctx.CreateBuilder().WithName("FirstName").WithTitle("Ім’я").IsAttribute().HasValueOf(ScalarType.String);
            var secondName = ctx.CreateBuilder().WithName("SecondName").WithTitle("Прізвище").IsAttribute().HasValueOf(ScalarType.String);
            var fatherName = ctx.CreateBuilder().WithName("FatherName").WithTitle("По батькові").IsAttribute().HasValueOf(ScalarType.String);
            var photo = ctx.CreateBuilder().WithName("Photo").WithTitle("Фото").IsAttribute().HasValueOf(ScalarType.File);
            var birthDate = ctx.CreateBuilder().WithName("BirthDate").WithTitle("Дата народження").IsAttribute().HasValueOf(ScalarType.DateTime);
            var date = ctx.CreateBuilder().WithName("Date").WithTitle("Дата").IsAttribute().HasValueOf(ScalarType.DateTime);
            var attachment = ctx.CreateBuilder().WithName("Attachment").WithTitle("Додані файли").IsAttribute().HasValueOf(ScalarType.File);
            var website = ctx.CreateBuilder().WithName("Website").WithTitle("Офіційний сайт").IsAttribute().HasValueOf(ScalarType.String);
            var text = ctx.CreateBuilder().WithName("Text").WithTitle("Текст").IsAttribute().HasValueOf(ScalarType.String);


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
                ;
            var cellPhoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("CellPhoneSign")
                    .WithTitle("Мобільний телефон")
                    .Is(phoneSign)
                ;
            var homePhoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("HomePhoneSign")
                    .WithTitle("Стаціонарний телефон")
                    .Is(phoneSign)
                ;
            var customPhoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("CustomPhoneSign")
                    .WithTitle("Персоналізований телефон")
                    .Is(phoneSign)
                    .HasOptional(name, "phoneType")
                ;
            var emailSign = ctx.CreateBuilder().IsEntity()
                    .WithName("EmailSign")
                    .WithTitle("Електронна пошта")
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
                    .HasOptional(ctx, b =>
                        b.WithName("ZipCode").WithTitle("Поштовий індекс").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Region").WithTitle("Регіон").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("City").WithTitle("Місто").IsAttribute().HasValueOf(ScalarType.String))
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
            var tag = ctx.CreateEnum("Tag")
                    .WithTitle("Тег")
                    .IsAbstraction()
                ;
            var accessLevel = ctx.CreateEnum("AccessLevel") // seeded
                    .WithTitle("Рівень доступу")
                    .HasOptional(number)
                ;
            var applyToAccessLevel = ctx.CreateEnum("ApplyToAccessLevel") // seeded
                    .WithTitle("Форма, на яку подаеться")
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
                    .WithTitle("Ступень родинного зв’язку")
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
                    .HasOptional(code)
                    .HasOptional(ctx, b =>
                        b.WithName("IssueInfo").WithTitle("Відомості про порушення").IsAttribute().HasValueOf(ScalarType.String))
                ;

            var citizenship = ctx.CreateBuilder().IsEntity()
                    .WithName("Citizenship")
                    .WithTitle("Громадянство")
                    .HasOptional(country)
                    .HasOptional(taxId)
                    .HasMultiple(passport)
                ;

            // Organization
            var organization = ctx.CreateBuilder()
                    .WithName("Organization")
                    .WithTitle("Огранізація")
                    .IsEntity()
                    .HasOptional(taxId)
                    .HasOptional(name)
                    .HasOptional(website)
                    .HasOptional(photo)
                    .HasMultiple(ctx, b =>
                        b.WithName("OrganizationTag").WithTitle("Теги").IsEntity().Is(tag))
                    .HasOptional(propertyOwnership)
                    .HasOptional(legalForm)
                    .HasOptional(address, "LocatedAt") // Address kind?
                    .HasOptional(address, "RegisteredAt")
                    .HasOptional(address, "BranchAddress")
                    .HasOptional(address, "SecretFacilityAddress")
                    .HasOptional(address, "SecretFacilityArchiveAddress")
                    .HasOptional(attachment, "RSOCreationRequest")
                    // ... edit
                    .HasMultiple("Person", "Beneficiary", title: "Засновнки (бенефіциари)")
                    .HasOptional("Person", "Head", title: "Керівник")
                    .HasOptional(attachment, "StatuteOnEPARSS", title: "Положення про СРСД")
                    .HasOptional("Organization", "HeadOrganization",
                        CreateInversed("ChildOrganizations", "Дочірні організаціі", true),
                        "Відомча підпорядкованість")
                ;


            // Work in
            var workIn = ctx.CreateBuilder().IsEntity()
                .WithName("WorkIn")
                .WithTitle("Поточне місце роботи")
                .HasOptional(organization)
                .HasOptional(ctx, d =>
                    d.WithName("JobPosition").WithTitle("Посада").IsAttribute().HasValueOf(ScalarType.String));


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
                    .HasOptional(address, "BirthPlace")
                    .HasOptional(address, "RegistrationPlace")
                    .HasOptional(address, "LivingPlace")
                    .HasMultiple(phoneSign)
//                    .HasMultiple(citizenship)
                    .HasOptional(taxId)
                    .HasOptional(passport)
                // ... secret carrier
                    .HasMultiple(workIn)
                    .HasOptional(applyToAccessLevel)
                    .HasOptional(attachment, "ScanForm5")
                    .HasOptional(attachment, "AnswerRules")
                    .HasOptional(attachment, "Autobiography")
                    .HasOptional(attachment, "Form8")
                    .HasMultiple(familyRelationInfo, "FamilyRelations")
                ;


            // Permits
            var acccess = ctx.CreateBuilder().IsEntity()
                    .WithName("Access")
                    .WithTitle("Допуск")
                    .HasOptional(person, "Person",
                        CreateInversed("Access", "Допуск"))
                    .HasOptional(date, "IssueDate")
                    .HasOptional(date, "EndDate")
                    .HasOptional(accessLevel)
//                    .HasOptional(workIn)
                    .HasOptional(accessStatus) // computed?
                ;

            var organizationPermit = ctx.CreateBuilder().IsEntity()
                    .WithName("SpecialPermit")
                    .WithTitle("Спецдозвіл")
                    .HasOptional(organization, "Organization",
                        CreateInversed("SpecialPermit", "Спецдозвiл"))
                    .HasOptional(code, "IssueNumber")
                    .HasOptional(date, "IssueDate")
                    .HasOptional(date, "EndDate")
                    .HasOptional(accessLevel)
                    .HasOptional(specialPermitStatus) // computed?
                    .HasOptional(organization, "SBU") // restrictions?
                ;

        }

        private EntityRelationMeta CreateInversed(string code, string title = null, bool multiple = false) =>
            new EntityRelationMeta {
                AcceptsEntityOperations = new[] {EntityOperation.Create, EntityOperation.Update, EntityOperation.Delete},
                Inversed = new InversedRelationMeta { Code = code.ToLowerCamelcase(), Title = title, Multiple = multiple }};

        private AttributeRelationMeta CreateComputed(string formula) =>
            new AttributeRelationMeta { Formula = formula };

        public Task<Ontology> GetOntologyAsync(CancellationToken cancellationToken = default)
        {
            var ctx = new OntologyBuildContext();
            CreateBuilders(ctx);
            var types = ctx.BuildOntology();
            return Task.FromResult(new Ontology(types));
        }
    }
}
