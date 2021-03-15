namespace Iis.Services.Contracts.Interfaces
{
    public interface IConnectionStringService
    {
        string GetConnectionString(string name, string prefix);
        string GetFlightRadarConnectionString();
        string GetIisApiConnectionString();
    }
}