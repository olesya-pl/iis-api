using System;
namespace Iis.Interfaces.Materials
{
    public interface IMLResponseEntity
    {
        Guid Id { get; set; }
        Guid MaterialId { get; set; }
        string MLHandlerName { get; set; }
        string OriginalResponse { get; set; }
    }
}