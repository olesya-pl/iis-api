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
                    .HasRequired(value)
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
            var taxId = ctx.CreateBuilder().IsEntity()
                    .WithName("TaxId")
                    .Is(sign)
                ;
            var socialNetworksSign = ctx.CreateBuilder().IsEntity()
                    .WithName("SocialNetworkSign")
                    .Is(sign)
                ;


            // Address
            var address = ctx.CreateBuilder().IsEntity()
                    .WithName("Address")
                    .HasRequired(ctx, b =>
                        b.WithName("ZipCode").IsAttribute().HasValueOf(ScalarType.String))
                    .HasRequired(ctx, b =>
                        b.WithName("Region").IsAttribute().HasValueOf(ScalarType.String))
                    .HasRequired(ctx, b =>
                        b.WithName("City").IsAttribute().HasValueOf(ScalarType.String))
                    .HasRequired(ctx, b =>
                        b.WithName("Street").IsAttribute().HasValueOf(ScalarType.String))
                    .HasRequired(ctx, b =>
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
                    .HasRequired(code)
                    .HasRequired(name)
                ;
            var tag = ctx.CreateBuilder().IsEntity()
                    .WithName("Tag")
                    .IsAbstraction()
                    .Is(enumEntity)
                ;
            var accessLevel = ctx.CreateBuilder().IsEntity() // seeded
                    .WithName("AccessLevel")
                    .HasRequired(number)
                    .Is(enumEntity)
                ;

            var applyToAccessLevel = ctx.CreateBuilder().IsEntity() // seeded
                    .WithName("ApplyToAccessLevel")
                    .WithTitle("Форма, на яку подаеться")
                    .HasRequired(number)
                    .HasRequired(ctx, b => b
                        .WithName("Years").IsAttribute().HasValueOf(ScalarType.Integer))
                    .Is(enumEntity)
                ;

            var specialPermitStatus = ctx.CreateBuilder().IsEntity() // seeded
                    .WithName("SpecialPermitStatus")
                    .Is(enumEntity)
                ;

            var accessStatus = ctx.CreateBuilder().IsEntity() // seeded
                    .WithName("AccessStatus")
                    .Is(enumEntity)
                ;

            var controlType = ctx.CreateBuilder().IsEntity() // seeded
                    .WithName("ControlType")
                    .Is(enumEntity)
                ;

            var propertyOwnership = ctx.CreateBuilder().IsEntity() // seeded
                    .WithName("PropertyOwnership")
                    .Is(enumEntity)
                ;

            var sanctionAccessConclusion = ctx.CreateBuilder().IsEntity() // seeded
                    .WithName("SanctionAccessConclusion")
                    .Is(enumEntity)
                ;

            var country = ctx.CreateBuilder().IsEntity() // seeded
                    .WithName("Country")
                    .Is(enumEntity)
                ;

            // Family relations
            var familyRelationKind = ctx.CreateBuilder().IsEntity() // seeded
                    .WithName("FamilyRelationKind")
                    .WithTitle("Ступень родинного зв’язку")
                    .Is(enumEntity)
                ;
            var familyRelationInfo = ctx.CreateBuilder().IsEntity()
                    .WithName("FamilyRelationInfo")
                    .HasRequired(familyRelationKind)
                    .HasRequired(text, "FullName", title: "Прізвище, ім’я та по батькові")
                    .HasRequired(text, "DateAndPlaceOfBirth", title: "Дата та місце народження, громадянство")
                    .HasRequired(text, "WorkPlaceAndPosition", title:"Місце роботи (служби, роботи), посада")
                    .HasRequired(text, "LiveIn", title: "Місце проживання")
                    .HasOptional("Person", "PersonLink")
                ;

            // Entities
            var passport = ctx.CreateBuilder().IsEntity()
                    .WithName("Passport")
                    .HasRequired(code)
                    .HasRequired(ctx, b =>
                        b.WithName("IssueInfo").IsAttribute().HasValueOf(ScalarType.String))
                ;

            var citizenship = ctx.CreateBuilder().IsEntity()
                    .WithName("Citizenship")
                    .HasRequired(country)
                    .HasOptional(taxId)
                    .HasMultiple(passport)
                ;

            // Organization
            var organization = ctx.CreateBuilder()
                    .WithName("Organization")
                    .IsEntity()
                    .HasRequired(taxId)
                    .HasRequired(name)
                    .HasOptional(website)
                    .HasOptional(photo)
                    .HasMultiple(ctx, b =>
                        b.WithName("OrganizationTag").IsEntity().Is(tag))
                    .HasRequired(propertyOwnership)
//                    .HasRequired(ctx, b =>
//                        b.WithName("LegalStatus").IsEntity().Is(enumEntity))
                    .HasRequired(address, "LocatedAt")
                    .HasRequired(address, "RegisteredAt")
                    .HasOptional(address, "BranchAddress")
                    .HasOptional(address, "secretFacilityAddress")
                    .HasOptional(address, "secretFacilityArchiveAddress")
                ;


            // Work in
            var workIn = ctx.CreateBuilder().IsEntity()
                .WithName("WorkIn")
                .HasRequired(organization)
                .HasOptional(ctx, d =>
                    d.WithName("JobPosition").IsAttribute().HasValueOf(ScalarType.String));


            // Person
            var person = ctx.CreateBuilder().IsEntity()
                    .WithName("Person")
                    .HasRequired(name, "FullName", CreateComputed("Join(secondName, firstName, fatherName)"))
                    .HasRequired(firstName)
                    .HasRequired(secondName)
                    .HasRequired(fatherName)
                    .HasOptional(photo)
                    .HasRequired(birthDate)
                    .HasRequired(address, "BirthPlace")
                    .HasRequired(address, "RegistrationPlace")
                    .HasRequired(address, "LivingPlace")
                    .HasMultiple(phoneSign)
                    .HasMultiple(citizenship)
                // ... secret carrier
                    .HasMultiple(workIn)
                    .HasOptional(applyToAccessLevel)
                    .HasOptional(attachment, "ScanForm5")
                    .HasOptional(attachment, "AnswerRules")
                    .HasOptional(attachment, "Autobiography")
                    .HasOptional(attachment, "Form8")
                ;


            // Permits
            var acccess = ctx.CreateBuilder().IsEntity()
                    .WithName("Access")
                    .HasRequired(person, "Person",
                        CreateInversed("Access", "Допуск"))
                    .HasRequired(date, "IssueDate")
                    .HasRequired(date, "EndDate")
                    .HasRequired(accessLevel)
//                    .HasRequired(workIn)
                    .HasRequired(accessStatus) // computed?
                ;

            var organizationPermit = ctx.CreateBuilder().IsEntity()
                    .WithName("SpecialPermit")
                    .HasRequired(organization, "Organization",
                        CreateInversed("SpecialPermit", "Спецдозвiл"))
                    .HasRequired(code, "IssueNumber")
                    .HasRequired(date, "IssueDate")
                    .HasRequired(date, "EndDate")
                    .HasRequired(accessLevel)
                    .HasRequired(specialPermitStatus) // computed?
                    .HasRequired(organization, "SBU") // restrictions?
                ;

        }

        private EntityRelationMeta CreateInversed(string code, string title = null, bool multiple = false) =>
            new EntityRelationMeta { Multiple = multiple,
                Inversed = new InversedRelationMeta { Code = code.ToLowerCamelcase(), Title = title }};

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
