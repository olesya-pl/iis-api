namespace Iis.Interfaces.Roles
{
    public interface IAccessOperations
    {
        bool Create { get; }
        bool Delete { get; }
        bool Read { get; }
        bool Update { get; }
    }
}