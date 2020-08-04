using System.Collections.Generic;

namespace Iis.Services
{
    public interface IAutocompleteService
    {
        List<string> GetTips(string query, int count);
    }
}