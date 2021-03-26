using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.AccessLevels
{
    public class AccessLevel : IAccessLevel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int NumericIndex { get; set; }
        public AccessLevel() { }
        public AccessLevel(Guid id, string name, int numericIndex)
        {
            Id = id;
            Name = name;
            NumericIndex = numericIndex;
        }
        public AccessLevel(IAccessLevel accessLevel)
        {
            Id = accessLevel.Id;
            Name = accessLevel.Name;
            NumericIndex = accessLevel.NumericIndex;
        }
    }
}
