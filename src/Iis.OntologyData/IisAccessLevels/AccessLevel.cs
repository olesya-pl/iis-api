using Iis.Interfaces.AccessLevels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.IisAccessLevels
{
    public class AccessLevel: IAccessLevel
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public int NumericIndex { get; set; }
        public AccessLevel(Guid id, string name, int numericIndex)
        {
            Id = id;
            Name = name;
            NumericIndex = numericIndex;
        }
    }
}
