using System;
using System.Collections.Generic;

namespace Iis.Interfaces.AccessLevels
{
    public interface IAccessLevels
    {
        List<AccessLevel> Items { get; }
        bool IndexIsValid(int numericIndex);
        AccessLevel GetItemById(Guid id);
        AccessLevel GetItemByNumericIndex(int numericIndex);
    }
}