using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Materials
{
    public interface IMaterialLoadData
    {
        string Code { get; }
        string Coordinates { get; }
        string From { get; }
        string LoadedBy { get; }
        IEnumerable<string> Objects { get; }
        DateTime ReceivingDate { get; }
        IEnumerable<string> States { get; }
        IEnumerable<string> Tags { get; }
    }
}