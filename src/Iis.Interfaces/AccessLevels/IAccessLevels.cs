using System;
using System.Collections.Generic;

namespace Iis.Interfaces.AccessLevels
{
    public interface IAccessLevels
    {
        IReadOnlyList<IAccessLevel> Items { get; }
        bool IndexIsValid(int numericIndex);
        IAccessLevel GetItemById(Guid id);
        IAccessLevel GetItemByNumericIndex(int numericIndex);
    }
}