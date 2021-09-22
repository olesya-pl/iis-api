namespace Iis.CoordinatesEventHandler.Processors
{
    public class CoordinatesProcessorsFactory : ICoordinatesProcessorsFactory
    {
        public ICoordinatesProcessor GetProcessor(string materialSource, string materialType)
        {
            return (materialSource, materialType) switch
            {
                ("contour.doc", "document") => new ContourDocCoordinatesProcessor(),
                (_, _) => new DummyCoordinatesProcessor()
            };
        }
    }
}