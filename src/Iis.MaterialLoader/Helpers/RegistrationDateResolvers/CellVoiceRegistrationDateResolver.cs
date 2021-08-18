namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public class CellVoiceRegistrationDateResolver : SinglePropertyRegistrationDateResolver, IMaterialRegistrationDateResolver
    {
        private const string DateFormat = "dd.MM.yyyy, HH:mm:ss";
        private const string PropertyName = "regTime";

        public CellVoiceRegistrationDateResolver()
            : base(PropertyName, DateFormat)
        {
        }
    }
}