using System.Collections.Generic;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IAutocompleteService
    {
        List<string> GetTips(string query, int count);
    }
}