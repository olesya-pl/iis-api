using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class MigrationValueDivider
    {
        private Dictionary<string, Func<string, List<DividedNodePart>>> _library;
        public MigrationValueDivider()
        {
            _library = new Dictionary<string, Func<string, List<DividedNodePart>>>
            {
                { "FuzzyDate", DivideFuzzyDate }
            };
        }
        public List<DividedNodePart> DivideValue(string value, string targetTypeName)
        {
            if (!_library.ContainsKey(targetTypeName)) return null;
            var func = _library[targetTypeName];
            return func(value);
        }

        private List<DividedNodePart> DivideFuzzyDate(string value)
        {
            var date = DateTime.Parse(value);
            return new List<DividedNodePart> 
            { 
                new DividedNodePart("day", date.Day.ToString()),
                new DividedNodePart("month", date.Month.ToString()),
                new DividedNodePart("year", date.Year.ToString())
            };
        }

        //private List<DividedNodePart> DivideFullNameRu(string value)
        //{
        //    var parts = value.Split(' ');
        //    return new List<DividedNodePart>
        //    {
        //        new DividedNodePart("realName.lastName.original", parts.Length > 0 ? parts[0] : string.Empty),
        //        new DividedNodePart("realName.firstName.original", parts.Length > 1 ? parts[1] : string.Empty),
        //        new DividedNodePart("realName.fatherName.original", parts.Length > 2 ? parts[2] : string.Empty)
        //    };
        //}
    }
}
