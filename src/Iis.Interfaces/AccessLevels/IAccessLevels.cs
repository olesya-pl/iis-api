using System.Collections.Generic;

namespace Iis.Interfaces.AccessLevels
{
    public interface IAccessLevels
    {
        IReadOnlyList<AccessLevel> Items { get; }
        bool IndexIsValid(int numericIndex);
    }
}