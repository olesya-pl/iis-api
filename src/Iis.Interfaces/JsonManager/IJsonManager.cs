using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.JsonManager
{
    public interface IJsonManager
    {
        Dictionary<string, string> ConvertToKeyValues(string json);
    }
}
