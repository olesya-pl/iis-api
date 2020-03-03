using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticNodeFilter
    {
        int Limit { get; set; }
        int Offset { get; set; }
        string Suggestion { get; set; }
    }
}