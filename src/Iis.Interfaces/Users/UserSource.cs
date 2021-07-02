using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Users
{
    public enum UserSource
    {
        Internal = 0,
        ActiveDirectory = 1,
        Dummy = 2
    }
}
