namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public class SatIridiumVoiceRegistrationDateResolver : SeparatedDateTimeResolver, IMaterialRegistrationDateResolver
    {
        private const string DateFormat = "yyyy.MM.dd";

        public SatIridiumVoiceRegistrationDateResolver()
            : base(DateFormat)
        {
        }
    }
}