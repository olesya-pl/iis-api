namespace Iis.Interfaces.Roles
{
    public interface IAccessGranted
    {
        bool CreateGranted { get; }
        bool DeleteGranted { get; }
        AccessKind Kind { get; }
        AccessCategory Category { get; }
        string Title { get; }
        bool ReadGranted { get; }
        bool UpdateGranted { get; }
    }
}