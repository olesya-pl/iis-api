using Iis.Interfaces.Elastic;
using System;
using System.Text.RegularExpressions;

namespace Iis.Elastic
{
    public class GroupedAggregationNameGenerator : IGroupedAggregationNameGenerator
    {
        private const string GroupedAggregationName = "GroupedAggregation";
        private const string GroupedAggregationNameRegex = "GroupedAggregation\\S{32}$";

        public static bool IsGroupedAggregateName(string name) => Regex.IsMatch(name, GroupedAggregationNameRegex);
        public string GetUniqueAggregationName() => $"{GroupedAggregationName}{Guid.NewGuid().ToString("N")}";
    }
}