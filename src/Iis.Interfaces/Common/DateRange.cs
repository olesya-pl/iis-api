using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Common
{
    public class DateRange
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public DateRange() { }
        public DateRange(DateTime? from, DateTime? to)
        {
            From = from;
            To = to;
        }
        public bool IsEmpty => From == null && To == null;
    }
}
