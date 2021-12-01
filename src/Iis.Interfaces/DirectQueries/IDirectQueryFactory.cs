using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.DirectQueries
{
    public interface IDirectQueryFactory
    {
        IDirectQuery GetDirectQuery(string queryFileName);
    }
}
