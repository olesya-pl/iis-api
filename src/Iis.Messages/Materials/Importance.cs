using System;

namespace Iis.Messages.Materials
{
    [Flags]
    public enum Importance
    {
        Critical,
        High,
        Medium,
        Normal,
        Low,
        Ignore
    }
}