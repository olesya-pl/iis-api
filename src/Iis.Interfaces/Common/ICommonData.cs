using Iis.Interfaces.AccessLevels;

namespace Iis.Interfaces.Common
{
    public interface ICommonData
    {
        IAccessLevels AccessLevels { get; }
    }
}