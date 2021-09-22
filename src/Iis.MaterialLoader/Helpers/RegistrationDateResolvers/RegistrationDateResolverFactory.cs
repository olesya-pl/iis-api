using Iis.MaterialLoader.Constants;

namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public static class RegistrationDateResolverFactory
    {
        public static IMaterialRegistrationDateResolver Create(string source)
        {
            return (source) switch
            {
                Material.Sources.CellVoice => new CellVoiceRegistrationDateResolver(),
                Material.Sources.SatVoice => new SatVoiceRegistrationDateResolver(),
                Material.Sources.SatIridiumVoice => new SatIridiumVoiceRegistrationDateResolver(),
                Material.Sources.HfVoice => new HfVoiceRegistrationDateResolver(),
                _ => new DummyRegistrationDateResolver()
            };
        }
    }
}