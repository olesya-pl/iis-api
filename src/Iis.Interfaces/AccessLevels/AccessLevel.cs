using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.AccessLevels
{
    public class AccessLevel
    {
        public Guid Id { get; }
        public string Name { get; }
        public int NumericIndex { get; }
        public AccessLevel(Guid id, string name, int numericIndex)
        {
            Id = Id;
            Name = name;
            NumericIndex = numericIndex;
        }
    }
}
