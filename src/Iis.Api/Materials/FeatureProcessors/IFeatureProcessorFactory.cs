namespace IIS.Core.Materials.FeatureProcessors
{
    /// <summary>
    /// Defines factory that creates FeatureProcessor
    /// </summary>
    public interface IFeatureProcessorFactory
    {
        /// <summary>
        /// Creates instance of FeatureProcessor by material Source and Type keys 
        /// </summary>
        IFeatureProcessor GetInstance(string materialSource, string materialType); 
    }
}