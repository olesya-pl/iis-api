using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IGsmLocationService
    {
        Task TryFillTowerLocationHistory(JObject metadata, Guid materialId);
    }
}