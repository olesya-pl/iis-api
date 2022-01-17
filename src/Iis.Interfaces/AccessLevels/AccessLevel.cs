using System;

namespace Iis.Interfaces.AccessLevels
{
    public class AccessLevel : IAccessLevel
    {
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
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int NumericIndex { get; set; }

    }
}
