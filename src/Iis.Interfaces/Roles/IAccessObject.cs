namespace Iis.Interfaces.Roles
{
    public interface IAccessObject
    {
        IAccessOperations AllowedOperations { get; set; }
        AccessCategory Category { get; set; }
        AccessKind Kind { get; set; }
        string Title { get; set; }
    }
}