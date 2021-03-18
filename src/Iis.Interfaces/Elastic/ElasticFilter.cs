using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public class ElasticFilter
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Suggestion { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public List<string> CherryPickedItems { get; set; } = new List<string>();
        public List<Property> FilteredItems { get; set; } = new List<Property>();
    }

    public class Property
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public Property()
        {
            
        }
        public Property(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}