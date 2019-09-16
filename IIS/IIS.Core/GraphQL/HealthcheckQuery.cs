using System.Reflection;

namespace IIS.Core.GraphQL
{
    public class HealthcheckQuery
    {
        public string GetVersion()
        {
            return Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
        }
    }
}
