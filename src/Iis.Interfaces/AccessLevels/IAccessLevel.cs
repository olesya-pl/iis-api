using System;

namespace Iis.Interfaces.AccessLevels
{
    public interface IAccessLevel
    {
        Guid Id { get; }
        string Name { get; }
        int NumericIndex { get; }
    }
}