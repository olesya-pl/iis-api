using Iis.MaterialLoader.Constants;

namespace Iis.MaterialLoader.Helpers.RegistrationDateResolvers
{
    public static class RegistrationDateResolverFactory
    {
        public static IMaterialRegistrationDateResolver Create(string type, string source)
        {
            return (type, source) switch
            {
                (Material.Types.Audio, Material.Sources.CellVoice) => new CellVoiceRegistrationDateResolver(),
                (Material.Types.Audio, Material.Sources.SatVoice) => new SatVoiceRegistrationDateResolver(),
                (Material.Types.Audio, Material.Sources.SatIridiumVoice) => new SatIridiumVoiceRegistrationDateResolver(),
                (Material.Types.Audio, Material.Sources.HfVoice) => new HfVoiceRegistrationDateResolver(),
                (_, _) => new DummyRegistrationDateResolver()
            };
        }
    }
}