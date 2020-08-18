namespace Iis.Services.Contracts.Interfaces
{
    public interface ISanitizeService
    {
        string SanitizeBody(string body);
    }
}