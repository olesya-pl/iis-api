namespace IIS.Core
{
    public interface IInstance : IOntologyNode
    {
        new IType Schema { get; }
        bool IsTypeOf(IType schema);
    }
}
