namespace Iis.Services.Contracts.ExternalUserServices
{
    public class ExternalRole
    {
        public string Name { get; set; }
        public override string ToString() => Name;

        public static ExternalRole CreateFrom(string name) =>
            new ExternalRole { Name = name };
    }
}