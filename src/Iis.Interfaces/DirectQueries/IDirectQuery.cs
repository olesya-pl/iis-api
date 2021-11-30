using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.DirectQueries
{
    public interface IDirectQuery
    {
        IDirectQuery SetParameter(string name, string value);
        string GetFinalSql();
    }
}
