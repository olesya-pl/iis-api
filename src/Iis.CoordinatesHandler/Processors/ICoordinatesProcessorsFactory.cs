namespace Iis.CoordinatesEventHandler.Processors
{
    public interface ICoordinatesProcessorsFactory
    {
        ICoordinatesProcessor GetProcessor(string materialSource, string materialType);
    }
}