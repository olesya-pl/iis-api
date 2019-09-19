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
            var name = ctx.CreateBuilder().WithName("Name").IsAttribute().HasValueOf(ScalarType.String);
            var code = ctx.CreateBuilder().WithName("Code").IsAttribute().HasValueOf(ScalarType.String);
            var taxId = ctx.CreateBuilder().WithName("TaxId").IsAttribute().HasValueOf(ScalarType.String);
            var number = ctx.CreateBuilder().WithName("Number").IsAttribute().HasValueOf(ScalarType.Integer);
            var firstName = ctx.CreateBuilder().WithName("FirstName").IsAttribute().HasValueOf(ScalarType.String);
            var secondName = ctx.CreateBuilder().WithName("SecondName").IsAttribute().HasValueOf(ScalarType.String);
            var fatherName = ctx.CreateBuilder().WithName("FatherName").IsAttribute().HasValueOf(ScalarType.String);
            var photo = ctx.CreateBuilder().WithName("Photo").IsAttribute().HasValueOf(ScalarType.File);
            var birthDate = ctx.CreateBuilder().WithName("BirthDate").IsAttribute().HasValueOf(ScalarType.DateTime);
            var date = ctx.CreateBuilder().WithName("Date").IsAttribute().HasValueOf(ScalarType.DateTime);
            var attachment = ctx.CreateBuilder().WithName("Attachment").IsAttribute().HasValueOf(ScalarType.File);
            var website = ctx.CreateBuilder().WithName("Website").IsAttribute().HasValueOf(ScalarType.String);
            var text = ctx.CreateBuilder().WithName("Text").IsAttribute().HasValueOf(ScalarType.String);


            // Signs
            var value = ctx.CreateBuilder().WithName("Value").IsAttribute().HasValueOf(ScalarType.String);

            var sign = ctx.CreateBuilder().IsEntity()
                    .WithName("Sign")
                    .IsAbstraction()
                    .HasOptional(value)
                ;
            var phoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("PhoneSign")
                    .Is(sign)
                ;
            var cellPhoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("CellPhoneSign")
                    .Is(phoneSign)
                ;
            var homePhoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("HomePhoneSign")
                    .Is(phoneSign)
                ;
            var customPhoneSign = ctx.CreateBuilder().IsEntity()
                    .WithName("CustomPhoneSign")
                    .Is(phoneSign)
                    .HasOptional(name, "phoneType")
                ;
            var emailSign = ctx.CreateBuilder().IsEntity()
                    .WithName("EmailSign")
                    .Is(sign)
                ;
            var socialNetworksSign = ctx.CreateBuilder().IsEntity()
                    .WithName("SocialNetworkSign")
                    .Is(sign)
                ;


            // Address
            var address = ctx.CreateBuilder().IsEntity()
                    .WithName("Address")
                    .HasOptional(ctx, b =>
                        b.WithName("ZipCode").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Region").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("City").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Street").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Building").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Apartment").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Coordinates").IsAttribute().HasValueOf(ScalarType.Geo))
                ;


            // Enums
            var enumEntity = ctx.CreateBuilder().IsEntity()
                    .WithName("Enum")
                    .IsAbstraction()
                    .HasOptional(code)
                    .HasOptional(name)
                ;
            var tag = ctx.CreateEnum("Tag")
                    .IsAbstraction()
                ;
            var accessLevel = ctx.CreateEnum("AccessLevel") // seeded
                    .HasOptional(number)
                ;
            var applyToAccessLevel = ctx.CreateEnum("ApplyToAccessLevel") // seeded
                    .WithTitle("Форма, на яку подаеться")
                    .HasOptional(number)
                    .HasOptional(ctx, b => b
                        .WithName("Years").IsAttribute().HasValueOf(ScalarType.Integer))
                ;
            var specialPermitStatus = ctx.CreateEnum("SpecialPermitStatus") // seeded
                ;
            var accessStatus = ctx.CreateEnum("AccessStatus") // seeded
                ;
            var controlType = ctx.CreateEnum("ControlType") // seeded
                ;
            var legalForm = ctx.CreateEnum("LegalForm") // seeded
                ;
            var propertyOwnership = ctx.CreateEnum("PropertyOwnership") // seeded
                ;
            var sanctionAccessConclusion = ctx.CreateEnum("SanctionAccessConclusion") // seeded
                ;
            var country = ctx.CreateEnum("Country") // seeded
                ;

            // Family relations
            var familyRelationKind = ctx.CreateEnum("FamilyRelationKind") // seeded
                    .WithTitle("Ступень родинного зв’язку")
                ;
            var familyRelationInfo = ctx.CreateBuilder().IsEntity()
                    .WithName("FamilyRelationInfo")
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
                    .HasOptional(code)
                    .HasOptional(ctx, b =>
                        b.WithName("IssueInfo").IsAttribute().HasValueOf(ScalarType.String))
                ;

            var citizenship = ctx.CreateBuilder().IsEntity()
                    .WithName("Citizenship")
                    .HasOptional(country)
                    .HasOptional(taxId)
                    .HasMultiple(passport)
                ;

            // Organization
            var organization = ctx.CreateBuilder()
                    .WithName("Organization")
                    .IsEntity()
                    .HasOptional(taxId)
                    .HasOptional(name)
                    .HasOptional(website)
                    .HasOptional(photo)
                    .HasMultiple(ctx, b =>
                        b.WithName("OrganizationTag").IsEntity().Is(tag))
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
                .HasOptional(organization)
                .HasOptional(ctx, d =>
                    d.WithName("JobPosition").IsAttribute().HasValueOf(ScalarType.String));


            // Person
            var person = ctx.CreateBuilder().IsEntity()
                    .WithName("Person")
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
