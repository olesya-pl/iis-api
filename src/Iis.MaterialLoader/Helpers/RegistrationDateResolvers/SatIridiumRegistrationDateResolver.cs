namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public class SatIridiumRegistrationDateResolver : SeparatedDateTimeResolver, IMaterialRegistrationDateResolver
    {
        private const string DateFormat = "yyyy.MM.dd";

        public SatIridiumRegistrationDateResolver()
            : base(DateFormat)
        {
        }
    }
}