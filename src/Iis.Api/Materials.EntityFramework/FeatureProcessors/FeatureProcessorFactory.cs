using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class FeatureProcessorFactory : IFeatureProcessorFactory
    {
        public IFeatureProcessor GetInstance(string materialSource)
        {
            return materialSource switch
            {
                "cell.voice" => new GSMFeatureProcessor(),
                _ => new DummyFeatureProcessor()
            };
        }
    }
}