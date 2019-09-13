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
            var firstName = ctx.CreateBuilder().WithName("FirstName").IsAttribute().HasValueOf(ScalarType.String);
            var secondName = ctx.CreateBuilder().WithName("SecondName").IsAttribute().HasValueOf(ScalarType.String);
            var fatherName = ctx.CreateBuilder().WithName("FatherName").IsAttribute().HasValueOf(ScalarType.String);
            var photo = ctx.CreateBuilder().WithName("Photo").IsAttribute().HasValueOf(ScalarType.File);
            var birthDate = ctx.CreateBuilder().WithName("BirthDate").IsAttribute().HasValueOf(ScalarType.DateTime);
            var birthPlace = ctx.CreateBuilder().WithName("BirthPlace").IsAttribute().HasValueOf(ScalarType.String); // Address?
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
            var emailSign = ctx.CreateBuilder().IsEntity()
                    .WithName("EmailSign")
                    .Is(sign)
                ;
            var ipnSign = ctx.CreateBuilder().IsEntity()
                    .WithName("IPN")
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
                        b.WithName("Region").IsAttribute().HasValueOf(ScalarType.String))
                    .HasRequired(ctx, b =>
                        b.WithName("City").IsAttribute().HasValueOf(ScalarType.String))
                    .HasRequired(ctx, b =>
                        b.WithName("Street").IsAttribute().HasValueOf(ScalarType.String))
                    .HasRequired(ctx, b =>
                        b.WithName("House").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Office").IsAttribute().HasValueOf(ScalarType.String))
                    .HasOptional(ctx, b =>
                        b.WithName("Coordinates").IsAttribute().HasValueOf(ScalarType.Geo))
                ;

            // Enums
            var allowanceForm = ctx.CreateBuilder().IsEntity()
                    .WithName("AllowanceForm")
                    .HasRequired(value)
                ;



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
                    .HasMultiple(phoneSign)
                ;

            var uaPassport = ctx.CreateBuilder().IsEntity()
                    .WithName("UaPassport")
                    .HasRequired(ctx, b =>
                        b.WithName("PassportCode").IsAttribute().HasValueOf(ScalarType.String))
                    .HasRequired(ctx, b =>
                        b.WithName("IssuedBy").IsAttribute().HasValueOf(ScalarType.String))
                ;

            var uaCitizen = ctx.CreateBuilder().IsEntity()
                .WithName("UaCitizen")
                .Is(person)
                .HasOptional(ipnSign)
                .HasRequired(uaPassport);

            var secretCarrier = ctx.CreateBuilder().IsEntity()
                    .WithName("SecretCarrier")
                    .Is(uaCitizen)
                    .HasRequired(allowanceForm)
                    .HasRequired(attachment, "ScanForm5")
                    .HasRequired(attachment, "AnswerRules")
                    .HasRequired(attachment, "Autobiography")
                    .HasRequired(attachment, "Form8")
                ;


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
