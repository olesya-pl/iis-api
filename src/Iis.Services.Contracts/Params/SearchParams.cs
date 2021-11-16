using System;
using System.Collections.Generic;
using Iis.Interfaces.Common;
using Iis.Interfaces.Elastic;

namespace Iis.Services.Contracts.Params
{
    public class SearchParams
    {
        public string Suggestion { get; set; }
        public PaginationParams Page {get;set;}
        public SortingParams Sorting { get; set; }
        public IReadOnlyCollection<CherryPickedItem> CherryPickedItems { get; set; } = new List<CherryPickedItem>();
        public IReadOnlyCollection<Property> FilteredItems { get; set; } = new List<Property>();
        public DateRange CreatedDateRange { get; set; }
    }
}