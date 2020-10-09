using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologySchema
{
    public class DotName: IDotName
    {
        public string Value { get; }

        IReadOnlyList<string> _parts;
        public IReadOnlyList<string> Parts =>
            _parts ?? (_parts = Value.Split('.').ToList());
        public DotName(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException();
            Value = value;
        }
        public IDotName Concat(IDotName another)
        {
            return new DotName(Value + "." + another.Value);
        }
        public override string ToString()
        {
            return Value;
        }
    }
}
