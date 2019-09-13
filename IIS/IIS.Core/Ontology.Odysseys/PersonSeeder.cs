using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology.Odysseys
{
    public class PersonSeeder : IOntologyProvider
    {
        public OntologyBuildContext BuildPersonTypes(OntologyBuildContext ctx)
        {
            // Attributes - title and meta omitted
            var name = ctx.CreateBuilder().WithName("Name").IsAttribute().HasValueOf(ScalarType.String);
            var code = ctx.CreateBuilder().WithName("Code").IsAttribute().HasValueOf(ScalarType.String);
            var firstName = ctx.CreateBuilder().WithName("FirstName").IsAttribute().HasValueOf(ScalarType.String);
            var secondName = ctx.CreateBuilder().WithName("SecondName").IsAttribute().HasValueOf(ScalarType.String);
            var fatherName = ctx.CreateBuilder().WithName("FatherName").IsAttribute().HasValueOf(ScalarType.String);
            var photo = ctx.CreateBuilder().WithName("Photo").IsAttribute().HasValueOf(ScalarType.File);
            var birthDate = ctx.CreateBuilder().WithName("BirthDate").IsAttribute().HasValueOf(ScalarType.DateTime);
            var date = ctx.CreateBuilder().WithName("Date").IsAttribute().HasValueOf(ScalarType.DateTime);
            var attachment = ctx.CreateBuilder().WithName("Attachment").IsAttribute().HasValueOf(ScalarType.File);


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
            var allowanceForm = ctx.CreateBuilder().IsEntity()
                    .WithName("AllowanceForm")
                    .HasRequired(value)
                ;

            var permitState = ctx.CreateBuilder().IsEntity()
                    .WithName("PermitState")
                    .HasRequired(value)
                ;

            var country = ctx.CreateBuilder().IsEntity()
                    .WithName("Country")
                    .HasRequired(name)
                ;

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


            // Work in
            var workIn = ctx.CreateBuilder().IsEntity()
                .WithName("WorkIn")
                .HasRequired("Company")
                .HasOptional(ctx, d =>
                    d.WithName("JobPosition").IsAttribute().HasValueOf(ScalarType.String));


            // Entities
            var person = ctx.CreateBuilder().IsEntity()
                    .WithName("Person")
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
                    .HasRequired(allowanceForm)
                    .HasRequired(attachment, "ScanForm5")
                    .HasRequired(attachment, "AnswerRules")
                    .HasRequired(attachment, "Autobiography")
                    .HasRequired(attachment, "Form8")
                ;


            var permit = ctx.CreateBuilder().IsEntity()
                .WithName("Permit")
                .HasRequired(person)
                .HasRequired(code)
                .HasRequired(date, "IssueDate")
                .HasRequired(date, "EndDate")
                .HasRequired(allowanceForm)
                .HasRequired(workIn)
                .HasRequired(permitState) // computed?
                .HasRequired("Company", "SBU"); // restrictions?


            // ------ STUBS ------ //
            ctx.CreateBuilder().IsEntity().WithName("Company").HasRequired(name);

            return ctx;
        }

        public Task<Ontology> GetOntologyAsync(CancellationToken cancellationToken = default)
        {
            var ctx = new OntologyBuildContext();
            var types = BuildPersonTypes(ctx).BuildOntology();
            return Task.FromResult(new Ontology(types));
        }
    }
}
