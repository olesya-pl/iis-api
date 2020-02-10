using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class MigrationValueDevider
    {
        private Dictionary<string, Func<string, List<DevidedNodePart>>> _library;
        public MigrationValueDevider()
        {
            _library = new Dictionary<string, Func<string, List<DevidedNodePart>>>
            {
                {  "FuzzyDate", DevideFuzzyDate }
            };
        }
        public List<DevidedNodePart> DevideValue(string value, string targetTypeName)
        {
            if (!_library.ContainsKey(targetTypeName)) return null;
            var func = _library[targetTypeName];
            return func(value);
        }

        private List<DevidedNodePart> DevideFuzzyDate(string value)
        {
            var date = DateTime.Parse(value);
            return new List<DevidedNodePart> 
            { 
                new DevidedNodePart("day", date.Day.ToString()),
                new DevidedNodePart("month", date.Day.ToString()),
                new DevidedNodePart("year", date.Day.ToString())
            };
        }
    }
}
